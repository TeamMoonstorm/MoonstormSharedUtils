
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