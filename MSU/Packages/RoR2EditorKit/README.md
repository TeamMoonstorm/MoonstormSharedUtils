# RoR2EditorKit

The Risk of Rain 2 Editor Kit (Abreviated as ROR2EK) is a Thunderkit Extension designed specifically for helping mod creators create content for Risk of Rain 2.

The main goal of (ROR2EK) is to bring a friendly, easy to use editor experience for creating content, ranging from items, all the way to prefabs.

## Features:

---

## Inspectors

RoR2EditorKit comes bundled with custom Inspectors that overwrite the default view of certain Scriptable Objects in RoR2, specifically annoying to work with ones, either with new easier to use inspectors, or editor windows that break down the default inspector for a more viewable experience.

Currently, RoR2EK comes bundled with 7 Inspectors.

* SerializableContentPack: Splits the array view into buttons that can be viewed one by one, adds buttons for automatically populating the array depending on your manifest's assetbundles.
* ObjectScaleCurve: Ticking "Use overall curve only" will hide the other 3 animation curves
* HGButton: Creates an inspector for using the HGButton class, which is used in a variety of UI on RoR2
* ChildLocator: Modifies the entries of the ChildLocator to use only one line instead of two per element.
* CharacterBody: Makes the base and level stats foldable, so you can hide or expand them at will
* BuffDef: Hides the "Icon Path" field
* Entity State Configuration: Easily select an entity state from the target type, when selected, the inspector will automatically populate the serialized fields array with the necesary fields to serialize.

![](https://i.gyazo.com/bb05950708255bbb39c7efb923adea4f.png)

All the inspectors of ROR2EK can be toggled ON or OFF via a toggle switch on the Editor header GUI

---

### Property Drawers

ROR2EK comes with property drawers for handling certain types inside risk of rain 2, The main example is our SerialziableEntityStateType and SerializableSystemType drawer, which allows you to easily search, find and select an entity state for your skill def, entity state machine, or EntityStateConfiguration

![](https://cdn.discordapp.com/attachments/575431803523956746/903754837940916234/unknown.png)

ROR2EK also comes with the following property drawers:
* EnumMask: Used by almost all flag enums, the EnumMask property drawer will allow you to actually set the flags properly.
* PrefabReference: Used by the SkinDef as an example, the Prefab Reference drawer makes it possible to use the SkinDef scriptable object properly
* SkillFamily: Simply hides the unlockableName field of the skill family.

---

### Asset Creator Windows

ROR2EK comes with special editor windows designed specifically for creating Assets for Risk of Rain 2, so far it only comes bundled with editor windows for creating scriptable objects and an Interactable prefab. but we plan on adding more and even complex ones for creating projectiles, or maybe even full body boilerplates. Most of these windows have extra settings that can be used to specify details about the asset youre about to create.

Currently, RoR2EK comes with 8 Asset Creator Windows:

* ArtifactDef: Creates an ArtifactDef using only the artifact's name
* BuffDef: Creates a buffDef using only the name, color, if it stacks, and if tis a debuff
* EntityStateconfiguration: Creates an ESC using solely the target type
* EquipmentDef: Creates an EquipmentDef using the equipment's name, cooldown, wether its enigma compatible and if its a lunar equipment
* ItemDef: Creates an ItemDef using the item's name, tier, and tags.
* Sha256HashAsset: Used for creating codes for ArtifactCodeAPI
* SurvivorDef: creates a survivor def using the name, body prefab, and some other tidbits, currently half-way implemented
* Interactable: Creates an Interactable prefab.

![](https://cdn.discordapp.com/attachments/567852222419828736/903719556894326785/a10578cadaeaa9ad1fbaedbfb8a158b2.png)

---

### MaterialEditor

ROR2EK comes bundled with a special MaterialEditor, the material editor itself is used for handling the difficult to work with Shaders from Risk of Rain 2. It helps via either letting you modify important aspects that arent available by default, or by hiding entire sections if the shader keyword is not enabled.

Currently, ROR2EK comes bundled with 3 Material Editors
* HGStandard
* HGSnowTopped
* HGCloudRemap

All of these material editors work with either the real hopoo shaders, or with stubbed versions.

![](https://i.gyazo.com/172f157cefaefbfb619611b836a8f8fe.png)
###### (Notice how the PrintBehavior, Screenspace Dithering, Fresnell Emission, Splatmapping, flowmap and limb removal are hidden when their keywords are not enabled)

---

### Other:

* ScriptableCreators: A lot of MenuItems to create a myriad of scriptable objects, including the UnlockableDef and a miryad of hidden SkillDefs.

## Credits

* Coding: Nebby, Passive Picasso (Twiner), KingEnderBrine, KevinFromHPCustomerService
* Models & Sprite: Nebby
* Mod Icon: SOM

## Changelog

(Old Changelogs can be found [here](https://github.com/risk-of-thunder/RoR2EditorKit/blob/main/RoR2EditorKit/Assets/RoR2EditorKit/OldChangelogs.md))

### 1.0.0

* First Risk of Thunder release
* Rewrote readme a bit
* Added missing XML documentation to methods
* Added a property drawer for PrefabReference (Used on anything that uses RendererInfos)
* Added the MaterialEditor
    * The material editor is used for making modifying and working with HG shaders easier.
    * Works with both stubbed and non stubbed shaders
    * Entire system can be disabled on settings
* Properly added an Extended Property Drawer
* Added Inspector for CharacterBody
* Added Inspector for Child Locator
* Added Inspector for Object Scale Curve
* Added Inspector for BuffDef
* Fixed the enum mask drawer not working with uint based enum flags