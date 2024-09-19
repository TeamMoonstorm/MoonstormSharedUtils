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

## Key Features

### The IContentPiece, IContentPieceProvider and Module Framework

While Code only mods create their Content (Prefabs, ScriptableObjects, etc) at Runtime and usually at Awake, this causes the issues known as "Forever Black Screen", where most of the mod's systems are initialized before the Loading Screen of the game starts. this gives the idea that the game is frozen and not responsive to oblivious users. And while this is true, managing the loading and interactions between custom made content from Assetbundles and the game's systems is difficult.

MSU solves this by the triad of the ContentPiece, the ContentPiece Provider, and the Module framework. This triad is utilized to allow mods to Load their assets Asynchronously during the loading screen, and have them working properly with the base game's systems.

#### IContentPiece

* Represents some form of Content that a mod is adding
* Each ContentPiece is tied to a specific Module which handles loading and the implementation of the Content.
* ContentPieces have Availability systems, which the module uses to know what content to load and initialize.
* A ContentPiece has an Asynchronous loading method which the module uses during initialization, which is used to Asynchronously load assets for your content.
* Being an interface, the API provides further implementations:
    * ``IContentPiece<T>``, which represents a Content that's tied to a specific ``UnityEngine.Object``. 
        * MSU includes the following interfaces that implement ``IContentPiece<T>``
            * ``IArtifactContentPiece``, for ``ArtifactDefs``
            * ``IEquipmentContentPiece`` for ``EquipmentDefs``
                * A sub-interface called ``IEliteContentPiece`` manages ``EliteDefs`` associated to a specific Equipment.
            * ``IItemContentPiece`` for ``ItemDefs``.
                * A sub-interface called ``IVoidItemContentPiece`` manages the Item Corruption system added in Survivors of the Void.
            * ``IItemTierContentPiece``for ``ItemTierDefs``
            * ``ISceneContentPiece`` for ``SceneDefs``.
    * ``IGameObjectContentPiece<TComponent>``, which represents a Content that's tied to a specific type of ``UnityEngine.MonoBehaviour``
        * MSU includes the following interfaces that implement ``IGameObjectContentPiece<TComponent>``
            * ``ICharacterContentPiece`` for ``CharacterBodies``
                * The sub-interface called ``IMonsterContentPiece`` can be used for managing hostile monsters
                * The sub-interface called ``ISurvivorContentPiece``can be used for managing new Survivors.
            * ``IInteractableContentPiece`` for ``Interactables``.
    * ``IVanillaSurvivorContentPiece``, which represents modifications for a Vanilla survivor.
* ``IContentPiece`` classes can also implement the ``IContentPackModifier``, which is used for directly interfacing with your mod's ContentPack.

#### Modules

* A Module is a class that manages the loading and interaction of ``IContentPiece`` classes with the base game.
* Each Module handles a specific type of Interface, alongside possible sub-interfaces.
* The Module knows what classes to instantiate and initialize utilizing the ``IContentProvider`` system.
* Once you provide a ``IContentProvider``to a module, you can call it's ``Initialize`` method to get back a Coroutine that'll initialize your content in an Asynchronous fashion.
* MSU supplies the following modules:   
    * ``ArtifactModule``, manages ``IArtifactContentPiece``s, the ArtifactModule interfaces with ``R2API.ArtifactCode`` to add new Artifact Codes to the game. It'll also handle proper hooking and unhooking of the Artifact, so that only when the artifact is enabled it's hooks are enabled.
    * ``CharacterModule``, manages ``ICharacterContentPiece``, ``ISurvivorContentPiece`` and ``IMonsterContentPiece``, the module interfaces with ``R2API.Director`` to handle the spawning of Monsters for the stages of the game.
    * ``EquipmentModule``, manages ``IEquipmentContentPiece``and ``IEliteContentPiece``, the module utilizes a single hook for managing the Execution calls for the Equipments.
    * ``InteractableModule``, manages ``IInteractableContentPiece``, the module interfaces with ``R2API.Director`` to handle the spawning of Interactables for the stages of the game.
    * ``ItemModule``, manages ``IItemContentPiece`` and ``IVoidItemContentPiece``. It automatically handles item corruptions.
    * ``IItemTierModule``, manages ``IItemTierContentPiece``, it automatically creates at run start collections of the available drop list for the tier.
    * ``SceneModule``, manages ``ISceneContentPiece``, it interfaces with ``R2API.Stages`` to handle the addition of a Stage to the game.
    * ``VanillaSurvivorModule``, manages ``IVanillaSurvivorContentPiece``. which is used to add new content to the base game's survivors (Skins, skills, etc.)

#### IContentPieceProvider

* For a module to know what content pieces to create, load and initialize. MSU utilizes the ``IContentPieceProvider`` to provide ``IContentPiece``s to modules.
    * While you can create your own implementations of the ``IContentPieceProvider`` interface to manage availability scenarios, MSU's ``ContentUtil`` class contains methods for creating them from scratch, by only analyzing your Assembly.

### RoR2 Friendly Systems

One of the key goals of MSU is working alongside the Risk of Rain 2 systems, making content that works with it instead of forcing the base game's systems to work with ours. As such, MSU provides a robust system to interact alongside the game's key systems such as equipments, buffs, items, and more.

#### Interfaces for Components

To avoid unecesary hooking for commonly used types, MSU provides interfaces that can be used inside components for the following systems:

* ``IOnIncomingDamageOtherServerReciever``, it works as a hook on TakeDamage, which can be utilized to modify the incoming damage for a victim.
* ``IStatItemBehaviour``, an interface that works as an On hook for Recalculate Stats, containing methods for running before and after stat recalculations.
* ``IBodyStatArgModifier``, an interface that works as an implementation of ``R2API.RecalculateStats``'s GetStatCoefficient delegate.

#### BaseItemMasterBehaviour

The BaseItemMasterBehaviour, much like it's Body counterpart (``BaseItemBodyBehaviour``) is utilized for creatinng a behaviour that's added to a CharacterMaster when a specified item is obtained.

#### BaseBuffBehaviour

The BaseBuffBehaviour is a component that can be used for managing the effects of a Buff. For performance reasons, BaseBuffBehaviours are not destroyed when the buff gets removed, instead, when the buff is first obtained by a body, the behaviour is added, when the buff stacks are depleted, the behaviour is ``disabled``, afterwards, when the body recieves the buff again, the behaviour is ``enabled``. this reduces the workload of the GarbageCollector and overall improves the performance of the system.

### Improved Thunderkit Workflow:

As MSU's goals revolve around working in the Editor, MSU comes bundled with a multitude of utilities regarding the creation of content for the game.

#### WWise Support

MSU provides a custom ImportExtension that can be used to blacklist the WWise assemblies from the game, allowing you to use the WWise Integration system for your project.

#### Scriptable Objects

MSU provides the following ScriptableObjects that allows for the creation of "runtime only" content at Editor time.

* ``AchievableUnlockableDef``, works as an abstraction of the UnlockableDef and AchievementDef system, its an ``UnlockableDef`` thats unlocked via an achievement.
* ``DotBuffDef``, works as an abstraction of the DotDef, and automatically assigns a DotDef to its specified BuffDef.
* ``ExtendedEliteDef``, works as an extension of the ``EliteDef`` class, supporting automatic addition to base game tiers, elite ramps, overlay materials, and more.
* ``InteractableCardProvider``, a ScriptableObject that contains the metadata needed for spawning Interactables in stages, multiple stages can be assigned to a single card.
* ``MonsterCardProvider``, a ScriptableObject that contains the metadata needed for spawning Monsters in stages, multiple stages can be assigned to a single card.
* ``ItemDisplayDictionary``, a ScriptableObject that works akin to ``R2API.Items``'s ItemDisplayDictionary system, it can be used for adding multiple RuleGroups to existing ``ItemDisplayRuleSets``, the key assets and display prefabs are powered by the ``ItemDisplayCatalog`` system within MSU.
* ``NamedItemDisplayRuleSet``, a ScriptableObject that can be used for creating a complete ``ItemDisplayRuleSet`` for character models. the key assets and display prefabs are powered by the ``ItemDisplayCatalog``system within MSU.
* ``SerializableEliteTierDef``, works as an Abstraction of the ``EliteTierDef`` system within the game, can assign a cost multiplier, addressable references to base game elite tiers and mode.
* ``VanillaSkinDef``, works as an Extension of the ``SkinDef``system, its used for adding new skins to vanilla characters.

#### Prebuilt CompossableObjects

Since MSU was created with thunderkit in mind, MSU provides the following CompossableObjects to your project:

* A simple ``Manifest`` with the necesary ``ManifestDatums`` for declaring your mod
* A setup of ``PathReference``s that's used for clean building and releasing of your mod
* A highly configurable Pipeline system:
    * A release pipeline that automatically builds a zip file ready for release of your mod.
    * Generic pipelines, which can be used to build your assembly, build your assetbundles either compressed or uncompressed, and staging your mod's StreamingAssets.
    * A Contributor pipeline setup, which contributors can use to easily setup a pipeline for building your mod.

### Management of Configurations and Tokens

Creating configuration for your mod can be extremely verbose using the default BepInEx implementations, MSU implements a Configuration system on top of BepInEx that allows you to easily configure anything.

#### ConfigureField, ConfiguredVariable, and Risk of Options

MSU provides the following abstraction of the BepInEx Config System:

* ``ConfigureField``
    * A custom attribute that can be applied to static fields to automatically configure them.
    * The configuration process utilizes a unique string ID that you assign to a custom ConfigFile to tie the configuration to the ConfigFile.
    * The ``ConfigSection`` and ``ConfigNames`` are not necesary to be implemented, MSU by default utilizes the ``MemberInfo``'s name as the ``ConfigName``, and the ``DeclaringType``'s name as the ``ConfigSection``
    * A "RiskOfOptions" version of the ConfigureField exists, which automatically creates options utilizing the RiskofOptions API. These options however cannot be heavily customized due to the limitation of attributes.

* ``ConfiguredVariable``
    * The ConfiguredVariable is a class that represents a Variable that can be Configured.
    * it can be used for creating complex configuration scenarios with Risk of Options.

#### The FormatToken system

Tying Configuration changes to Token values is one of the best parts of creating tokens using code, however, translation of these tokens can be difficult to manage as it requires translators to code the translations directly into the C# source file.

MSU provides the ``FormatToken`` system, you can now write your token's values utilizing the String Formatting system of C#. With this, you can properly levrage JSON langauge files, which translators can easily use for translations as long as they keep the proper order of indexing.

## Documentation and Sourcecode

* The Documentation and Sourcecode can be found in MoonstormSharedUtil's Github Repository, which can be found [here](https://github.com/TeamMoonstorm/MoonstormSharedUtils)

## Donations

MSU is a passion project from one of TeamMoonstorm's members, Nebby. as such, he works in his free time on this to allow the rest of the community to create amazing and awe-inspiring content.

MSU will forever be free to use and never gated behind paywalls, however, donations are incredibly appreciated.

[![ko-fi](https://raw.githubusercontent.com/TeamMoonstorm/MoonstormSharedUtils/main/Docs/Readme/SupportNebby.png)](https://ko-fi.com/nebby1999)

## Mods Utilizing MSU

<details><summary>(Click me!)</summary>
<p>

(Note: click the icon to open a new tab to the Mod!)
| Icon/URL | Name | Description |
|--|--|--|
|[![StarstormIcon](https://raw.githubusercontent.com/TeamMoonstorm/MoonstormSharedUtils/main/Docs/Readme/SS2Logo.png)](https://thunderstore.io/package/TeamMoonstorm/Starstorm2/)| Starstorm2 | A sequel to Starstorm 1. Adds new survivors, mechanics, items, skills, enemies, and more! |
|[![LostInTransitIcon](https://raw.githubusercontent.com/TeamMoonstorm/MoonstormSharedUtils/main/Docs/Readme/LITLogo.png)](https://thunderstore.io/package/LostInTransitTeam/LostInTransit/)| Lost in Transit | Lost in Transit is a mod focused on restoring features lost from Risk of Rain 1, currently including items. |
|[![VarianceAPIIcon](https://raw.githubusercontent.com/TeamMoonstorm/MoonstormSharedUtils/main/Docs/Readme/VAPILogo.png)](https://thunderstore.io/package/Nebby/VarianceAPI/) | Variance API | VarianceAPI allows you to create Variants for CharacterBodies, Variants can have different textures, lights, skills, and more. |
|[![FortunesFromTheScrapyardIcon](https://raw.githubusercontent.com/TeamMoonstorm/MoonstormSharedUtils/main/Docs/Readme/FFTSLogo.png)](https://discord.gg/n3SnnJkBmb) | TO BE RELEASED - Fortunes from the Scrapyard | An expansion-esque content mod that revolves around a third party Space Corporation, and visually based on Junkyard and Cyberpunk themes. |
|[![RulersOfTheRedPlaneIcon](https://raw.githubusercontent.com/TeamMoonstorm/MoonstormSharedUtils/main/Docs/Readme/RORPLogo.png)](https://thunderstore.io/package/IEye/Rulers_of_the_Red_Plane/) | Rulers of the Red Plane | A general content mod expanding upon the Red Plane |

</p>
</details>

## Thanks and Credits

* Twiner for the creation of ThunderKit.
* IDeath and Harb for helping out with various coding questions.
* KingEnderBrine and RuneFox237 for the RoR2SkinBuilder, which was used as a base for the VanillaSkinDef system.
* GrooveSalad for helping out during the conceptualization of the module system and providing the StubbedShaders.
* UnkownGlaze for MSU's Logo
* The Starstorm 2 Team, for allowing nebby to go off the deep end and create MSU in the first place.
* The Fortunes from the Scrapyard team, for believing in my goals and utilizing MSU for their mod.
* KevinFromHPCustomerService, for creating the original modules systems that eventually became MSU 2.0
* Everyone from the Risk of Rain 2 community for appreciating my work.