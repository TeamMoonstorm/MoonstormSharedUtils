# Moonstorm Shared Utils

MoonstormSharedUtils, otherwise known as MSU, is a "Content Loading" and "General Utility" API designed to work around the ThunderKit system for creating Content Mods.

Originally part of the API and framework built for updating Starstorm2 to Thunderkit, it was eventually split into it's own separate library so anyone with the desire to create mods with thunderkit can use it.

## Goals:

* MSU Strifes for the following goals:
    * Simple systems for Managing and Loading content for the game.
    * Modularity on its systems to allow high flexibilty.
    * Simplification of Thunderkit workflow.
    * High enphasis on using RoR2 systems over hooks if possible
    * Enphasis on Editor work over Code work for development.

## Key Features:

### Content Bases and Modules:

To aid in the loading and management of content pieces for the game, MSU comes bundled with what are called Content and Module classes.

A very detailed explanation of these can be found [here](https://github.com/TeamMoonstorm/MoonstormSharedUtils/wiki/The-modules-and-content-relationship), but in a nutshell:

* ContentBase:
    * Represents an abstracted Content piece of the game (IE: Buff, Artifact, Item, etc)
    * Contains methods and systems for implementing the behaviour of a content piece
    * Allows for high customization via inheritance.
    * Allows for automatic behaviour usage, creation and interaction with base game systems via ModuleBases.

* ModuleBase:
    * Represents a class that manages a type of content base.
    * A ModuleBase implements all the hard work of making a Content from the managed content base appear in game properly, so it lets you create more content instead of implementing systems.
    * Allows for high customization via inheritance.
    * A module base will always have a ContentBase to manage, unless stated otherwise.
    * A ModuleBase creates all instances of it's ContentBase, allowing modularity via Config options.

### RoR2 Friendly Systems:

MSU's goals belive that by working with ROR2's systems instead of forcing them to work with ours means that a code for a mod will become more robust overtime, as such, MSU provides multitude of systems apppart from the Content and Module bases for implementing the behaviour of your mod's features.

* BaseBuffBodyBehaviour:
    * A custom written behaviour system that works like the base game's BaseItemBodyBehaviour.
    * Works with BuffDefs instead of ItemDefs, perfect for implementing complex behaviours with buffs.
* The following are Interfaces that are meant to be used inside BodyBehaviours.
    * IBodyStatItemBehaviour: an interface that wraps around R2API's RecalculateStatsAPI's systems. Allows for complex modifications of stats via RecalculateStatsAPI's systems.
    * IOnIncomingDamageOtherServerReciever: Works as a hook on TakeDamage, this interface allows you to modify the incoming DamageInfo of a Victim.
    * IStatItemBehavior: An interface that allows you to run code before and after RecalculateStats.

We also have a guide explaining the base game's interfaces that can replace a lot of hook usage [here](https://github.com/TeamMoonstorm/MoonstormSharedUtils/wiki/Item-Behavior-Crashcourse#what-interfaces) (Note, guide may be out of date.)

### Improved Thunderkit Workflow:

As MSU's goals revolve around working in the editor, this means that the more time a developer spends in the editor implementing content the better, as such, MSU comes bundled with ScriptableObjects, and a custom Editor package that works off the systems implemented in ThunderKit and RoR2EditorKit.

* On the Runtime aspect, we have ScriptableObjects:
    * VanillaSkinDefinition: An extended version of a SkinDef, It Allows you to create a Skin for a vanilla body. Akin to RoR2SkinBuilder.
    * MSUnlockableDef: An extended version of an UnlockableDef, It allows you to create a fully working AchievementDef tied to an UnlockableDef.
    * MaterialCopier: Allows you to copy the properties and shaders of an AddressableMaterial into your project at runtime.
    * NamedIDRS: A complete self contained IDRS system, the NamedIDRS can be used to create the ItemDisplayRuleSet of a Survivor or Monster. Completely serialized in the editor via Direct References or Addressables, alongside tools for using the ItemDisplayPlacementHelper.
    * ItemDisplayDictionary: Inspired by R2API.ItemAPI's ItemDisplayDictionary, it allows you to append new ItemDisplayRules to multiple IDRS. Completely serialized in the editor via Direct References or Addressables, alongside tools for using the ItemDisplayPlacementHelper.
    * SerializableEliteTierDef: a serializable version of an EliteTierDef, easily create new elite tiers with this and the bundled in EliteTierDefModule and Content bases.
    * MSEliteDef: An extended version of a regular EliteDef, an MSEliteDef alongside the Elite systems in MSU will take care of adding the elite to vanilla tiers, alongside implementing the Ramp, Overlay and Visual Effects.
    * MSInteractableDirectorCard: An extended version of an InteractableSpawnCard, it allows you to easily create a card that can be used immediatly in the game via MSU's interactions with R2API.DirectorAPI.
    * MSMonsterDirectorCard: An extended version of a CharacterSpawnCard, it allows you to easily create a card that can be used immediatly in the game via MSU's interactions with R2API.DirectorAPI.

* On the Editor aspect, we have MoonstormSharedEditorUtils, a complete overview of the package can be found [here](https://github.com/TeamMoonstorm/MoonstormSharedUtils/blob/main/Editor/README.md).

## Documentation & Sourcecode:

* The Documentation and Sourcecode can be found in MoonstormSharedUtil's Github Repository, which can be found [here](https://github.com/TeamMoonstorm/MoonstormSharedUtils)

## Donations:

MSU is a passion project from one of TeamMoonstorm's members, Nebby. as such, he works in his free time on this to allow the rest of the community to create amazing and awe-inspiring content.

MSU will forever be free to use and never gated behind paywalls, however, donations are incredibly appreciated.

[![ko-fi](https://media.discordapp.net/attachments/850538397647110145/994431434817273936/SupportNebby.png)](https://ko-fi.com/nebby1999)

## Thanks and Credits:

* General Help and Advice: Twiner, IDeathHD, Harb.
* Programmers: Nebby, KevinFromHPCustomerService.
* KingEnderBrine and RuneFox237 for the RoR2SkinBuilder, which was used as a base for the VanillaSkinDefinition
* Anreol for providing more Stubbed Shaders.
* Swuff, TheTimeSweeper, Dotflare, HeyImNoop, JaceDaDorito, Enigma, Vale-X, 2Cute2Game, Riskka, Bruh, UnkownGlaze, Neik, Jame, MysticSword, PlasmaCore and many other developers for beliving in my work and goal of MSU
* Everyone from the Risk of Rain 2 community for appreciating my work.

## Changelog:

(Old Changelog can be found [here](https://github.com/TeamMoonstorm/MoonstormSharedUtils/blob/main/Runtime/README-OLD.md))

### '1.1.0' - Void Items

* General:
    * Added Tests for MSU
    * Not bundled with the Thunderstore Release
    * Contains classes for testing out the systems of MSU and the API as a whole

* Runtime:
    * AddressableAssets now have a bool to check if it uses a direct reference rather than an addressable reference
    * Added a VoidItemBase, for creating Void items
    * Added the following extensions:
        * Play for NetworkSoundEventDefs
        * GetItemCount for CharacterBodies
    * Event announcements now are properly networked
    * Added configuration options for the Event Announcements
        * Opacity reduced to 75%
        * Message size set to 40
        * Y position Offset of 225 (Messages appear right below boss health bars)
    * Added a Tri-Planar and CalmWater material controlers, courtesy of jaceDaDorito
    * Added missing documentation
    * Removed most weather related components, these have been migrated to Starstorm2

* Editor:
    * Added a custom property drawer for AddressableAssetDrawers
    * Added a Constants file
    * Updated ShaderDictionary to use the SerializableShaderWrapper from RoR2EK
    * MaterialShaderManager's Upgrade and Downgrade methods now use the correct dictionary

### '1.0.0' - Official Release

* MSU is now a "PackageRepo" in github! this means that it is now heavily recommended to use the repository for creating mods with MSU instead of this release version.
* Updated to Survivors Of The Void (Version 1.2.4)
* Complete Refactoring of the entire codebase.
    
    * Some names have changed
    * Some systems have been redone or checked up.
    * A lot of internal changes on how things work.
    * This just means that 1.0.0 marks the begining of LTS, which means future versions shouldnt break compatibility with older versions.

* Events are now considered to be in a Finished, usable state.
* Complete XML Documentation for all classes except some components
* Improved experience with Scriptable Objects.
* Added a custom Addressables Solution
* Removed redundant features with newest version of R2API
* Removed already deprecated classes.
* Deprecated some existing classes
* Improved loader systems.
* Way, way too many more changes i cant remember currently, a more complete changelog can be found [here](https://github.com/TeamMoonstorm/MoonstormSharedUtils/blob/main/Runtime/1.0.0%20Changelog.md)!