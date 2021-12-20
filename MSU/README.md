# Moonstorm Shared Utils

- Moonstorm Shared Utils (Abreviated as MSU) is a library mod with the intention to help ease the creation of mods, mainly ones that are developed trough Thunderkit. It is a library mod used by the members of Team Moonstorm.

- Contains both classes for mod making and an assembly for Editor utilities.

- Classes for mod making include a modular system using abstract classes, alongside abstract content classes. Taking heavy use off the ItemBehavior class found inside the CharacterBody.

## Features

- Module Bases
    - A Module base is a type of class inside MSU that handles a specific type of content found inside RoR2. Theyre abstract by nature so you need to inherit from them to properly work.
    - Key aspects of module bases include:
        - Easily add items to your contentpacks.
        - Handles the automatic use of ItemBehaviors.
        - Handles most of the interaction between the game's systems and the content bases.
    - Module bases included are:
        - Artifact Module
        - Buff Module
        - Character Module
        - Damage Type Module (R2API Dependant)
        - Elite Module
        - Interactable Module
        - Item Display Module
        - Items and Equipments Module
        - Projectile Module
        - Scene Module
        - Unlockables Module

- Content Bases
    - A Content base is what MSU uses to identify the type of content youre trying to add to the game. Theyre abstract by nature so you need to inherit from them to properly work.
    - Key Aspects of Content bases include:
        - Initialization methods to finish anything on runtime, such as replacing materials with ingame ones, or getting any kind of asset that needs to be finished in runtime.
        - Abstract fields for ContentDefinitions (ItemDef, ArtifactDef, EquipmentDef, ETC).
    - Content Bases Included are:
        - Artifact
        - Buff
        - Character
        - Damage Type
        - Equipment (& Elite Equipment)
        - Interactable
        - Item
        - Monster
        - Projectile
        - Survivor
        - Scene
        - Unlockable

- Scriptable Object
    - MSU comes packaged with scriptable objects to either help the aid of content creation, or for the help of working alongside RoR2's Systems.
    - Scriptable Object Included are split into 5 categories:
        - General:
            - EffectDefHolder - Used for creating EffectDefs in the editor and adding them to the content pack.
            - MSUnlockableDef - Used alongside the Unlockable content class, the MSUnlockableDef not only works like a regular UnlockableDef, but also creates the AchievementDef associated to the UnlockableDef on runtime.
            - SerializableDifficultyDef - A Serialized DifficultyDef, Implementation of the difficulty itself is done thru R2API's DifficultyAPI
            - VanillaSkinDef - Creates a Skin for a vanilla character, avoids the needles implementation of doing a hook on SkinDef's awake.
        - DirectorCards:
            - MSInteractableDirectorCard - Used alongside the Interactable content class, the InteractableDirectorCard works as an extension of the InteractableSpawnCard and has special fields that allow for implementation of the interactable thru R2API's DirectorAPI.
            - MSMonsterDirectorCard - Used for creating a Director Card for a Monster.
            - MSMonsterdirectorCardHolder - Used for adding MSMonsterDirectorCards to scenes.
            (Both MSMonsterdirectorCard and it's Holder are outdated and do not follow the same philosophy as the MSInteractableDirectorcard, expect an overhaul in the near future)
        - Elites:
            - MSAspectAbilityDataHolder - Holds data for MysticSword's Aspect Abilities, proper implementation is on the end user (LIT has an example on how to implement it.)
            - MSEliteDef - An extended version of an EliteDef, the MSEliteDef has the ability to automatically set the Elite's ramp, automatically determine in which tier it spawns, what overlay materials to use and if it has an Effect that accompanies it (Like Tier2 Elites)
        - Events:
            - EventDirectorCard - A DirectorCard for MSU's EventDirector, holds information such as the identifier, likelyhood of spawning, event flags, required unlockables, and more.
            - EventSceneDeck - A Holder for EventDirectorCards, allows the end user to add new events to specific scenes.
        - IDRS:
            - KeyAssetDisplayPairHolder - Used for handling the addition of key assets and display prefabs to the IDRS module.
            - MSIDRS - A version of the IDRS that works in editor, Achieves this function by using Strings to represent key assets and display prefabs. an MSIDRS can be used to append new IDRS to ANY existing IDRS in game.
            - MSSingleItemDisplayRule - A variation of the MSIDRS, the MSSIDR works by handling a single key asset and a single display prefab, and can add it to as many item display rule sets as wanted.

- Components:
    - Item Manager - Takes care of managing the items made with MSU. automatically handling their ItemBehaviors.
    - Manager Extensions - Extend the ItemManager using Manager Extensions.
    - Moonstorm Item Display Helper - Used for the ItemDisplays module.
    - DestroyOnEnable - Quite literally as it says, this component destroys it's parent game object as soon as its enabled.
    - MoonstormEliteBehavior - Used for managing the MSEliteDef.
    - Comes pre-packaged with KomradeSpectre's HGController finder. modify in real time a material that uses hopoo's shaders with an inspector.

- Utilities, Interfaces and Attributes and More:
    - Utilities:
        - MSUtil - Contains a shorthand method for knowing if a mod is installed, and a method that creates InverseHyperbolicScaling (Courtesy of KomradeSpectre)
        - MSUDebug - A MonoBehavior that gets attached to the base unity plugin. enabling this debug component causes changes that should facilitate the creation of mods.
            - Changes include:
                - Connect to yourself with a second instance of RoR2.
                - Making huntress the default character instead of commando, since huntress doesnt shoot or attack when pressing M1 and having no enemies nearby (Useful if you need to do a lot of things in a Runtime inspector)
                - Automatic deployment of the no_enemies command from debug toolkit, alongside removing the need to spawn via an escape pod
                - Automatic addition of the Moonstorm Item Display Helper component to all the characterbodies found in the catalog.
                - Spawning the Material Tester, which comes pre-packaged with the HGControllerFinder.
    - Interfaces:
        - IBodyStatArgModifier - An interface that works alongside the ItemBehaviors, its used for interacting with R2Api's RecalculateStatsAPI. which means most recalculate stats interaction should be done with this interface.
        - IStatItemBehavior - An interface that works alongside the ItemBehaviors. this Interface allows you to interact with RecalculateStats. Albeit only for the Begining portion and the end portion. It's more or less useful for modifying stats that arent available in IBodyStatArgModifier or for getting values right after stat recalculation is finished.
        - IOnIncomingDamageOtherServerReceiver - An interface used to modify the incoming damage of soon to be Victims in the damage report, it is similar to the IOnIncomingDamage interface.
        - Provides a fix for IOnKilledOtherServerReceiver interface, it'll no longer run code twice in a row.
    - Attributes:
        - DisabledContent - Put this attribute on top of a content base inheriting class, and MSU will not load it nor add it to the content pack.
        - TokenModifier - Put this attribute on top of a field that is public & static, give it a token to modify, a stat type, and a formatting index, and MSU will format said token with the values in the field, this can be used so configurable values are always in sync between ingame and config file. a guide on doing this can be found [here](https://github.com/TeamMoonstorm/MoonstormSharedUtils/wiki/%5BTokenModifier%5D). You'll need to submit your mod to the TokenModifierManager via "TokenModifierManager.AddMod(Assembly)"
        - ConfigurableField - Put this attribute on top of a field that's both public & static, and MSU will proceed to automatically create configuration for the field. You'll need to submit your mod to the ConfigurableFieldManager via "ConfigurableFieldManager.AddMod(Assembly, ConfigFile)". Contains 3 string properties for giving custom Section, Name and Description for your configuration.
    - Loader Classes:
        - Loader classes are a type of class that loads external content into the game, their main purpose is to facilitate the loading of assetbundles, language files, and content packs.
            - AssetsLoader: Class for handling loading assetbundles, contains method for automatically swapping the stubbed shaders from MoonstormSharedUtils and creation of EffectDefs.
            - ContentLoader: Class for handling loading your mod's content, it's main appeal is the ability to load and set up content asynchronously, instead of doing everything in Awake. Contains arrays of Actions for both Loading content and Setting static fields on static types, much like RoR2Content does.
            - LanguageLoader: Class for handling loading Language folders, automatically handles loading the Language files into the game's systems for use with the TokenModifier attribute.

## Documentation & Sourcecode

* The Documentation and Sourcecode can be found in MoonstormSharedUtil's Github Repository, which can be found [here](https://github.com/TeamMoonstorm/MoonstormSharedUtils)

Some things to note...

> The Repository might have some disparaties between the current release of MSU and the repo itself, this is because it's not the real repository where the development happens

> we'll try to keep this repository up to date at any cost.

- MoonstormSharedUtils now has a [Wiki](https://github.com/TeamMoonstorm/MoonstormSharedUtils/wiki), filled with introductions and explanations on how the systems work.

- For now, you can find an example on how to use the mod in Lost In Transit's Github repository, which can be found [here](https://github.com/swuff-star/LostInTransit/tree/master/LIT/Assets/LostInTransit)

## Changelog

(Old Changelog can be found [here](https://github.com/TeamMoonstorm/MoonstormSharedUtils/blob/main/MSU/README_OLD.md))

### '0.7.0'

* Additions:
    * Now finally comes bundled with proper XML based documentation, huzzah!
    * Added Assets, Content and Language Loaders
        * These classes handle external loading of assets, such as assetbundles and language files
        * ContentLoader works as a simplified version of a class implementing IContentPackProvider, and helps with loading content Asynchronously

* Changes:
    * The AchievementDefs are now added directly to the game using R2API

### '0.6.0'

* Changes:
    * MSU no longer has any kind of dependency on AspectAbilities
    * MSU no longer handles the implementation of an aspect ability by itself
    * Removed dependency on Microsoft.Csharp
    * Event Director:
        * No longer should gain negative amounts of credits on custom difficulties with indexes on the negatives.

* Additions
    * Interfaces:
        * Added IBodyStatArgModifier Interface
            * Used for interacting with R2Api's RecalculateStatsAPI
    * Unlockables:
        * Added an Unlockables Module
        * Unlockables module handles the implementation of UnlockableDefs and the creation of AchievementDefs
        * UnlockableDefs and Achievementdefs are made inside the MSUnlockableDef class
        * Unlockables are registered inside UnlockableBase classes. the norm is also having it's related Achievement as a nested class
        * Unlockables can have dependencies on other ContentBases
        * If a dependency is not enabled, the unlockable will not be added to the game
        * In case the dependency is a custom made content base, you can override OnFailedToCheck() method to handle it.
    * Interactables:
        * Added an Interactables Module
        * Interactable Module handles the implementation of custom Interactables to the game
        * Interactables are created from the MSInteractableDirectorCard, which itself inherits from the InteractableSpawnCard
        * Interactablkes are automatically added to stages via DirectorAPI

### '0.5.1'

* Fixed the Damn Readme file.

* Changes:
    * EventAPI
        * Fixed the Director trying to spawn events when there where no available events.
        * Director no longer spawns when there are no events registered.
    * ItemDisplayModuleBase
        * Changed how vanilla IDRS are populated, theyre not taken directly from the BodyCatalog when its initialized.
        * This in turn enables people to add IDRS to other characterBodies from mods.
        * Deprecated "PopulateVanillaIDRSFromAssetBundle()"
    * MSIDRSUtil
        * Deprecated as we're trying to change the standard on how modded IDRS are done

### '0.5.0'

~~* Additions:
    * Added Event system API (*Look, I normally don't do this, okay? I don't really know what else has been done , but this is Starstorm 2's Event API, forcefully ripped out and put in a place where YOU can use it. There is NO documentation. I don't even know if it works. But you can (probably) use it to do cool stuff!
     ...I hope Nebby forgives me for this one.)~~

No, I do not.

Actual changelog:

* Additions:
    * Added the ability to extend from the MoonstormItemManager component.
        * Extending from the manager requires you to extend from the "ManagerExtension" component.
        * Immediate References to the characterBody attatched to the manager extension, the manager itself as well.
        * Virtual methods for GetInterfaces, CheckForItems and CheckForBuffs.
    * Added the EventAPI from Starstorm2Nightly into MSU.
        * The Event API itself is not documented and very much WIP.
        * EventAPI should have everything to add custom events.
        * EventAPI works via a custom director, events themselves are simply entity states.
        * All Events should inherit from the GenericEvent entitystate, which is found in the EntityStates.Events namespace.
* Changes
    * Artifact Content Base:
        * Added OnArtifactEnabled() and OnArtifactDisabled() abstract methods, subscribe and unsuscribe from hooks in these methods. System closely resembles how the Artifact Managers of RoR2 Work.
        * Added an Abstract field for an ArtifactCode from R2API's ArtifactCodeAPI, can be left null.
    * Artifact Module Base:
        * Added some actual hooks onto the RunArtifactManager.
    * Pickups Module Base: Added an Event when the ItemManager is added.
    * Material Tester:
        * Can no longer be spawned outside of runs
        * Renderer is no longer null by default
        * Can now be destroyed easily by enabling the "DestroyOnEnable" component.

### '0.4.1'

* Additions:
    * Added the StageModuleBase & StageBase
        * Used for handling custom stages
        * Compatible with ROS
* Changes:
    * Marked the MSAspectAbilityDataHolder as Deprecated, will be removed on the next major update.
    * Now Dependant on [Microsoft CSharp](https://thunderstore.io/package/RiskofThunder/Microsoft_CSharp/)
    * Changed how Elite Equipments get initialized
        * This Retroactively fixes an issue where, if the Elite Equipment Base overrides the AddItemBehavior method, but its not fully initialized (like disabling it from a config) it would add the item behavior regardless ((Example on the issue)[https://github.com/swuff-star/LostInTransit/issues/2])
    * Completely Revamped the Dynamic Description attribute.
        * Now called the "TokenModifier" attribute.
        * Used on Fields that are public & static
        * Requires the following arguments
            * String: the Token to modify
            * StatType: Used for modifying the value held in the field
                * Default: No changes are made
                * Percentage: The value on the field is multiplied by 100
                * DivideBy2: The value on the field is divided by 2
                * If you need a specific stat type, ask it in the starstorm discord and we might add it.
            * FormatIndex: the index used for formatting.
        * Should technically work with any mod.
        * Not usable on mods that load their language via LanguageAPI (Due to languageAPI's string by tokens dictionaries being private.)
* Bug Fixes: 
    * Fixed an issue where moonstorm dependant mods would try to access MSUtil on the Moonstorm.Utilities namespace despite being on the Moonstorm namespace 

### '0.4.0'

* Added ConfigurableField attribute
    * Used for automatically creating config options for fields.
* Added DynamicDescription attribute
    * Used for dynamically modifying the description of an item via the use of formatting, and provided fields.
