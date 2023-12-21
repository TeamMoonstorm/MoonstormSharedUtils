
# MSEU Thunderkit Templates
This folder contains a few ThunderKit like templates for your project.

The goal of This folder is to contain common pipelines and manifest structures used by most MSU related mods (SS2 and LIT for example)
## Paths
MSU's building process comes with path components that are used for building mods, these path components are structured to maintain order and not overwrite changes between versions and keep an overal tidy setup.

| Path Reference | Components |Result|
|--|--|--|
|BuildPath|0. ThunderKit Root; 1. Constant; 2. ManifestName; 3. ManifestVersion|.../ThunderKit/Builds/[ManifestName]/[ManifestVersion|
|BuildPathPlugins|0. Output Reference (BuildPath); 2. Constant|.../ThunderKit/Builds/[ManifestName]/[ManifestVersion]/plugins|
|BuildPathAssets|0. Output Reference (BuildPathPlugins); 2. Constant|.../ThunderKit/Builds/[ManifestName]/[ManifestVersion]/plugins/assetbundles|
|BuildPathSoundbanks|0. Output Reference (BuildPathPlugins); 1. Constant|.../ThunderKit/Builds/[ManifestName]/[ManifestVersion]/plugins/soundbanks|
|ReleasePath|0. ThunderKit Root; 1. Constant; 2. ManifestName; 3. ManifestVersion|.../ThunderKit/Releases/[ManifestName]/[ManifestVersion]|


## Manifest

Contains a generic Manifest for use with MSU, The manifest uses the custom PathReference assets found under the "Paths" folder.

This manifest should be duplicated into your Assets folder instead of modifying it directly.
| Datum  | Usage | Staging Path |
|--|--|--|
| ManifestIdentity (0) | The ManifestIdentity for the manifest, contains an already asigned dependency on MSU's manifest | ManifestPluginStaging |
|Assembly Definitions (1)|The AssemblyDefinition datum| BuildPathPlugins|
|Asset Bundle Definitions (2)|The AssetBundleDefinition Datum| BuildPathPlugins|
|Files (3)| This Files datum is used for storing the WWise SoundBank|BuildPathSoundbanks|
|Language Folder Tree (4)|The LanguageFolderTree Datum, contains a pre-made language folder for english|BuildPathPlugins|
|Thunderstore Data (5)|The ThunderStoreData Datum, this creates the manifest.json for your mod|BuildPath |
|Files (6)|The second files datum, this one stores your mod's Icon, & README|BuildPath|

## Pipelines

The MSU pipelines are split into 3, the Release pipeline, the Generic pipelines & the Contributor pipelines


### Release: 
* This pipeline is used for release, it runs the CompleteBuild pipeline, and afterwards runs the StageThunderstoreManifest, StageManifestFiles and Zips the finalized product, the overall output is `<ReleasePath>.zip`
	* The ZIP produced by this pipeline is ready for publishing into thunderstore.
	* It is recommended to copy and paste this pipeline instead of directly using it, as it's ZIP job has not been modified tto use a whitelist instead of a blacklist
	* The pipeline is built to use MSU's paths and manifest structure

### Generic:
* The generic pipelines can be used for general build purposes, they're modular by nature as they try to make the build process for a mod as smooth as possible
	* These pipelines are built to use MSU's Paths and Manifest structure
	* Since these pipelines do not use manifest processor jobs, it is safe to use them directly and its not necesary to copy these

| Name | Description | Jobs |
|--|--|--|
| BasicBuild | A basic build that stages assetbundles and stages assemblies | 0. Swap Shaders and Stage Assetbundles; 1. Stage Assemblies |
|StageLanguageAndSoundbanks| A pipeline for staging both the SoundBank and the LanguageFile|0. Stage Manifest Files; 1. Stage Language Files|
|Complete Build|A pipeline that executes BasicBuild and StageLanguageAndSoundBanks|0. Execute Pipeline (Basic Build); 1. Execute Pipeline (Stage LanguageFiles)|

### Contributor:
* On big projects, it is very common for multiple people to be contributing to the project itself, it can be tricky to help new people set up pipelines or letting them know how to build the mod specifically, the Contributor pipeline set takes care of this problem by having a "template" for extremely simple building and deploying.
	* It is recommended that each new person contributing duplicates the "Generic Contributor" pipeline, and then modify the pipeline name and path reference accordingly.
	* Once duplicated, change all mentiojns of "generic contributor" to your name. Example:
		* Folder: [GenericContributor -> Nebby]
		* Pipeline: [GenericContributorBuild -> NebbyBuild]
		* Path: [GenericContributorPath -> NebbyPath]
	* Do not forget to set the constant on the path!