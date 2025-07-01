using System.Collections.Generic;
using System.Text.RegularExpressions;
using Jellyfin.Data.Enums;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.TubeArchivistSponsorblock.Configuration;

/// <summary>
/// Plugin configuration.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Gets or sets the Tube Archivist collection title.
    /// </summary>
    public string TubeArchivistCollectionTitle { get; set; } = "YouTube";

    /// <summary>
    /// Gets or sets the Tube Archivist host URL.
    /// </summary>
    public string TubeArchivistHostUrl { get; set; } = "http://127.0.0.1:8000/";

    /// <summary>
    /// Gets or sets the Tube Archivist API key.
    /// </summary>
    public string TubeArchivistAPIKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user provided regex text for Intros.
    /// </summary>
    public string? CustomMappingIntro { get; set; } = "intro";

    /// <summary>
    /// Gets or sets the user provided regex text for Previews.
    /// </summary>
    public string? CustomMappingPreview { get; set; } = "preview|filler";

    /// <summary>
    /// Gets or sets the user provided regex text for Recaps.
    /// </summary>
    public string? CustomMappingRecap { get; set; } = "interaction|selfpromo";

    /// <summary>
    /// Gets or sets the user provided regex text for Outros.
    /// </summary>
    public string? CustomMappingOutro { get; set; } = "outro";

    /// <summary>
    /// Gets or sets the user provided regex text for Commercials.
    /// </summary>
    public string? CustomMappingCommercial { get; set; } = "sponsor";

    /// <summary>
    /// Gets the regular expressions with a mapping of their respective Segment types.
    /// </summary>
    /// <returns>A list of regexes with their respective segment types.</returns>
    public IReadOnlyList<(MediaSegmentType Type, string? Regex)> Patterns()
    {
        return
        [
            (MediaSegmentType.Intro, CustomMappingIntro),
            (MediaSegmentType.Commercial, CustomMappingCommercial),
            (MediaSegmentType.Preview, CustomMappingPreview),
            (MediaSegmentType.Recap, CustomMappingRecap),
            (MediaSegmentType.Outro, CustomMappingOutro),
        ];
    }
}

/* ORIGINAL TEMPLATE
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.TubeArchivistSponsorblock.Configuration;

/// <summary>
/// The configuration options.
/// </summary>
public enum SomeOptions
{
    /// <summary>
    /// Option one.
    /// </summary>
    OneOption,

    /// <summary>
    /// Second option.
    /// </summary>
    AnotherOption
}

/// <summary>
/// Plugin configuration.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
    /// </summary>
    public PluginConfiguration()
    {
        // set default options here
        Options = SomeOptions.AnotherOption;
        TrueFalseSetting = true;
        AnInteger = 2;
        AString = "string";
    }

    /// <summary>
    /// Gets or sets a value indicating whether some true or false setting is enabled..
    /// </summary>
    public bool TrueFalseSetting { get; set; }

    /// <summary>
    /// Gets or sets an integer setting.
    /// </summary>
    public int AnInteger { get; set; }

    /// <summary>
    /// Gets or sets a string setting.
    /// </summary>
    public string AString { get; set; }

    /// <summary>
    /// Gets or sets an enum option.
    /// </summary>
    public SomeOptions Options { get; set; }
}
 */
