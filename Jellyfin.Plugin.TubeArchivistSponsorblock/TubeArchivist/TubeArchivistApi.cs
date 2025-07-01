using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Jellyfin.Plugin.TubeArchivistSponsorblock.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Jellyfin.Plugin.TubeArchivistSponsorblock.TubeArchivist;

/// <summary>
/// Class to interact with TubeArchivist API.
/// </summary>
public class TubeArchivistApi
{
    private readonly ILogger<TubeArchivistApi> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="TubeArchivistApi"/> class.
    /// </summary>
    /// <param name="logger">Instance of the <see cref="ILogger"/> interface.</param>
    /// /// <param name="httpClientFactory">Instance of the <see cref="IHttpClientFactory"/> interface.</param>
    public TubeArchivistApi(
        ILogger<TubeArchivistApi> logger,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    private async Task<string?> GetDataFromApi(string endpoint)
    {
        // Get instance config
        var config = TubeArchivistSponsorblockPlugin.Instance!.Configuration;
        // Create HttpClient from the factory
        var httpClient = _httpClientFactory.CreateClient();
        // Add API token from config.TubeArchivistAPIKey to the Authorization header
        string apiKey = config.TubeArchivistAPIKey;
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", apiKey);
        // httpClient.DefaultRequestHeaders.Add("Authorization", $"Token {config.TubeArchivistAPIKey}");
        _logger.LogDebug("GetDataFromApi() API Key - {String}", apiKey);
        // Concatenate the base URL with the endpoint
        string baseUrl = config.TubeArchivistHostUrl;  // Assuming the base URL is configured in the settings
        string fullUrl = $"{baseUrl}{endpoint}";
        // Make the request
        try
        {
            HttpResponseMessage response = await httpClient.GetAsync(fullUrl).ConfigureAwait(true);
            // Check for successful response
            if (response.IsSuccessStatusCode)
            {
                // Return the raw response content as a string
                var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                _logger.LogDebug("GetDataFromApi() succeeded - {String}", responseContent);
                return responseContent;
            }
            else
            {
                // Log error if the response is not successful
                _logger.LogInformation(
                    "GetDataFromApi() [{Url}] failed with status {StatusCode} - {ReasonPhrase}",
                    fullUrl,
                    response.StatusCode,
                    response.ReasonPhrase);
                return null;
            }
        }
        catch (Exception ex)
        {
            // Log exception details
            _logger.LogInformation(ex, "GetDataFromApi() [{Url}]- {Exception}", fullUrl, ex);
            return null;
        }
    }

    /// <summary>
    /// Retrieves the raw video info from TubeArchivist.
    /// </summary>
    /// <param name="videoId">YouTube video id to query.</param>
    /// <returns>A task.</returns>
    public async Task<string?> GetRawVideoData(string videoId)
    {
        // Dummy response contents
        // // TEST: null -- passed
        // string? rawData = null;
        // TEST: empty -- passed
        // string? rawData = string.Empty;
        // // TEST: valid -- passed
        // string rawData = @"{
        //     ""sponsorblock"": {
        //         ""segments"":[{
        //             ""actionType"":""skip"",
        //             ""segment"":[248.621,311.231],
        //             ""votes"":3,
        //             ""category"":""sponsor""
        //         }]
        //     }
        // }";
        // // TEST: invalid -- passed
        // string rawData = "fartnoise";
        // // DEBUG - Dummy await
        // await Task.CompletedTask.ConfigureAwait(true);
        // Call and await the API
        var endpoint = $"api/video/{videoId}/";
        string? rawData = await GetDataFromApi(endpoint).ConfigureAwait(true);
        _logger.LogDebug("GetRawVideoData() {String}", rawData);
        return rawData;
    }

    /// <summary>
    /// Retrive sponsorblock segments from TubeArchivist.
    /// </summary>
    /// <param name="videoId">YouTube video id to query.</param>
    /// <returns>A task.</returns>
    public async Task<IReadOnlyList<SponsorblockSegment>> GetVideoSponsorblockSegments(string videoId)
    {
        // Call the API
        string? rawData = await GetRawVideoData(videoId).ConfigureAwait(true);
        _logger.LogDebug("GetVideoSponsorblockSegments() {0}", rawData);
        // return new List<SponsorblockSegment>();  // DEBUG - Return an empty list

        if (string.IsNullOrEmpty(rawData))
        {
            _logger.LogDebug("Raw data is empty or null.");
            return new List<SponsorblockSegment>(); // Return an empty list instead of null
        }

        try
        {
            // Deserialize the raw data to a JObject (dynamic)
            var jsonObject = JsonConvert.DeserializeObject<JObject>(rawData);

            // Check if 'sponsorblock' exists and contains the 'segments'
            var sponsorblockSegments = jsonObject?["sponsorblock"]?["segments"];
            if (sponsorblockSegments == null || !sponsorblockSegments.HasValues)
            {
                _logger.LogDebug("No sponsorblock segments found.");
                return new List<SponsorblockSegment>(); // Return an empty list if no segments
            }

            // Ensure sponsorblockSegments is a JArray
            var segmentArray = sponsorblockSegments as JArray;
            // Parse the segments if it is a valid JArray
            if (segmentArray != null)
            {
                var segments = segmentArray
                .Select(segment =>
                {
                    // Parse category
                    var category = segment["category"]?.ToString() ?? string.Empty;
                    // Parse segment times and convert from float ms to ticks
                    var scalar = 1e7;
                    var segmentTimes = segment["segment"]?
                        .ToObject<List<double>>()?
                        .Select(time => (long)(time * scalar))
                        .ToList() ?? new List<long>();
                    _logger.LogDebug(
                        "GetVideoSponsorblockSegments() [{Category}] {StartTime} to {EndTime}",
                        category,
                        segmentTimes[0],
                        segmentTimes[1]);
                    return new SponsorblockSegment(category, segmentTimes);
                })
                .ToList();
                return segments;
            }

            _logger.LogDebug("GetVideoSponsorblockSegments() Failed to parse sponsorblock segments.");
            return new List<SponsorblockSegment>();
        }
        catch (JsonException ex)
        {
            _logger.LogInformation("GetVideoSponsorblockSegments() Error parsing sponsorblock data: {0}", ex.Message);
            return new List<SponsorblockSegment>();
        }
    }
}
