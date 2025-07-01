using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Jellyfin.Plugin.TubeArchivistSponsorblock.TubeArchivist;

/// <summary>
/// Initializes a new instance of the <see cref="SponsorblockSegment"/> class.
/// </summary>
public class SponsorblockSegment
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SponsorblockSegment"/> class.
    /// </summary>
    /// <param name="category"> is the name of the segment type.</param>
    /// <param name="segmentTimes"> are the start and end ticks of the segment.</param>
    public SponsorblockSegment(string category, IReadOnlyList<long> segmentTimes)
    {
        Category = category;
        SegmentTimes = segmentTimes;
    }

    /// <summary>
    /// Gets or sets the category of the sponsorblock segment.
    /// </summary>
    [JsonProperty("category")]
    public string Category { get; set; } = null!;

    /// <summary>
    /// Gets the segment start and end ticks of the sponsorblock segment.
    /// </summary>
    [JsonProperty("segment")]
    public IReadOnlyList<long> SegmentTimes { get; } = new List<long>();
}
