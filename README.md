<h1 align="center">Jellyfin Email Plugin</h1>
<h3 align="center">Part of the <a href="https://jellyfin.media">Jellyfin Project</a></h3>

<p align="center">
<img alt="Logo Banner" src="https://raw.githubusercontent.com/jellyfin/jellyfin-ux/master/branding/SVG/banner-logo-solid.svg?sanitize=true"/>
<br/>
<br/>
<a href="https://github.com/jellyfin/jellyfin-plugin-emailnotifications/actions?query=workflow%3A%22Test+Build+Plugin%22">
<img alt="GitHub Workflow Status" src="https://img.shields.io/github/workflow/status/jellyfin/jellyfin-plugin-emailnotifications/Test%20Build%20Plugin.svg">
</a>
<a href="https://github.com/jellyfin/jellyfin-plugin-emailnotifications">
<img alt="MIT License" src="https://img.shields.io/github/license/jellyfin/jellyfin-plugin-emailnotifications.svg"/>
</a>
<a href="https://github.com/jellyfin/jellyfin-plugin-emailnotifications/releases">
<img alt="Current Release" src="https://img.shields.io/github/release/jellyfin/jellyfin-plugin-emailnotifications.svg"/>
</a>
</p>

## About
Jellyfin Email plugin is a notification plugin to send Jellyfin notifications via mail

## Build Process

1. Clone or download this repository

2. Ensure you have .NET Core 5.0 SDK setup and installed

3. Build plugin with following command.

```sh
dotnet publish --configuration Release --output bin
```
