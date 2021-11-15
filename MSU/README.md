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
    - Module bases included are:
        - Artifact Module
        - Buff Module
        - Character Module
        - Damage Type Module (R2API Dependant)
        - Elite Module
        - Item Display Module
        - Items and Equipments Module
        - Projectile Module
        - Stage Module

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
        - Item
        - Monster
        - Projectile
        - Survivor
        . Stages

- Scriptable Object
    - MSU comes packaged with scriptable objects to either help the aid of content creation, or for the help of working alongside RoR2's Systems.
    - Scriptable Object Included are:
        - Vanilla Skin Def - Create a Skin for a vanilla character.
        - Serializable Difficulty Def - Creates a Difficulty Def
        - Monster Director Card & Monster Director Card Holder - Used for creation of monsters.
        - EffectDef Holder - Used for creating EffectDefs in the editor and adding them to the content pack.
        - String ItemDisplayRuleSet - Used for creating item display rules in the editor. all based off a system of strings and dictionaries. Creating a new display is as simple as writing the name of the key asset, and the display prefab. and you can even use the values from KEB's Item Display Placement Helper. as simple as copying them and pasting them in it's field.
        - Single Item Display Rule - Similar to R2API's ability to create item displays for each character in an item. Create item display rules for any vanilla IDRS using strings.
        - Key Asset Display Prefab Holder - Holds the Key Asset and their respective Display Prefab. used in the IDRS module.
        - MSEliteDef - An improved version of the vanilla Elite Def. allows you to easily create either a tier 1 or tier 2 elite in the editor. including the ability to pre-set the Elite's Ramp, Overlay, Particles or Effects.

- Components:
    - Item Manager - Takes care of managing the items made with MSU. automatically handling their ItemBehaviors.
    - Manager Extensions - Extend the ItemManager using Manager Extensions.
    - Moonstorm Item Display Helper - Used for the ItemDisplays module.
    - Comes pre-packaged with KomradeSpectre's HGController finder. modify in real time a material that uses hopoo's shaders with an inspector.

- Utilities, Interfaces and Attributes:
    - Utilities:
        - MSUtil - Contains for now a single method for checking wether or not a mod is installed.
        - MSDebug - A MonoBehavior that gets attached to the base unity plugin. enabling this debug component causes changes that should facilitate the creation of mods.
            - Changes include:
                - Connect to yourself with a second instance of RoR2.
                - Muting commando by removing his skills (In case you need to do a lot of searching in a runtime inspector.)
                - Automatic deployment of the no_enemies command from debug toolkit.
                - Automatic addition of the Moonstorm Item Display Helper component to all the characterbodies found in the catalog.
                - Spawning the Material Tester, which comes pre-packaged with the HGControllerFinder.
    - Interfaces:
        - IStatItemBehavior - An interface that works alongside the ItemBehaviors. this Interface allows you to interact with RecalculateStats. Albeit only for the Begining portion and the end portion. if you want to interact in a more deeply level you should use recalcstatsAPI.
        - IOnIncomingDamageOtherServerReceiver - An interface used to modify the incoming damage of soon to be Victims in the damage report, it is similar to the IOnIncomingDamage interface.
        - Provides a fix for IOnKilledOtherServerReceiver interface, it'll no longer run code twice in a row.
    - Attributes:
        - DisabledContent - Put this attribute on top of a content base inheriting class, and MSU will not load it nor add it to the content pack.
        - TokenModifier - Put this attribute on top of a field that is public & static, give it a token to modify, a stat type, and a formatting index, and MSU will format said token with the values in the field, this can be used so configurable values are always in sync between ingame and config file. a guide on doing this can be found [here](https://github.com/TeamMoonstorm/MoonstormSharedUtils/wiki/%5BTokenModifier%5D). You'll need to submit your mod to the TokenModifierManager via "TokenModifierManager.AddMod(Assembly)"
        - ConfigurableField - Put this attribute on top of a field that's both public & static, and MSU will proceed to automatically create configuration for the field. You'll need to submit your mod to the ConfigurableFieldManager via "ConfigurableFieldManager.AddMod(Assembly, ConfigFile)". Contains 3 string properties for giving custom Section, Name and Description for your configuration.

## Documentation & Sourcecode

* The Documentation and Sourcecode can be found in MoonstormSharedUtil's Github Repository, which can be found [here](https://github.com/TeamMoonstorm/MoonstormSharedUtils)

Some things to note...

> The Repository might have some disparaties between the current release of MSU and the repo itself, this is because it's not the real repository where the development happens

> we'll try to keep this repository up to date at any cost.

- MoonstormSharedUtils now has a [Wiki](https://github.com/TeamMoonstorm/MoonstormSharedUtils/wiki), filled with introductions and explanations on how the systems work.

- For now, you can find an example on how to use the mod in Lost In Transit's Github repository, which can be found [here](https://github.com/swuff-star/LostInTransit/tree/master/LIT/Assets/LostInTransit)

## Changelog

(Old Changelog can be found [here](https://github.com/TeamMoonstorm/MoonstormSharedUtils/blob/main/MSU/README_OLD.md))

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