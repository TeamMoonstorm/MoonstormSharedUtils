# Moonstorm Shared Utils

Moonstorm Shared Utils (Abreviated as MSU) is a library mod with the intention to help with the creation of mods, mainly ones that are developed thru ThunderKit. The library mod is used by the members of TeamMoonstorm. But anyone is free to use it for their own projects

# Git navigation

This repository by itself is a UnityProject, as such, you need to open it with the unity version risk of rain uses (currently being 2018.4.16f1)

Inside the assets folder you'll be able to see a couple of folders that contain the project itself. The three most important are:

## Moonstorm Shared Editor Utils

This folder contains the Editor utilities made for MSU, it uses [Risk of Rain 2 Editor Kit](https://github.com/risk-of-thunder/RoR2EditorKit) to implement it's features.
In case you need access to the latest, indev version of MSEU, you can copy both the Editor and StubbedShaders folder to your project's MSEU folder (which should be under packages), replacing the existing ones if there are any.

## Moonstorm Shared Utils

The Library itself, decompiled and ready to work, you'll be able to explore all the features of the library by hand by exploring the Components, ScriptableObjects and Modules folder.

If you need to access the latest, indev version of MSU, you'll need to compile it using thunderkit's build pipelines. More details can be found on the section below

## ThunderkitRelated

This folder contains all the custom paths and pipelines created for building the library mod. the first two pipelines build and bundle into zip files both MSU and MSEU. While you can simply run "BuildMSUAndRelease", copy the zip file's contents, extract them in your project for accessing indev versions, the better way of doing this is by building your own pipeline.

You can create your own pipeline insude "OtherPipelines" folder, the folder itself is git ignored completely, but you'll be able to find the "Nebby" folder, which can serve as a guideline on how to setup your own pipelines to build only the important aspects of MSU (Assetbundle and assembly), and have thunderkit copy the outputs to your projects, or mod manager profiles.

## Latest Releases:

MSU 0.8.0

MSEU 1.3.0