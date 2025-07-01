using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using Jellyfin.Plugin.TubeArchivistSponsorblock.Configuration;
using Jellyfin.Plugin.TubeArchivistSponsorblock.TubeArchivist;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Model;
using MediaBrowser.Model.MediaSegments;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TubeArchivistSponsorblock.Providers;

/// <inheritdoc />
public class SponsorblockMediaSegmentProvider : IMediaSegmentProvider
{
    private readonly ILogger<SponsorblockMediaSegmentProvider> _logger;
    private readonly IItemRepository _itemRepository;
    private readonly ILibraryManager _libraryManager;
    private readonly TubeArchivistApi _tubeArchivistApi;

    /// <summary>
    /// Initializes a new instance of the <see cref="SponsorblockMediaSegmentProvider"/> class.
    /// </summary>
    /// <param name="itemRepository">Instance of the <see cref="IItemRepository"/> interface.</param>
    /// <param name="libraryManager">Instance of the <see cref="ILibraryManager"/> interface.</param>
    /// <param name="logger">Instance of the <see cref="ILogger"/> interface.</param>
    /// <param name="tubeArchivistApi">Instance of the <see cref="TubeArchivistApi"/> interface.</param>
    public SponsorblockMediaSegmentProvider(
        IItemRepository itemRepository,
        ILibraryManager libraryManager,
        ILogger<SponsorblockMediaSegmentProvider> logger,
        TubeArchivistApi tubeArchivistApi)
    {
        _logger = logger;
        _itemRepository = itemRepository;
        _libraryManager = libraryManager;
        _tubeArchivistApi = tubeArchivistApi;
    }

    /// <inheritdoc />
    public string Name => "TubeArchivist Sponsorblock Segments Provider";

    /// <inheritdoc />
    public ValueTask<bool> Supports(BaseItem item)
    {
        var config = TubeArchivistSponsorblockPlugin.Instance!.Configuration;
        var itemCollectionFolders = _libraryManager.GetCollectionFolders(item);
        var itemLibraryName = itemCollectionFolders.FirstOrDefault()?.Name;
        if (config.TubeArchivistCollectionTitle == itemLibraryName)
        {
            _logger.LogDebug("Supports() [0] {0} supported", item.Name);
            return new ValueTask<bool>(true);
        }
        else
        {
            _logger.LogDebug("Supports() [!] {0} not supported", item.Name);
            return new ValueTask<bool>(false);
        }
    }

    private MediaSegmentType? GetMediaSegmentType(string name)
    {
        var mappings = TubeArchivistSponsorblockPlugin.Instance?.Configuration.Patterns();

        foreach (var item in mappings!.Where(e => !string.IsNullOrWhiteSpace(e.Regex)))
        {
            if (!string.IsNullOrEmpty(item.Regex)
                && Regex.IsMatch(name, item.Regex, RegexOptions.IgnoreCase | RegexOptions.Singleline))
            {
                return item.Type;
            }
        }

        return null;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<MediaSegmentDto>> GetMediaSegments(MediaSegmentGenerationRequest request, CancellationToken cancellationToken)
    {
        // Get Video ID from path name for the API call
        var config = TubeArchivistSponsorblockPlugin.Instance!.Configuration;
        var item = _itemRepository.RetrieveItem(request.ItemId);
        var videoId = Path.GetFileNameWithoutExtension(item.Path);
        _logger.LogDebug(
            "GetMediaSegments() [{Name}] (videoId: {Id}) ({Path})",
            item.Name,
            videoId,
            item.Path);

        // Don't process item if it is not a mediaItem
        if (item is not IHasMediaSources mediaItem)
        {
            return Task.FromResult<IReadOnlyList<MediaSegmentDto>>(Array.Empty<MediaSegmentDto>());
        }

        // Make blocking API call to retrieve Sponsorblock segments from Tube Archivist
        var sponsorblockSegments = _tubeArchivistApi.GetVideoSponsorblockSegments(videoId).GetAwaiter().GetResult();
        if (sponsorblockSegments.Count > 0)
        {
            _logger.LogDebug(
                "GetMediaSegments() [{Name}] (videoId: {Id}) Found {Count} sponsorblock segments.",
                item.Name,
                videoId,
                sponsorblockSegments.Count);
        }

        // Process the segments
        var segments = new List<MediaSegmentDto>(sponsorblockSegments.Count);
        foreach (var sponsorblockSegment in sponsorblockSegments)
        {
            var category = sponsorblockSegment.Category;
            var type = GetMediaSegmentType(category);

            if (type.HasValue)
            {
                segments.Add(new MediaSegmentDto
                {
                    Id = Guid.NewGuid(),
                    ItemId = item.Id,
                    Type = type.Value,
                    StartTicks = sponsorblockSegment.SegmentTimes[0],
                    EndTicks = sponsorblockSegment.SegmentTimes[1]
                });
            }

            // Readback the results
            _logger.LogDebug(
            "GetMediaSegments() [{Category} = {Type}] {StartTime} to {EndTime}",
            category,
            type,
            sponsorblockSegment.SegmentTimes[0],
            sponsorblockSegment.SegmentTimes[1]);
        }

        // var segments = new List<MediaSegmentDto>(Array.Empty<MediaSegmentDto>());  // DEBUG  - empty list return

        /*
        var chapters = _itemRepository.GetChapters(item);
        if (chapters.Count == 0)
        {
            // No chapters, so nothing to parse.
            return Task.FromResult<IReadOnlyList<MediaSegmentDto>>(Array.Empty<MediaSegmentDto>());
        }

        var segments = new List<MediaSegmentDto>(chapters.Count);

        for (var index = 0; index < chapters.Count; index++)
        {
            var chapterInfo = chapters[index];
            var nextChapterInfo = index + 1 < chapters.Count ? chapters[index + 1] : null;

            if (string.IsNullOrEmpty(chapterInfo.Name))
            {
                continue;
            }

            var type = GetMediaSegmentType(chapterInfo.Name);

            if (type.HasValue)
            {
                _logger.LogInformation("GetMediaSegments() {Name} ({Path})", item.Name, item.Path);
                segments.Add(new MediaSegmentDto
                {
                    Id = Guid.NewGuid(),
                    ItemId = item.Id,
                    Type = type.Value,
                    StartTicks = chapterInfo.StartPositionTicks,
                    EndTicks = nextChapterInfo?.StartPositionTicks ?? mediaItem.RunTimeTicks ?? chapterInfo.StartPositionTicks
                });
            }
        }
        */

        return Task.FromResult<IReadOnlyList<MediaSegmentDto>>(segments);
    }
}
