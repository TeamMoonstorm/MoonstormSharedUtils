# Moonstorm Shared Utils Editor Package

The Editor Package for MSU (Otherwise known as MSEU) is a collection of Classes and systems designed to further improve the ThunderKit experience of creating mods for risk of rain 2.

* Just like its runtime package, The editor package strifes for the following things: 
    * Simplification of Thunderkit workflow.
    * Enphasis on Editor work over Code work for development.

We achieve this with the following features:

## Material Workflow

Working with materials has been difficult ever since the first mods arrived, The editor package tries to help on this by implementing the following features:

* A compelte list of commonly used Stubbed Shaders.
* An extension of the MaterialEditor that allows you to select .asset shader files.
* A "SwapShadersAndStageAssetBundles" pipeline:
    * It swaps the .asset shaders with the stubbed shaders provided by the package.
    * It builds the AssetBundle, making it have stubbed shaders.
    * It swaps the stubbed shaders for the .asset shaders in the editor.
* All of the material systems are handled via the ShaderDictionary thunderkit setting, which is completely extensible.

## Templates

We know that working with thunderkit can be a bit difficult, especially due to the intimidating nature of Pipelines, Path components, and Manifests for new users, as such, MSU comes bundled with Templates of all these, which will improve the workflow and ease of use for newcomers to the thunderkit space.

* Includes:
    * A template manifest that takes care of AssetBundles, Language folders, soundbanks, and more.
    * A collection of Path components that will build your mods in an inteligent fashion
    * A collection of Pipelines, which works in conjunction with the manifest and path components, split into three categories:
        * Release: Easily create a zipped version of your mod ready for release
        * Generic: Easily create a pipeline that will build your mod, highly modulable.
        * Contributor: Extremly simple building process for those devs who just want to build a mod and have it in their r2modman profile.

## Editor Inspectors and Windows.

We belive that a good editor experience is created by it's utilities and inspectors, as such, the Editor package comes bundled with carefully crafted editors and inspectors that will speed up the experience of using MSU's Systems.

![](https://media.discordapp.net/attachments/850538397647110145/994440529251151892/unknown.png?width=1440&height=419)

# Changelog

(Old Changelogs can be found [here]())

# '2.0.0'

* Updated to use RoR2EditorKit 3.2.2
* Updated to use MSU 1.0.0
* No longer being updated in Thunderkit extension store.
* Added a compelte collection of common Stubbed Shaders, for use with MSU's AssetLoader
* Added a complete system of Manifest, Pipeline and Path templates
* Added Import Extensions
* Shader System now works with a Dictionary instead of a hard coded one.
* Inspectors and Editor Windows now use Visual Elements.