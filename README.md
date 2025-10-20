# Introduction

<p align="center">
<img alt="Plugin Banner" src="https://raw.githubusercontent.com/Atinoda/jellyfin-plugin-tubearchivist-sponsorblock/master/assets/TubeArchivistSponsorblock.png"/>
<br/>
</p>

This Jellyfin plugin integrates the [Sponsorblock](https://sponsor.ajay.app/) functionality of [Tube Archivist](https://github.com/tubearchivist/tubearchivist) with Jellyfin's [Media Segments](https://jellyfin.org/docs/general/server/metadata/media-segments/) feature. It enables automatic skipping of sponsor segments while watching YouTube in Jellyfin. **This plugin does not implement the metadata provider functionality of the existing TubeArchivist Jellyfin plugin.**

*That's right... you need both installed. The original intention was to extend the functionality of the [existing Tube Archivist plugin](https://github.com/tubearchivist/tubearchivist-jf-plugin), but unfortunately - for a long list of boring technical reasons - this was not possible.*

# Quick-start

## Install
- Add this repo in Jellyfin Dashboard -> Catalogue -> Settings Cog -> Repositories -> +

    - **Repository Name**: `TubeArchivist Sponsorblock` *(or whatever you like)*
    - **Repository URL**: `https://raw.githubusercontent.com/Atinoda/jellyfin-plugin-tubearchivist-sponsorblock/master/manifest.json`

- Install the plugin from Jellyfin Dashboard -> Catalogue -> Metadata -> TubeArchivist Sponsorblock

## Configure
- Configure the Tube Archivist Settings:
    - **Collection Title:** **must** match the library name that is managed by tube archivist
    - **Host URL:** **must** be formatted starting with `http://` and ending with a single slash `/`, e.g., `http://127.0.0.1:8000/`
    - **API Key:** not strictly required, but may be used in the future
- *(Optional)* Customise the Sponsorblock Segments to Jellyfin Segments mapping

## Enjoy
**You only have one chance to program segments into each piece of media.** Read the Notes section.
- Run Jellyfin Dashboard > Scheduled Tasks -> Media Segment Scan.
- Remember to set segment actions (Skip, Ask to Skip, None) in the Jellyfin settings
- *Note:* Most Jellyfin clients do not yet support segment actions. However, they are supported by the Web Client / Player, and the Android TV app.

# Notes

## You only get one shot
If Jellyfin maps segments to a piece of media, it will never query segments for that media again, unless an overwrite command is sent. While there is a mechanism to ask for an overwrite, it is not implemented here. It may be in the future.

## You get as many shots as you like
Jellyfin stores its media segments in a dedicated database table. The database is a sqlite file, and is stored in `jellyfin.db` The media segments table is called `MediaSegments`.

Hypothetically, if a person was developing and testing a jellyfin plugin, they might want to delete all rows in that table so that the scan can run again.

- They might back up the server config, shut it down, edit that database with a sqlite tool, and then restart the server.
- They might have used the command `DELETE FROM MediaSegments;` to clear the table.
- They might even have got impatient with that process and deleted the table contents on a running and live server (which had a **FULL BACKUP AVAILABLE**), then immediately run the segment scan task again. This procedure may have caused absolutely no issues at all, because the `MediaSegments` table appears to be relatively standalone.

Hypothetically a user of this plugin could delete the contents of the segments table if they somehow had a problem and wanted to re-add segments.

- The user would fully realise that the plugin author does not support this, and that they are on their own in this exciting self-hosting adventure. They would know that the author would direct them to the section **"You only get one shot"**, if they ran into difficulties.
- The user would reflect on the impacts of deleting all the rows in that table. It would not only delete all segments inserted by this plugin - it would delete all segments inserted by any plugin.
- The user would also find it interesting that Chapters (the tick marks on the player) and Segments are different things. In fact, they would find it so fascinating that they read Jellyfin's docs and forums for more information.
- The user would also wisely muse that a backup of a jellyfin config takes up a **trivial** amount of disk space and they would certainly **not attempt any database edits** without a **tested** server backup.

## Jellyfin version support
This project is coupled to Tube Archivist, and they state that they will support only the latest version of Jellyfin. This plugin aims to align with that policy. It was initially developed to support Jellyin `v10.10.*`. This plugin also supports Jellyfin `v10.11.0`.

# Contribution
- This plugin was hacked together functionality-first; there is no love here, it was built to do a single job! I do not expect to develop it further, there is no feature roadmap.
- Opening issues is encouraged, and PRs are welcomed.
- This plugin will be brought to the attention of Tube Archivist developers, hopefully they will integrate the functionality into the main plugin.
