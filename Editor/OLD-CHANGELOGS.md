# '1.3.0'

* Updated to use RoR2EditorKit 1.0.0
* Updated to use MSU 0.7.0
* Probably changed some interal things that i cant remember
* Moved the project from Starstorm2's github to its own github.
* Added the following inspectors:
    * MSMonsterDirectorCard
    * MSMonsterFamily

# '1.2.0'

* Updated to use RoR2EditorKit 0.2.3
* Updated to use MSU 0.6.0

* Added the following asset creators:
    * MSUnlockableDef

* Added the following inspectors:
    * MSUnlockableDef
    * MSInteractableDirectorCard

* Added the following property drawers:
    * AchievementStringAssetReference (MSunlockableDef)

* Added DeployToFolders pipeline job
    * Works as an iteratable version of the "Copy" pipeline job

* Added the ability to downgrade or upgrade all the shaders in the project
    * Found inside "Tools" menu item.

# '1.1.2'

* Fixed mod shipping with a duplicate assmebly definition

# '1.1.1'

* Updated to use RoR2EditorKit 0.1.4
* MSIDRS Editor window now has a dictionary with the values that depend on its flags.
* Added the ability to add missing key assets to the MSIDRS editor window
* Added the ability to delete a selected key asset in the MSIDRS editor window

* SIDR editor window now has a dictionary with the values that depend on its flags.
* Added the ability to add a missing IDRS to the SIDR editor window
* Added the ability to delete a selected IDRS in the SIDR editor window

# '1.0.0'

* Moved a lot of the code that made this work into RoR2EditorKit.
* Added RoR2EditorKit as a dependency.
* Removed a lot of unused assemblies from the assembly definition file
* Should properly work without needing to manually set MSU as the dll reference.
* Added an asset creation window for the following types:
    * MSIDRS
    * MSSingleItemDisplayRuleSet
    * ItemDisplayHolder
* Added shader dictionary support for decalicious

# '0.1.0'
* Added MSU as a Needed Dependency for the mod to work properly.
* Added Editor Scripts for custom editor windows for the following types:
    * EntityStateConfigurations
    * Key Asset Display Pair Holder
    * MSIDRS
    * SerializableContentPack
    * SingleItemDisplayRuleSet
    * Vanilla Skin Def
* SwapShadersAndStageAssetBundles now fix any stubbed shaders that are found
* Added CalmWater stubbed shaders.

# '0.0.1'
* Real initial release

# '0.0.0'
* Test release