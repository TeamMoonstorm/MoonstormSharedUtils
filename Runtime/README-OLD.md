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