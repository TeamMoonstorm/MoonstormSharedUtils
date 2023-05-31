### '1.5.0' - Config Update May

* Now dependant on Risk of Options
* Cleaned up the source code

* Editor:
    * Added ``InstallRiskOfOptions`` import extension`

* Runtime:
    * Deprecated ``TokenModifierAttribute.extraData``, replaced by ``operationData``
    * Reworked the Configuration systems of MSU
    * Deprecated ``ConfigurableFieldManager``, replaced by the ``ConfigSystem`` class.
    * Deprecated ``ConfigLoader.identifierToConfigFile``
    * Added ``OnConfigured`` method to the ``ConfigurableFieldAttribute``.
    * Added ``RooConfigurableFieldAttribute``
        * Allows for very basic Risk of Options implementation.
        * Supports ``bool, float, int, string, UnityEngine.Color, Enum``
    * Added ``ConfigurableVariable<T>`` class
        * Represents a variable that can be configured using the bepinex config system
        * Works as fields and properties.
        * a ``ConfigFile`` can be assigned directly or via the identifier system of the ``ConfigSystem``
        * Just like ``ConfigurableFieldAttribute``, if a ``Section`` and ``Key`` are not specified, it uses a nicified string of the ``DeclaringType`` and ``MemberInfo`` names
        * ConfigurableVariables can be created with method chaining or Object Initialization
        * ConfigurableVariables can be bound immediatly, or bound automatically when RoR2 loads
        * Doesnt support Risk of Options by itself
    * Added the following classes that inherit from ``ConfigurableVariable``:
        1. ``ConfigurableBool``
        2. ``ConfigurableColor``
        3. ``ConfigurableEnum<TEnum>``
        4. ``ConfigurableFloat``
        5. ``ConfigurableInt``
        6. ``ConfigurableString``
        * These classes automatically gets implemented into your mod's RiskOfOptions configuration page.
        * Allows for complexity with the OptionConfig system of Risk of Options
    * Implemented Risk of Options support for MSU's config

### '1.4.3' - No one will ever know!

* Runtime:
    * Added a Return statement on EventDirector's FindIdleStateMachine when the event card's event flags has the WeatherRelated flag.

### '1.4.2' - Fixes and Improvements

* Editor:
    * Fixed general issues with the NamedIDRS and ItemDisplayDictionary editor windows
    * Removed InstallDebugToolkit import extension

* Runtime:
    * Fixed issue where instantiate addressable prefab wouldnt properly instantiate the prefab under multiplayer circumstances
    * AddressableInjector, CameraInstantiator and SurfaceDefInjector now properly wont save the instantiated assets
    * Fixed interactable and monster director cards not working properly
    * Made the ``bool IsAvailable()`` method of Interactable and Monster director cards virtual
    * Added new stubbed speed tree shaders
    * Deprecated ``EventFlags.WeatherRelated``
    * Event Cards can now specify the name of the entity state machine it must play on
    * Added a method to the EventDirectorClass to add new EntityStateMachines.
    * Fixed issue where the Event director wouldn't pick new cards after getting a valid one. causing repeating events without checking availability
    * Debug Build Exclusive:
        * Prefixed debug commands and convars with ``ms``
        * Fixed the MoonstormIDH component not working properly
        * Fixed MSUDebug having an unintended dependency on DebugToolkit
        * Added back the EnableEvents conVar
        * Added ``ms_enable_event_logging`` BoolConVar
        * Added ``ms_play_event`` command, which tries to play an event while following the regular availability checks
        * Added ``ms_add_credits`` command, which allows you to add or subtract credits from the Event Director.

### '1.4.1' - Whoops

* Fixed an NRE that could occur under specific circumstances caused by the ItemDisplayCatalog

### '1.4.0' - IDRS Utility and Stabilization

* Finally updated the ``package.json`` file to properly display the Runtime version

* Editor:
    * Updated to use RoR2EK 4.0.1
    * Fixed issue where ``BasicBuild`` would Stage Assetbundles before Staging the Assembly
    * Fixed issue where the ``GenericContributorBuild`` wouldn't have MSU in its whitelist
    * Stubbed Shaders are now included in the MSU AssetBundle to avoid duplicate assets across multiple bundles
    * ``CameraInstantiator`` now doesnt save in Editor (thanks Cacapexac)
    * Added ``ItemDisplayCatalog`` class, which is populated by the ``ItemDisplayCatalog.json`` which is created at Run time when building MSU in Debug mode.
    * Added a new and improved version of the NamedIDRS window that uses the ItemDisplayCatalog
    * Added a new and improved version of the ItemDisplayDictionary window that uses ItemDisplayCatalog
    * Fixed issue where ``DecaliciousDeferredDecal`` would have the wrong shader name

* Runtime:
    * Objects loaded/instantiated by the AddressableComponents no longer save in editor and builds. theyre not editable but do show up in the hierarchy
    * Fixed an issue where the ``ItemTierPickupDisplayHelper`` would throw an NRE under specific situations
    * Added HOLY.dll support
        * Internally fixes issues where interfaces implementing ``IStatItemBehaviour.RecalculateStatsStart()`` would run after ``orig(self)``
    * Added an ``AsValidOrNull`` extension method which allows the ussage of the ``?.`` and ``??`` operators with unity objects
    * Improved the ``InstantiateAddressablePrefab`` (thanks Caxapexac)
    * Marked ``AddressableKeyAsset`` and ``AddressableIDRS`` as Obsolete
    * Marked ``NamedIDRS.AddressNamedRuleGroup.keyAsset`` and ``NamedIDRS.addressNamedDisplayRule.displayPrefab`` as Obsolete
    * ``ItemDisplayDictionary`` can now have multiple display prefabs, the selected one is chosen via indices.
    * Marked ``ItemDisplayDictionary.NamedDisplayDictionary.idrs`` and ``ItemDisplayDictionary.displayPrefabs`` as Obsolete
    * Added ``ItemDisplayCatalog``
        * The ``ItemDisplayCatalog`` is used at Runtime for appending the item display data of ``ItemDisplayDictionary`` and ``NamedIDRS`` to their respective target
        * This is done via collecting all the IDRS in the game and assigning string keys, alongside collecting all the display prefabs, and assigning string keys that correspond to their key asset
        * In ``DEBUG`` mode, the ItemDisplayCatalog serializes all the data collected into an ``ItemDisplayCatalog.json``
        * The ``ItemDisplayCatalog.json`` is then used in the Editor for adding data and manipulating existing ``ItemDisplayDictionary`` and ``NamedIDRS`` assets
    * Added ContextMenus for ``ItemDisplayDictionary`` and ``NamedIDRS`` for updating to the ``ItemDisplayCatalog`` system
        * This is also ran at runtime awake to ensure previous, non updated mods dont break.
    * Fixed issue where moduleAvailability didnt work at all

### '1.3.0' - Stage Creation Utilities

* Removed MSUTests, as it wasnt an actual tests package, it'll come back soon(tm)
* Github package updated to use ThunderKit version 7.0.0 or Greater

* Editor:
    * Added inspectors for ``AddressableInjector``, ``InstantiateAddressablePrefab``
    * Added a pipeline to change the BuildMode for StageAssemblies jobs on a Pipeline, which allows to build mods on Debug or Release mode.
    * Added ChangeAssemblyBuildMode to ``Release`` & ``GenericContributorBuild`` pipelines
    * Added MSU's R2API hard dependencies JSON file into the Editor folder of the package, use this when setting up a MSU project in ThunderKit
    * Added a PropertyDrawer for ``AddressableSpawnCard``
    * Fixed issue where ``SwapShadersAndStageAssetBundles`` would still add .yaml shaders into the finished bundle.

* Runtime:
    * Fixed issue where ``BaseBuffBodyBehaviour`` would throw exceptions under certain circumstances.
    * Added a ``GetEnabledExpansions`` method thatt returns all the enabled expansions for a run inside MSUtil
    * Fixed mutliple isssues with EventtCard's IsAvailable() method
    * Added Implicit ``bool`` and ``UnityEngine.Object`` casts to AddressableAssets
    * Added components for creating and manipulating vanilla assets inside scenes
        * ``AddressableInjector``, for injecting an AddressableAsset to a component's field
        * ``CameraInstantiator``, which instantiates the ror2 camera prefab for using PostProcessing
            * This component should exist in editor only and not on release builds.
        * ``InstantiateAddressablePrefab``, for Instantiating prefabs
        * ``SurfaceDefInjector``, for injecting a surface def address to multiple GameObjects
        * These are untested, report bugs if necesary
    * Removed a large amount of Debug related code, which now exists in Debug development builds.
    * Added missing ``EventDirectorCategorySelection`` for Artifact Reliquary
    * Marked event Actions on module bases as obsolete, replaced by ``ResourceAvailability``.
    * Marked ``RemoveIfNottInCollection`` method as obsolete, as its wrongly named.
    * Removed config option for enabling debug features, as to obtain them you must build MSU on Debug mode
    * Added ``MSDCCSPool`` and ``MSDirectorCardCategorySelection`` & ``AddressableSpawnCard``
        * These are used for creating DCCSPools and DirectorCardCategorySelection respectively for custom stages.
        * Untested, report bugs if necesary
    * Added a field to ``Eventcard`` to specify a cost multiplier if the event has already played on a stage
    * Fixed an issue where Events that should only play once per run could play multiple times

### '1.2.0' - Item Tier Support

* Runtime:
    * Updated to use the R2API Split Assemblies update.
    * Added ItemTier support
        * Support is in the form of the ItemTierModule and ItemTierBase
        * ItemTierModule handles loading of ItemTierBases, and implements custom lists that contain the amount of items using said tier, and the current available items in a run.
        * ItemTierBase can be used to specify custom Color entries using R2API's ColorsAPI, alongside a custom pickup display VFX
    * Added a deconstruct method for key value pairs.
    * Added a nicify string method to MSUtils
    * Marked MaterialCopier as Deprecated
    * Elites now properly have their ramps set ingame.
    * Added the AddressableMaterialShader shader
        * replacement for MaterialCopier
        * Contains a custom field where the address can be inputed
        * Calling "FinalizeMaterialsWithAddressableMaterialShader" method in your assetloader copies the addressable material's properties and shaders to your custom instance.
   * Event Related
        * Removed the requiredExpansionDef field from EventCard
        * EventCard's selection weight now ranges between 0 and 100
        * Fixed a major issue where the EventDirector prefab was set to server only (this fix makes events actually network.)
    * Fixed TokenModifiers and ConfigurableFields crashing the game if no instance of either was found.
    * When a field configurable by ConfigurableFied gets it's value changed, Configurablefield will now apply the new value. (this allows ConfigurableField to work with ROO)
    * Added missing XML documentation


### '1.1.2' - Hotfix

* Runtime:
    * Fixed issue where event messages wouldnt display properly.

### '1.1.1' - Bug Fixes

* Runtime:
    * Event Related:
        * Fixed null reference exception issue with the Event Director on Custom Stages.
        * SetupWeatherController no longer causes issues with Custom Stage
        * Added an X offset for the event messages
        * Event director wont instantiate new events when the teleporter is charged, or when it's charging percent is over 25%
        * Event Cards can now have multiple required Expansion Defs.
        * Added missing EventDirectorCategorySelection for Gilded Coast
        * Fixed most Properties in VanillaEventDirectorCategorySelection returning null
    * Token Modifier changes:
        * Marked "AddMod" method as obsolete
        * Now inherits from SearchableAttribute
        * Now works on Properties and Fields
        * Marked StatTypes.Percentage and StatTypes.DivideBy2 as obsolete
        * Added the following StatTypes:
            * DivideByN
            * MultiplyByN
            * AddN
            * SubtractN
        * added a new field for representing the N in the new stat types, this allows mod creators to have a lot more control on the displayed values.
    * ConfigurableField attribute now uses SearchableAttribute
    * AddressableAssets changes:
        * AddressableAssets' OnAddressableAssetsLoaded no longer runs before initialized sets to true.
        * Added constructors for the bundled in AddressableAssets

* Editor:
    * Shader Dictionary now is kept loaded in static memory on domain reloads.
    * SwapShadersAndStageAssetbundles will now revert swapped shaders back to normal if an exception is thrown

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

### '0.8.0'

* Additions:
    * Added a method on MSUtil for playing a networked sound event def
    * The HGCloudRemap controller now has the option to modify the Src and Dst blend enums.
    * Revamped the CharacterModuleBase class
        * Now allows for proper implementation of Monsters, including having them spawn on stages
        * Added the MonsterDirectorCard scriptable object
* Fixes:
    * Added missing Submodule dependency for UnlockableAPI and DirectorAPI
* Other:
    * Moved the entire codebase and project to the github, instead of being inside Starstorm2's Github
    * Rewrote parts of the ReadMe


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


### '0.3.0'

* Rewrote a lot of the Elite related code to allow for more pleasant and balanced behaviors.
    * Fixed Elite Ramps not Reseting properly when grabbing an MSEliteEquipment and then grabbing a vanilla one.
    * Added Tooltips for the MSEliteDef
* Added an Opaque Cloud Remap runtime editor. (OCR are deprecated, ghor recommends using regular cloud remaps instead).
* Projectiles with CharacterBody components now get added to the Serializable Content Pack's CharacterBody prefabs.
* Added IOnIncomingDamageOtherServerReceiver
    * Used to modify the incoming damage of soon to be Victims in the damage report, it is similar to the IOnIncomingDamage interface.
* Almost all classes are now documented, However, MSU does not come with XML documentation yet.
* Made MoonstormEditorUtils Dependant on MoonstormSharedUtils.
* Changes to the IDRS System.
    * The ItemDisplayPrefab scriptable object has been revamped to a key asset display pair holder.
    * The KeyAssetDisplayPairHolder can be given to the IDRS module and it'll populate both the key asset dictionary and the display prefab dictionary.
    * This can be used for external mods that dont directl depend on MSU to populate the item display dictionary. 

### '0.2.1'

* Re-Added the MSAspectAbility, now named "MSAspectAbilityDataHolder"
    * Only holds data, doesnt use aspect abilities at all.
    * Used for elite equipments.
* Fixed an issue where the EliteModuleBase wouldnt register new elites.
* Fixed an issue where the ItemManager would not properly obtain new  StatItemBehavior interfaces on buff removal.
* Removed EditorUtils until further notice.
* General internal changes

### '0.2.0'

* Deprecated MSAspectAbility due to issues with initalization of the mod.
    * It has been replaced by an Animation curve asset and a simple float for AIMaxUseDistance
* Fixed an issue where MSIDRS/SIDRS would not be able to retrieve their display prefab correctly.
* Changed any references to definitions adding to content pack to use the content pack's name.
* Added more material controllers, courtesy of Vale-X
* Removed abstract field from module abses
* Module bases now use SystemInitializer attribute

### '0.0.1'

* Initial Release