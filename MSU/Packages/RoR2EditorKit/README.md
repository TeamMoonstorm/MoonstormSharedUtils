# RoR2EditorKit

The Risk of Rain 2 Editor Kit is a Thunderkit Extension designed specifically for helping mod creators create content for Risk of Rain 2.

Our main goal is to bring a friendly, easy to use editor experience for creating content, ranging from items, all the way to ideally bodies.

Features:

## Inspectors

RoR2EditorKit comes bundled with custom Inspectors that overwrite the default view of certain Scriptable Objects in RoR2, specifically annoying to work with ones, either with new easier to use inspectors, or editor windows that break down the default inspector for a more viewable experience. Examples of these include:

* Serializable Content Pack: Simply click one of the buttons and the inspector will show it's corresponding list. allowing for easy managment of the content of your mod, alongside the ability to auto populate these fields
![](https://i.gyazo.com/7d9a746fe9386cfe68f1c1a0d2a44c78.png)

* Entity State Configuration: Easily select an entity state from the target type, when selected, the inspector will automatically populate the serialized fields array with the necesary fields to serialize.

![](https://i.gyazo.com/bb05950708255bbb39c7efb923adea4f.png)

## Property Drawers

RoR2EditorKit comes with custom property drawers for handling certain types inside Risk of Rain 2, the main example is our SerializableEntityStateType drawer. which allows you to easily search, find and select an entity state for your skill def or entity state machine.

![](https://cdn.discordapp.com/attachments/575431803523956746/903754837940916234/unknown.png)

## Asset Creator Windows

RoR2EditorKit comes with special editor windows designed specifically for creating Assets for Risk of Rain 2, so far it only comes bundled with editor windows for creating scriptable objects and an Interactable prefab. but we plan on adding more and even complex ones for creating projectiles, or maybe even full body boilerplates.

* ItemDef: Easily create an item def by only giving the name, tier, and tags. you can also automatically create pickup and display prefabs with the correct needed components and proper naming scheme of HopooGames. You can specify more things by clicking the extra settings or prefab settings buttons.

* EquipmentDef: Create an equipment by simply giving it a name, cooldown, and wether or not its a lunar equipment/engima compatible. Like the itemDef window, it can also automatically create display and pickup prefabs. if you need more specification, you can click the extra settings or prefab settings buttons.

* ArtifactDef: Create an artifact quickly by just giving it a name, thats it. it'll also create a pickup prefab if desired.

* Sha256HashAsset: Create a hash asset for use in R2API's ArtifactCodeAPI. simply input the code in the 3 vector3Int fields and hit create. RoR2EditorKit will automatically create a hash asset with the correct hash values.

![](https://cdn.discordapp.com/attachments/567852222419828736/903719556894326785/a10578cadaeaa9ad1fbaedbfb8a158b2.png)

## Other:

* ScriptableCreators: A lot of MenuItems to create a myriad of scriptable objects, including the UnlockableDef and a miryad of hidden SkillDefs.

## Credits

* Coding: Nebby, Passive Picasso (Twiner), KingEnderBrine, Kevin from HP Customer Service
* Models & Sprite: Nebby
* Mod Icon: SOM

## Changelog

(Old Changelogs can be found [here](https://github.com/Nebby1999/RoR2EditorKit/blob/main/RoR2EditorKit/Assets/RoR2EditorKit/OldChangelogs.md))

### 0.2.4

* Made sure the Assembly Definition is Editor Only.

### 0.2.3

* Added the ability for the EntityStateConfiguration inspector to ignore fields with HideInInspector attribute.

### 0.2.2

* Added 2 new Extended Inspector inheriting classes
    * Component Inspector: Used for creating inspectors for components.
    * ScriptableObject Inspector: Used for creating inspectors for Scriptable Objects.
* Modified the existing inspectors to inherit from these new inspectors.
* Added an inspector for HGButton
* Moved old changelogs to new file

### 0.2.1

* Renamed UnlockableDefCreator to ScriptableCreators
* All the uncreatable skilldefs in the namespace RoR2.Skills can now be created thanks to the ScriptableCreator
* Added an EditorGUILayoutProperyDrawer
    * Extends from property drawer.
    * Should only be used for extremely simple property drawer work.
    * It's not intended as a proper extension to the PropertyDrawer system.
* Added Utility methods to the ExtendedInspector

### 0.2.0

* Added CreateRoR2PrefabWindow, used for creating prefabs.
* Added a window for creating an Interactable prefab.
* Fixed an issue where the Serializable System Type Drawer wouldn't work properly if the inspected type had mode than 1 field.
* Added a fallback on the Serializable System Type Drawer
* Added a property drawer for EnumMasks, allowing proper usage of Flags on RoR2 Enums with the Flags attribute.
