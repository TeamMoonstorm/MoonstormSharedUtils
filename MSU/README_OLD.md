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
