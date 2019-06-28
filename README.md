# VPUpdater
Automatic updater for Virtual Paradise

## About
This tool is directly inspired by [VPUpdater](https://github.com/Evonex/VPUpdater) by Evonex. However since its last commit 4 years prior, a lot has changed and there were a lot of issues.

* The tool did not support the 64-bit version of Virtual Paradise.
* The original dev committed build output and binaries to SCM.
* It is incompatible with new the VP download flow.
* It was written in VB.NET (this isn't necessarily bad, but C# is more properly supported by the community.)

I've rewritten the tool from the ground up in C# to correct all of these issues.

## How to use
1. Ensure you have the [.NET Framework 4.8 Runtime](https://dotnet.microsoft.com/download/dotnet-framework/net48)
2. Download the [latest release](https://github.com/oliverbooth/VPUpdater) and extract the files to your Virtual Paradise directory
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

## Known issues
* If pre-release builds are enabled, it will always say there is an update even if you have it. Pre-release builds install into `%PROGRAMFILES%\Virtual Paradise (pre-release)`, where VPUpdater probably *isn't*. Possible fix: Have the app scan this directory too for the Virtual Paradise version number.

* An exception is thrown trying to fetch the version number of a pre-release install of Virtual Paradise. Possible fix (?): Truncate string if it contains `"alpha"` or `"beta"` perhaps?

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