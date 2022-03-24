# README #

The Unity Multiplayer High Level API is the open source component of the Unity Multiplayer system, this was formerly a Unity extension DLL with some parts in the engine itself, now it all exist in a package. In this package we have the whole networking system except the NetworkTransport related APIs and classes. This is all the high level classes and components which make up the user friendly system of creating multiplayer games. This document details how you can enable or embed the package and use it in your games and applications.

Unlike the regular HLAPI package found in Github or in the Unity Package Manager, this version of HLAPI does **__Not__** come with a forced dependency on Mono.Cecil. This was done to ensure compatibility with different mod loaders, such as BepInEx.
It is the responsibility of the person installing this extension to add Mono.Cecil, either by using BepInEx or other sources.

# License Statement

The Unity Multiplayer High Level API is under the MIT license, as such, due to the license, i am allowed to redistribute here.
A copy of the license is both in the package itself, and this readme.

  > The MIT License (MIT)

  > Copyright (c) 2020, Unity Technologies

  > Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

  > The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

  > THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.