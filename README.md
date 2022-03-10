[![GitHub license](https://img.shields.io/github/license/Onebeld/Regul?style=flat-square)](https://github.com/Onebeld/Regul/blob/main/LICENSE) ![GitHub repo size](https://img.shields.io/github/repo-size/Onebeld/Regul?style=flat-square) ![GitHub all releases](https://img.shields.io/github/downloads/Onebeld/Regul/total?style=flat-square) ![GitHub release (latest by date including pre-releases)](https://img.shields.io/github/v/release/Onebeld/Regul?include_prereleases&style=flat-square) ![GitHub release (latest by date)](https://img.shields.io/github/v/release/Onebeld/Regul?style=flat-square) [![CodeFactor](https://www.codefactor.io/repository/github/onebeld/regul/badge?style=flat-square)](https://www.codefactor.io/repository/github/onebeld/regul)

<h1 align="center">
    <img src="https://user-images.githubusercontent.com/44552715/130894645-a777e103-9e3d-442e-90a6-33d3021f2887.png">
    
    Regul
</h1>

![program](https://user-images.githubusercontent.com/44552715/130894590-64ea727e-0545-40d3-805f-b3a5120ef1d7.png)

**Regul** - a modular, modern file editor. The goal of this project: to create a simple and feature-rich file editor.

To find out what modules exist and how to install them, take a look at [this page](https://github.com/Onebeld/Regul/blob/main/modules.md).

## Features

Here is a description of the main features of the Regul program at the moment.

### Module system

That's exactly what I've been working on more often. 

The module system allows the program to use third-party modules from third-party developers. Also, these modules are easy to update and you don't have to reinstall the entire program (and the program can weigh 20MB or more). 
At the moment, these modules are officially working and they come with Regul:

* CettaEditor

### Localization Support

This allows the program to work in multiple languages and change it without restarting the program. Modules can also support this, but you need to implement it yourself (see an example of how this can be done).

### Cross-platform

Thanks to .NET 6 and the AvaloniaUI library, the program can run on other OS. This allows you to run the program not only on Windows, but also on macOS and Linux.

### Tabs

It would be inconvenient to work with several windows at the same time, so tabs are created for this purpose. Thanks to this it became much more convenient to work with several files at the same time.

## How to support this project?

There are several ways to support the project:

1. **Create PR** - This is the best way, so you speed up the development of the project and can provide your features that can be useful;
2. **Patreon support** - Less ideal, but that money can go in the right direction (like building a personal website).

## Screenshots

![Screenshot 1](https://user-images.githubusercontent.com/44552715/130896431-f1ddd9fd-9bce-4290-a1cf-bbbd54b83e3c.png)

![Screenshot 2](https://user-images.githubusercontent.com/44552715/130896486-fbade870-39b7-4757-a955-ece1cece7495.png)

## Credits

Libraries:
* [AvaloniaUI](https://github.com/AvaloniaUI/Avalonia)
* [AvaloniaEdit](https://github.com/AvaloniaUI/AvaloniaEdit)
* PleasantUI
* Some controls from PieroCastillo's [Aura.UI](https://github.com/PieroCastillo/Aura.UI) library
* Modified version of [DotNetCorePlugins](https://github.com/natemcmaster/DotNetCorePlugins) to run .NET Standard 2.0 plugins

The editors I used to create this project::
* [JetBrains Rider](https://www.jetbrains.com/rider/) for creating and editing the application
* [Microsoft Visual Studio](https://visualstudio.microsoft.com/) for publishing the application

<img src="https://user-images.githubusercontent.com/44552715/130897295-8a60dd97-32d1-4bd7-8737-101b4a9f044e.png" width="360" align="right"/>
