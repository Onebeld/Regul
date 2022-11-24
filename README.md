[![GitHub license](https://img.shields.io/github/license/Onebeld/Regul?style=flat-square)](https://github.com/Onebeld/Regul/blob/main/LICENSE) ![GitHub repo size](https://img.shields.io/github/repo-size/Onebeld/Regul?style=flat-square) ![GitHub all releases](https://img.shields.io/github/downloads/Onebeld/Regul/total?style=flat-square) ![GitHub release (latest by date including pre-releases)](https://img.shields.io/github/v/release/Onebeld/Regul?include_prereleases&style=flat-square) ![GitHub release (latest by date)](https://img.shields.io/github/v/release/Onebeld/Regul?style=flat-square) [![CodeFactor](https://www.codefactor.io/repository/github/onebeld/regul/badge?style=flat-square)](https://www.codefactor.io/repository/github/onebeld/regul)

<h1 align="center">
    <img src="https://user-images.githubusercontent.com/44552715/130894645-a777e103-9e3d-442e-90a6-33d3021f2887.png">
    
    Regul
</h1>

![program](https://user-images.githubusercontent.com/44552715/203835754-ef33bbb3-1706-4354-bf27-3310fb90d70b.png)

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

Thanks to .NET 7 and the AvaloniaUI library, the program can run on other OS. This allows you to run the program not only on Windows, but also on macOS and Linux.

### Tabs

It would be inconvenient to work with several windows at the same time, so tabs are created for this purpose. Thanks to this it became much more convenient to work with several files at the same time.

## Screenshots

![Screenshot 1](https://user-images.githubusercontent.com/44552715/203836033-af9ef113-b7f1-422e-8bcd-c36c44d3bd7b.png)

![Screenshot 2](https://user-images.githubusercontent.com/44552715/203836169-7c887c1e-a775-4e50-898f-860878167f92.png)

![Screenshot 3](https://user-images.githubusercontent.com/44552715/203836407-39de0058-639a-4f70-a83c-021d8937aa73.png)

![Screenshot 4](https://user-images.githubusercontent.com/44552715/203836531-64f2957f-1bd6-455d-a38b-456cd7014ae4.png)

## Credits

Libraries:
* [AvaloniaUI](https://github.com/AvaloniaUI/Avalonia)
* [AvaloniaEdit](https://github.com/AvaloniaUI/AvaloniaEdit)
* PleasantUI 2, so far exclusive only to this project
* Some controls from PieroCastillo's [Aura.UI](https://github.com/PieroCastillo/Aura.UI) library
* Modified version of [DotNetCorePlugins](https://github.com/natemcmaster/DotNetCorePlugins)

The editors I used to create this project:
* [JetBrains Rider](https://www.jetbrains.com/rider/)

<img src="https://user-images.githubusercontent.com/44552715/130897295-8a60dd97-32d1-4bd7-8737-101b4a9f044e.png" width="360" align="right"/>
