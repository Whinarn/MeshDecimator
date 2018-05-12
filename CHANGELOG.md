# Changelog

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

[v0.2.0]: https://github.com/Whinarn/UnityMeshSimplifier/compare/v0.1.0...v0.2.0
