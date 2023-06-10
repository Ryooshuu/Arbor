<div align="center">
    <img src=".github/logo.png" alt="Arbor" width="500"/>

    *Where imagination blossoms*
</div>

---

## Table of contents:
- [About this project](#about-arbor)
- [Requirements](#requirements)
- [Setup](#setup)
  - [Installation](#installation)
  - [Concepts](#concepts) 
  - [First game](#first-game)
- [Contributing](#contributing)
- [License](#license)

---

## About Arbor

Arbor is a game framework written entirely in C#.
It is designed to have multiple layers of API, from the low-level rendering and audio to the high-level game logic.
It is designed to be as simple as possible to use, while still being powerful enough to create complex games.
Arbor currently uses [Veldrid](https://veldrid.dev/) for rendering, but it does not support any audio API yet.

Arbor is designed around an [Entity-Component-System](https://en.wikipedia.org/wiki/Entity_component_system) architecture,
which has been proven to be very powerful and flexible for game development.
It is specifically designed for 2D games, and as such 3D support is not planned.

Arbor is still in early development, and as such it is not recommended to use it for any serious projects yet.

## Requirements

- [.NET 7.0](https://dotnet.microsoft.com/download/dotnet/7.0)
- [Rider](https://www.jetbrains.com/rider/) or [Visual Studio](https://visualstudio.microsoft.com/) (optional)

## Setup

It is very easy to get started with Arbor, and you can get a basic game up and running in just a few minutes.

### Installation

To install Arbor into your project, you can use the [dotnet CLI](https://docs.microsoft.com/en-us/dotnet/core/tools/)
or use your [NuGet](https://www.nuget.org/) package manager of choice.

```bash
dotnet add package Arbor
```

### Concepts

Arbor is designed around an [Entity-Component-System](https://en.wikipedia.org/wiki/Entity_component_system) architecture.
This means that you will be creating entities within a scene, and adding components to them to give functionality.
You can also create your own components to add custom functionality to your game.

### First game

WIP

## Contributing

We welcome all contributions, but keep in mind that we are still in early development.
If you wish to work on a new feature, please open an issue and we will discuss it further from there, to avoid any wasted effort.

If you're unsure of what you can help with, check out the list of [open issues](https://gitlab.com/ryooshuu/arbor/-/issues). (especially those with the "good first issue" label).

Please see [contributing.md](#) for information about the code standards we will expect from pull requests.
While we have standards set in place, nothing is set in stone. If you have an issue with the way code is structured; with any libraries we are using;
with any process involved with contributing, *please* bring it up. We welcome all feedback so we can make contributing to this project as pain-free as possible.

## License

Arbor is licensed under the [MIT License](https://opensource.org/licenses/MIT). For more information, refer to the [license file](LICENSE)
regarding what is permitted or disallowed in the use of this software.