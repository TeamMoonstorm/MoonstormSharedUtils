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