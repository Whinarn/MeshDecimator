# MeshDecimator
A mesh decimation library for .NET and [Unity](https://unity3d.com/). The project is written entirely in C# and released under the MIT license.

## Compatibility
This provided Unity Example project is currently compatible with Unity 2018.1.0f2

This does not in any way limit the code to that specific Unity version however, but you might have to modify some Unity API usage.

## Prerequisites
The following must be installed before this repository can be properly cloned:

- [git-lfs](https://git-lfs.github.com/)

## Installation into Unity
After building the project, copy over *MeshDecimator.dll* and *MeshDecimator.xml* into your Unity project, anywhere within your Assets directory (for example *Assets/MeshDecimator*).
It is strongly recommended to build the C# project over copying the binaries found in the Unity Example project.

In the provided Unity example in this repository you can find examples of how to use the mesh decimation from Unity.

## Fast Quadric Mesh Simplification Algorithm
MeshDecimator uses an algorithm based on the [Fast Quadric Mesh Simplification](https://github.com/sp4cerat/Fast-Quadric-Mesh-Simplification) algorithm, completely rewritten in C#.
Currently it is the only mesh decimation algorithm available to use.

### The Smart Linking feature
In order to solve artifacts in the mesh simplification process where holes or other serious issues could arise, a new feature called smart linking has been introduced. This feature is enabled by default but can be disabled through the *EnableSmartLink* property on the *FastQuadricMeshSimplification* class. Disabling this could give you a minor performance gain in cases where you do not need this.

The *VertexLinkDistanceSqr* property on the *FastQuadricMeshSimplification* class could be used to change the maximum squared distance between two vertices for the linking. The default value is *double.Epsilon*.

### My decimated meshes have holes, what is wrong?
The original algorithm that was ported from C++ did not support situations where multiple vertices shared the same position, instead of being treated as one vertex they were treated individually. This would then end up creating visible holes in the mesh where the vertices were not connected through triangles.

There are several ways to solve this problem. The smart linking feature (mentioned above) is enabled by default and should take care of most of these problems for you. But there are also options to preserve borders, seams and UV foldovers. The properties *PreserveBorders*, *PreserveSeams* and *PreserveFoldovers* will preserve some vertices from being decimated, strongly limiting the decimation algorithm, but should prevent holes in most situations.

The recommendation is to use the smart linking feature that is enabled by default, but the options for preservation exists in those cases where you may want it.

## Credits
The teddy bear model in the Unity example project is from the following package from Unity Asset Store: https://assetstore.unity.com/packages/essentials/tutorial-projects/mecanim-example-scenes-5328

## Other projects
If you are interested in a more simple mesh simplification in Unity you can visit my other project [UnityMeshSimplifier](https://github.com/Whinarn/UnityMeshSimplifier).
