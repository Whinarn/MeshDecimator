# Change Log

## [v0.3.0] - 2018-10-20

### Added
- Added support to get an array of all sub-mesh indices on a mesh.

### Fixed
- The maximum hash distance is now calculated based on the VertexLinkDistanceSqr property value instead of being hardcoded to 1.
- Fixed an issue with the vertex hashes not using the entire integer range, but instead was using only half of it.
- Reworked the OBJ mesh parsing.

## [v0.2.1] - 2018-07-05

### Fixed
- Heavily optimized the initialization and simplification process for the fast quadric mesh simplification algorithm.
- Removed the maximum vertex count limit for over Unity 2017.3 in the Unity example.
- Added support to specify if meshes should be combined when creating LOD levels in the Unity example.

## [v0.2.0] - 2018-05-12

### Added
- Feature to change the maximum iteration count for the fast quadric mesh simplification algorithm.
- A feature (Smart Linking) to the quadric mesh simplification algorithm that attempts to solve problems where holes could appear in simplified meshes.

### Deprecated
- Linked vertices feature has been deprecated. It was very expensive and has been replaced with the new smart linking feature.

### Changed
- Protected fields of *DecimationAlgorithm* are now private in order to make it easier to deal with backwards-compatibility.
- The *KeepBorders* property has been renamed to *PreserveBorders* on the *DecimationAlgorithm* class.
- The Unity example project is now for Unity version 2018.1.0f2
- The solution file is now for Visual Studio 2017.

### Fixed
- More error logging when vertex attribute array lengths don't match the vertex count.
- Instead of always merging vertex attributes when an edge is removed, the vertex attributes are moved in cases where it would be more effective.
- Better support for skinned meshes.

## v0.1.0 - 2018-04-01

### Added
- A mesh simplification algorithm based on the [Fast Quadric Mesh Simplification](https://github.com/sp4cerat/Fast-Quadric-Mesh-Simplification) algorithm.
- Support for static and skinned meshes.
- Support for 2D, 3D and 4D UVs.

[v0.3.0]: https://github.com/Whinarn/MeshDecimator/compare/v0.2.1...v0.3.0
[v0.2.1]: https://github.com/Whinarn/MeshDecimator/compare/v0.2.0...v0.2.1
[v0.2.0]: https://github.com/Whinarn/MeshDecimator/compare/v0.1.0...v0.2.0
