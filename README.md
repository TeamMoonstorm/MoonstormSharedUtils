# Moonstorm Shared Utils - Content Loading and General Utility Framework for Large Content Mods.

MoonstormSharedUtils, otherwise known as MSU, is an API designed to work around the ThunderKit system for creating Content Mods.

![](https://raw.githubusercontent.com/TeamMoonstorm/MoonstormSharedUtils/main/icon.png)

Originally part of the API and Framework built for updating [Starstorm2](https://thunderstore.io/package/TeamMoonstorm/Starstorm2/) to a ThunderKit setup for a better development experience, it has grown exponentially into it's own separate library so anyone with the desire to create large Content mods with thunderkit can use it.

## Goals of the API

MSU strifes for the following goals regarding mod development and management.

* Simple but powerful systems for managing and loading content for the game in an Asynchronous fashion.
* A robust framework built upon modules that allows you to automate various parts of the modding workflow, such as equipment execution, monster and interactable spawning, and more.
* Simplification of the ThunderKit workflow by providing a custom set of CompossableObjects for managing Manifests, Paths and Pipelines.
* A High enphasis on utilizing the systems provided by the game over hooks whenever possible, such as default provided delegates, base item behaviours, and more.
* Utilization of the Existing R2API Framework to power systems such as Director modifications.
* Emphasis on working within the Editor instead of working on code for the creation of assets.

## The purpose of this Github.

The Github is used as the main front of the API, Used for creating new issues and as well for development of the project itself.

To get started developing new features for MSU, please look at the [ONBOARDING](https://github.com/TeamMoonstorm/MoonstormSharedUtils/blob/main/ONBOARDING.md) document.

## Donations

MSU is a passion project from one of TeamMoonstorm's members, Nebby. as such, he works in his free time on this to allow the rest of the community to create amazing and awe-inspiring content.

MSU will forever be free to use and never gated behind paywalls, however, donations are incredibly appreciated.

[![ko-fi](https://raw.githubusercontent.com/TeamMoonstorm/MoonstormSharedUtils/tree/main/Docs/Readme/SupportNebby.png)](https://ko-fi.com/nebby1999)

## Mods Utilizing MSU

<details><summary>(Click me!)</summary>
<p>

(Note: click the icon to open a new tab to the Mod!)
| Icon/URL | Name | Description |
|--|--|--|
|[![StarstormIcon](https://gcdn.thunderstore.io/live/repository/icons/shbones-ShbonesVariants-0.0.2.png.128x128_q95.png)](https://thunderstore.io/package/shbones/ShbonesVariants/)| Shbones Variants | Adds new monster variants to the game using Nebby's VarianceAPI. |

</p>
</details>

## Thanks and Credits

* Twiner for the creation of ThunderKit.
* IDeath and Harb for helping out with various coding questions.
* KingEnderBrine and RuneFox237 for the RoR2SkinBuilder, which was used as a base for the VanillaSkinDef system.
* GrooveSalad for helping out during the conceptualization of the module system and providing the StubbedShaders.
* The Starstorm 2 Team, for allowing nebby to go off the deep end and create MSU in the first place.
* The Fortunes from the Scrapyard team, for believing in my goals and utilizing MSU for their mod.
* KevinFromHPCustomerService, for creating the original modules systems that eventually became MSU 2.0
* Everyone from the Risk of Rain 2 community for appreciating my work.