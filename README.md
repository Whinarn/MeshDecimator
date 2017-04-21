# MeshDecimator
A mesh decimation library for .NET and Unity. The project is written entirely in C# and released under the MIT license.

## Building
Before you open up the Visual Studio project you must first make sure that you have a *UNITY_HOME* environment variable set to the Unity Editor installation directory (for example *C:\Program Files\Unity\Editor* on Windows or */Applications/Unity/Unity.app* on OSX).

## Compatibility
This project is currently compatible with Unity 5.5.X and 5.6.X

While it might work with other versions of Unity, this will be the officially supported Unity versions.

## Installation into Unity
After building the project, copy over *MeshDecimator.dll*, *MeshDecimator.xml*, *MeshDecimator.Unity.dll* and *MeshDecimator.Unity.xml* into your Unity project, anywhere within your Assets directory (for example *Assets/MeshDecimator*). Now copy over *MeshDecimator.UnityEditor.dll* to the same directory and make sure to make it available only within the Unity Editor in the Plugin Inspector, or place it within the Editor special directory (for example *Assets/MeshDecimator/Editor*).

## Fast Quadric Mesh Simplification Algorithm
MeshDecimator uses an algorithm based on the [Fast Quadric Mesh Simplification](https://github.com/sp4cerat/Fast-Quadric-Mesh-Simplification) algorithm, completely rewritten in C#.
Currently it is the only mesh decimation algorithm available to use.