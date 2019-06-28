# VPUpdater
[![](https://img.shields.io/github/release/oliverbooth/VPUpdater.svg)](https://github.com/oliverbooth/VPUpdater/releases/latest)
[![](https://img.shields.io/github/issues/oliverbooth/VPUpdater.svg)](https://github.com/oliverbooth/VPUpdater/issues)
[![](https://img.shields.io/github/license/oliverbooth/VPUpdater.svg)](https://github.com/oliverbooth/VPUpdater/blob/master/LICENSE)

Automatic updater for Virtual Paradise

## About
This tool is directly inspired by [VPUpdater](https://github.com/Evonex/VPUpdater) by Evonex. However since its last commit 4 years prior, a lot has changed and there were a few issues:

* The tool did not support the 64-bit version of Virtual Paradise
* It is incompatible with new the download & install flow (the tool expected to download and extract a `.zip` file, where Virtual Paradise now uses a Squirrel installer)

I've rewritten the tool from the ground up to correct all of these issues.

## How to use
1. Ensure you have the [.NET Framework 4.8 Runtime](https://dotnet.microsoft.com/download/dotnet-framework/net48)
2. Download the [latest release](https://github.com/oliverbooth/VPUpdater/releases/latest) and extract the files to your Virtual Paradise directory
3. Create a shortcut to VPUpdater.exe
4. Rename the shortcut to something meaningful (you know... like `Virtual Paradise`)
5. Place that shortcut on your desktop / taskbar / useful location.

Now when you launch "Virtual Paradise", it will launch the updater first. If there are no updates, the tool with simply launch Virtual Paradise. If there is an update, it will download the latest setup and you can go from there.

## How to enable pre-release builds
Edit (or create) file `VPUpdater.cfg` in the Virtual Paradise directory with the line:
```properties
stable_only=0
```
The next time the updater runs, it will check for pre-release builds.

## Building prerequisites
|Prerequisite|Version|
|- |- |
|[.NET Framework Developer Pack](https://dotnet.microsoft.com/download/dotnet-framework/net48)|4.8|

## Nuget package dependencies
|Dependency|Version|
|- |- |
|[AngleSharp](https://www.nuget.org/packages/AngleSharp/0.12.1)|0.12.1|
|[SemanticVersioning](https://www.nuget.org/packages/SemanticVersioning/1.2.0)|1.2.0|

## Disclaimer
This tool is provided as-is.