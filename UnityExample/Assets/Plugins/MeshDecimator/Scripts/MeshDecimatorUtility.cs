using System;
using System.Collections.Generic;
using MeshDecimator.Algorithms;
using MeshDecimator.Math;

using UMesh = UnityEngine.Mesh;
using UBoneWeight = UnityEngine.BoneWeight;
using UVector2 = UnityEngine.Vector2;
using UVector3 = UnityEngine.Vector3;
using UVector4 = UnityEngine.Vector4;
using UColor = UnityEngine.Color;
using UMatrix = UnityEngine.Matrix4x4;
using UMath = UnityEngine.Mathf;
using UTransform = UnityEngine.Transform;
using UMaterial = UnityEngine.Material;

namespace MeshDecimator.Unity
{
    /// <summary>
    /// A mesh decimation utility.
    /// </summary>
    public static class MeshDecimatorUtility
    {
        #region Static Initializer
        static MeshDecimatorUtility()
        {
            // Sets the default logger to a Unity logger, but only if there is no logger set
            // or the current logger is of type Console Logger.
            if (Logging.Logger == null || Logging.Logger is MeshDecimator.Loggers.ConsoleLogger)
            {
                Logging.Logger = new Loggers.UnityLogger();
            }
        }
        #endregion

        #region Private Methods
        #region Vertices
        #region To Simplify
        private static Vector3d[] ToSimplifyVertices(UVector3[] vertices)
        {
            var newVertices = new Vector3d[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                var vertex = vertices[i];
                newVertices[i] = new Vector3d(vertex.x, vertex.y, vertex.z);
            }
            return newVertices;
        }

        private static Vector2[] ToSimplifyVec(UVector2[] vectors)
        {
            if (vectors == null) return null;
            var newVectors = new Vector2[vectors.Length];
            for (int i = 0; i < vectors.Length; i++)
            {
                var vector = vectors[i];
                newVectors[i] = new Vector2(vector.x, vector.y);
            }
            return newVectors;
        }

        private static Vector3[] ToSimplifyVec(UVector3[] vectors)
        {
            if (vectors == null) return null;
            var newVectors = new Vector3[vectors.Length];
            for (int i = 0; i < vectors.Length; i++)
            {
                var vector = vectors[i];
                newVectors[i] = new Vector3(vector.x, vector.y, vector.z);
            }
            return newVectors;
        }

        private static Vector4[] ToSimplifyVec(UVector4[] vectors)
        {
            if (vectors == null) return null;
            var newVectors = new Vector4[vectors.Length];
            for (int i = 0; i < vectors.Length; i++)
            {
                var vector = vectors[i];
                newVectors[i] = new Vector4(vector.x, vector.y, vector.z, vector.w);
            }
            return newVectors;
        }

        private static Vector4[] ToSimplifyVec(UColor[] colors)
        {
            if (colors == null) return null;
            var newVectors = new Vector4[colors.Length];
            for (int i = 0; i < colors.Length; i++)
            {
                var vector = colors[i];
                newVectors[i] = new Vector4(vector.r, vector.g, vector.b, vector.a);
            }
            return newVectors;
        }

        private static BoneWeight[] ToSimplifyBoneWeights(UBoneWeight[] boneWeights)
        {
            if (boneWeights == null) return null;
            var newBoneWeights = new BoneWeight[boneWeights.Length];
            for (int i = 0; i < boneWeights.Length; i++)
            {
                var boneWeight = boneWeights[i];
                newBoneWeights[i] = new BoneWeight(
                    boneWeight.boneIndex0, boneWeight.boneIndex1, boneWeight.boneIndex2, boneWeight.boneIndex3,
                    boneWeight.weight0, boneWeight.weight1, boneWeight.weight2, boneWeight.weight3);
            }
            return newBoneWeights;
        }
        #endregion

        #region From Simplify
        private static UVector3[] FromSimplifyVertices(Vector3d[] vertices)
        {
            var newVertices = new UVector3[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                var vertex = vertices[i];
                newVertices[i] = new UVector3((float)vertex.x, (float)vertex.y, (float)vertex.z);
            }
            return newVertices;
        }

        private static UVector2[] FromSimplifyVec(Vector2[] vectors)
        {
            if (vectors == null) return null;
            var newVectors = new UVector2[vectors.Length];
            for (int i = 0; i < vectors.Length; i++)
            {
                var vector = vectors[i];
                newVectors[i] = new UVector2(vector.x, vector.y);
            }
            return newVectors;
        }

        private static UVector3[] FromSimplifyVec(Vector3[] vectors)
        {
            if (vectors == null) return null;
            var newVectors = new UVector3[vectors.Length];
            for (int i = 0; i < vectors.Length; i++)
            {
                var vector = vectors[i];
                newVectors[i] = new UVector3(vector.x, vector.y, vector.z);
            }
            return newVectors;
        }

        private static UVector4[] FromSimplifyVec(Vector4[] vectors)
        {
            if (vectors == null) return null;
            var newVectors = new UVector4[vectors.Length];
            for (int i = 0; i < vectors.Length; i++)
            {
                var vector = vectors[i];
                newVectors[i] = new UVector4(vector.x, vector.y, vector.z, vector.w);
            }
            return newVectors;
        }

        private static UColor[] FromSimplifyColor(Vector4[] vectors)
        {
            if (vectors == null) return null;
            var newColors = new UColor[vectors.Length];
            for (int i = 0; i < vectors.Length; i++)
            {
                var vector = vectors[i];
                newColors[i] = new UColor(vector.x, vector.y, vector.z, vector.w);
            }
            return newColors;
        }

        private static UBoneWeight[] FromSimplifyBoneWeights(BoneWeight[] boneWeights)
        {
            if (boneWeights == null) return null;
            var newBoneWeights = new UBoneWeight[boneWeights.Length];
            for (int i = 0; i < boneWeights.Length; i++)
            {
                var boneWeight = boneWeights[i];
                newBoneWeights[i] = new UBoneWeight()
                {
                    boneIndex0 = boneWeight.boneIndex0,
                    boneIndex1 = boneWeight.boneIndex1,
                    boneIndex2 = boneWeight.boneIndex2,
                    boneIndex3 = boneWeight.boneIndex3,
                    weight0 = boneWeight.boneWeight0,
                    weight1 = boneWeight.boneWeight1,
                    weight2 = boneWeight.boneWeight2,
                    weight3 = boneWeight.boneWeight3
                };
            }
            return newBoneWeights;
        }
        #endregion

        #region Add To List
        private static void AddToList(List<Vector3d> list, UVector3[] arr, int previousVertexCount, int totalVertexCount)
        {
            if (arr == null || arr.Length == 0)
                return;

            for (int i = 0; i < arr.Length; i++)
            {
                UVector3 vector = arr[i];
                list.Add(new Vector3d(vector.x, vector.y, vector.z));
            }
        }

        private static void AddToList(ref List<Vector2> list, UVector2[] arr, int previousVertexCount, int currentVertexCount, int totalVertexCount, Vector2 defaultValue)
        {
            if (arr == null || arr.Length == 0)
            {
                if (list != null)
                {
                    for (int i = 0; i < currentVertexCount; i++)
                    {
                        list.Add(defaultValue);
                    }
                }
                return;
            }

            if (list == null)
            {
                list = new List<Vector2>(totalVertexCount);
                for (int i = 0; i < previousVertexCount; i++)
                {
                    list.Add(defaultValue);
                }
            }

            for (int i = 0; i < arr.Length; i++)
            {
                UVector2 vector = arr[i];
                list.Add(new Vector2(vector.x, vector.y));
            }
        }

        private static void AddToList(ref List<Vector3> list, UVector3[] arr, int previousVertexCount, int currentVertexCount, int totalVertexCount, Vector3 defaultValue)
        {
            if (arr == null || arr.Length == 0)
            {
                if (list != null)
                {
                    for (int i = 0; i < currentVertexCount; i++)
                    {
                        list.Add(defaultValue);
                    }
                }
                return;
            }

            if (list == null)
            {
                list = new List<Vector3>(totalVertexCount);
                for (int i = 0; i < previousVertexCount; i++)
                {
                    list.Add(defaultValue);
                }
            }

            for (int i = 0; i < arr.Length; i++)
            {
                UVector3 vector = arr[i];
                list.Add(new Vector3(vector.x, vector.y, vector.z));
            }
        }

        private static void AddToList(ref List<Vector4> list, UVector4[] arr, int previousVertexCount, int currentVertexCount, int totalVertexCount, Vector4 defaultValue)
        {
            if (arr == null || arr.Length == 0)
            {
                if (list != null)
                {
                    for (int i = 0; i < currentVertexCount; i++)
                    {
                        list.Add(defaultValue);
                    }
                }
                return;
            }

            if (list == null)
            {
                list = new List<Vector4>(totalVertexCount);
                for (int i = 0; i < previousVertexCount; i++)
                {
                    list.Add(defaultValue);
                }
            }

            for (int i = 0; i < arr.Length; i++)
            {
                UVector4 vector = arr[i];
                list.Add(new Vector4(vector.x, vector.y, vector.z, vector.w));
            }
        }

        private static void AddToList(ref List<Vector4> list, UColor[] arr, int previousVertexCount, int currentVertexCount, int totalVertexCount)
        {
            if (arr == null || arr.Length == 0)
            {
                if (list != null)
                {
                    for (int i = 0; i < currentVertexCount; i++)
                    {
                        list.Add(new Vector4());
                    }
                }
                return;
            }

            if (list == null)
            {
                list = new List<Vector4>(totalVertexCount);
                for (int i = 0; i < previousVertexCount; i++)
                {
                    list.Add(new Vector4());
                }
            }

            for (int i = 0; i < arr.Length; i++)
            {
                UColor color = arr[i];
                list.Add(new Vector4(color.r, color.g, color.b, color.a));
            }
        }

        private static void AddToList(ref List<BoneWeight> list, UBoneWeight[] arr, int previousVertexCount, int currentVertexCount, int totalVertexCount)
        {
            if (arr == null || arr.Length == 0)
            {
                if (list != null)
                {
                    for (int i = 0; i < currentVertexCount; i++)
                    {
                        list.Add(new BoneWeight());
                    }
                }
                return;
            }

            if (list == null)
            {
                list = new List<BoneWeight>(totalVertexCount);
                for (int i = 0; i < previousVertexCount; i++)
                {
                    list.Add(new BoneWeight());
                }
            }

            for (int i = 0; i < arr.Length; i++)
            {
                UBoneWeight boneWeight = arr[i];
                list.Add(new BoneWeight(
                    boneWeight.boneIndex0, boneWeight.boneIndex1, boneWeight.boneIndex2, boneWeight.boneIndex3,
                    boneWeight.weight0, boneWeight.weight1, boneWeight.weight2, boneWeight.weight3));
            }
        }
        #endregion

        #region Transform Vertices
        private static void TransformVertices(UVector3[] vertices, ref UMatrix transform)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = transform.MultiplyPoint3x4(vertices[i]);
            }
        }

        private static void TransformVertices(UVector3[] vertices, UBoneWeight[] boneWeights, UMatrix[] oldBindposes, UMatrix[] newBindposes)
        {
            // TODO: Is this method doing what it is supposed to?? It has not been properly tested

            // First invert the old bindposes
            for (int i = 0; i < oldBindposes.Length; i++)
            {
                oldBindposes[i] = oldBindposes[i].inverse;
            }

            // The transform the vertices
            for (int i = 0; i < vertices.Length; i++)
            {
                UVector3 vertex = vertices[i];
                UBoneWeight boneWeight = boneWeights[i];
                if (boneWeight.weight0 > 0f)
                {
                    int boneIndex = boneWeight.boneIndex0;
                    float weight = boneWeight.weight0;
                    vertex = ScaleMatrix(ref newBindposes[boneIndex], weight) * (ScaleMatrix(ref oldBindposes[boneIndex], weight) * vertex);
                }
                if (boneWeight.weight1 > 0f)
                {
                    int boneIndex = boneWeight.boneIndex1;
                    float weight = boneWeight.weight1;
                    vertex = ScaleMatrix(ref newBindposes[boneIndex], weight) * (ScaleMatrix(ref oldBindposes[boneIndex], weight) * vertex);
                }
                if (boneWeight.weight2 > 0f)
                {
                    int boneIndex = boneWeight.boneIndex2;
                    float weight = boneWeight.weight2;
                    vertex = ScaleMatrix(ref newBindposes[boneIndex], weight) * (ScaleMatrix(ref oldBindposes[boneIndex], weight) * vertex);
                }
                if (boneWeight.weight3 > 0f)
                {
                    int boneIndex = boneWeight.boneIndex3;
                    float weight = boneWeight.weight3;
                    vertex = ScaleMatrix(ref newBindposes[boneIndex], weight) * (ScaleMatrix(ref oldBindposes[boneIndex], weight) * vertex);
                }
                vertices[i] = vertex;
            }
        }
        #endregion

        #region Scale Matrix
        private static UMatrix ScaleMatrix(ref UMatrix m, float scale)
        {
            return new UMatrix()
            {
                m00 = m.m00 * scale,
                m01 = m.m01 * scale,
                m02 = m.m02 * scale,
                m03 = m.m03 * scale,

                m10 = m.m10 * scale,
                m11 = m.m11 * scale,
                m12 = m.m12 * scale,
                m13 = m.m13 * scale,

                m20 = m.m20 * scale,
                m21 = m.m21 * scale,
                m22 = m.m22 * scale,
                m23 = m.m23 * scale,

                m30 = m.m30 * scale,
                m31 = m.m31 * scale,
                m32 = m.m32 * scale,
                m33 = m.m33 * scale
            };
        }
        #endregion
        #endregion

        #region Arrays
        private static T[] MergeArrays<T>(T[] arr1, T[] arr2)
        {
            T[] newArr = new T[arr1.Length + arr2.Length];
            Array.Copy(arr1, 0, newArr, 0, arr1.Length);
            Array.Copy(arr2, 0, newArr, arr1.Length, arr2.Length);
            return newArr;
        }
        #endregion

        #region Remap Bones
        private static void RemapBones(UBoneWeight[] boneWeights, int[] boneIndices)
        {
            for (int i = 0; i < boneWeights.Length; i++)
            {
                var boneWeight = boneWeights[i];
                if (boneWeight.weight0 > 0)
                {
                    boneWeight.boneIndex0 = boneIndices[boneWeight.boneIndex0];
                }
                if (boneWeight.weight1 > 0)
                {
                    boneWeight.boneIndex1 = boneIndices[boneWeight.boneIndex1];
                }
                if (boneWeight.weight2 > 0)
                {
                    boneWeight.boneIndex2 = boneIndices[boneWeight.boneIndex2];
                }
                if (boneWeight.weight3 > 0)
                {
                    boneWeight.boneIndex3 = boneIndices[boneWeight.boneIndex3];
                }
                boneWeights[i] = boneWeight;
            }
        }
        #endregion

        #region Create Mesh
        private static UMesh CreateMesh(UMatrix[] bindposes, UVector3[] vertices, Mesh destMesh, bool recalculateNormals)
        {
            // TODO: Support blend shapes also?
            if (recalculateNormals)
            {
                // If we recalculate the normals, we also recalculate the tangents
                destMesh.RecalculateNormals();
                destMesh.RecalculateTangents();
            }

            int subMeshCount = destMesh.SubMeshCount;
            var newNormals = FromSimplifyVec(destMesh.Normals);
            var newTangents = FromSimplifyVec(destMesh.Tangents);
            var newUV1 = FromSimplifyVec(destMesh.UV1);
            var newUV2 = FromSimplifyVec(destMesh.UV2);
            var newUV3 = FromSimplifyVec(destMesh.UV3);
            var newUV4 = FromSimplifyVec(destMesh.UV4);
            var newColors = FromSimplifyColor(destMesh.Colors);
            var newBoneWeights = FromSimplifyBoneWeights(destMesh.BoneWeights);

            UMesh newMesh = new UMesh();
            if (bindposes != null) newMesh.bindposes = bindposes;
            newMesh.subMeshCount = subMeshCount;
            newMesh.vertices = vertices;
            if (newNormals != null) newMesh.normals = newNormals;
            if (newTangents != null) newMesh.tangents = newTangents;
            if (newUV1 != null) newMesh.uv = newUV1;
            if (newUV2 != null) newMesh.uv2 = newUV2;
            if (newUV3 != null) newMesh.uv3 = newUV3;
            if (newUV4 != null) newMesh.uv4 = newUV4;
            if (newColors != null) newMesh.colors = newColors;
            if (newBoneWeights != null) newMesh.boneWeights = newBoneWeights;
            for (int i = 0; i < subMeshCount; i++)
            {
                var subMeshIndices = destMesh.GetIndices(i);
                newMesh.SetTriangles(subMeshIndices, i);
            }

            newMesh.RecalculateBounds();
            return newMesh;
        }
        #endregion
        #endregion

        #region Public Methods
        #region Decimate Mesh
        /// <summary>
        /// Decimates a mesh.
        /// </summary>
        /// <param name="mesh">The mesh to decimate.</param>
        /// <param name="transform">The mesh transform.</param>
        /// <param name="quality">The desired quality.</param>
        /// <param name="recalculateNormals">If normals should be recalculated.</param>
        /// <param name="statusCallback">The optional status report callback.</param>
        /// <returns>The decimated mesh.</returns>
        public static UMesh DecimateMesh(UMesh mesh, UMatrix transform, float quality, bool recalculateNormals, DecimationAlgorithm.StatusReportCallback statusCallback = null)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            int subMeshCount = mesh.subMeshCount;
            var meshVertices = mesh.vertices;
            var meshNormals = mesh.normals;
            var meshTangents = mesh.tangents;
            var meshUV1 = mesh.uv;
            var meshUV2 = mesh.uv2;
            var meshUV3 = mesh.uv3;
            var meshUV4 = mesh.uv4;
            var meshColors = mesh.colors;
            var meshBoneWeights = mesh.boneWeights;
            var meshBindposes = mesh.bindposes;

            int totalTriangleCount = 0;
            var meshIndices = new int[subMeshCount][];
            for (int i = 0; i < subMeshCount; i++)
            {
                meshIndices[i] = mesh.GetTriangles(i);
                totalTriangleCount += meshIndices[i].Length / 3;
            }

            // Transforms the vertices
            TransformVertices(meshVertices, ref transform);

            var vertices = ToSimplifyVertices(meshVertices);
            quality = UMath.Clamp01(quality);
            int targetTriangleCount = UMath.CeilToInt(totalTriangleCount * quality);
            var sourceMesh = new Mesh(vertices, meshIndices);

            if (meshNormals != null && meshNormals.Length > 0)
            {
                sourceMesh.Normals = ToSimplifyVec(meshNormals);
            }
            if (meshTangents != null && meshTangents.Length > 0)
            {
                sourceMesh.Tangents = ToSimplifyVec(meshTangents);
            }
            if (meshUV1 != null && meshUV1.Length > 0)
            {
                sourceMesh.UV1 = ToSimplifyVec(meshUV1);
            }
            if (meshUV2 != null && meshUV2.Length > 0)
            {
                sourceMesh.UV2 = ToSimplifyVec(meshUV2);
            }
            if (meshUV3 != null && meshUV3.Length > 0)
            {
                sourceMesh.UV3 = ToSimplifyVec(meshUV3);
            }
            if (meshUV4 != null && meshUV4.Length > 0)
            {
                sourceMesh.UV4 = ToSimplifyVec(meshUV4);
            }
            if (meshColors != null && meshColors.Length > 0)
            {
                sourceMesh.Colors = ToSimplifyVec(meshColors);
            }
            if (meshBoneWeights != null && meshBoneWeights.Length > 0)
            {
                sourceMesh.BoneWeights = ToSimplifyBoneWeights(meshBoneWeights);
            }

            var algorithm = MeshDecimation.CreateAlgorithm(Algorithm.Default);
            algorithm.MaxVertexCount = ushort.MaxValue;
            if (statusCallback != null)
            {
                algorithm.StatusReport += statusCallback;
            }

            var destMesh = MeshDecimation.DecimateMesh(algorithm, sourceMesh, targetTriangleCount);
            var newMeshVertices = FromSimplifyVertices(destMesh.Vertices);
            if (statusCallback != null)
            {
                algorithm.StatusReport -= statusCallback;
            }

            return CreateMesh(meshBindposes, newMeshVertices, destMesh, recalculateNormals);
        }
        #endregion

        #region Decimate Meshes
        /// <summary>
        /// Decimates an array of meshes, and combines them into one mesh.
        /// </summary>
        /// <param name="meshes">The meshes to decimate.</param>
        /// <param name="transforms">The mesh transforms.</param>
        /// <param name="materials">The mesh materials, including submesh materials.</param>
        /// <param name="quality">The desired quality.</param>
        /// <param name="recalculateNormals">If normals should be recalculated.</param>
        /// <param name="resultMaterials">The resulting materials array.</param>
        /// <param name="statusCallback">The optional status report callback.</param>
        /// <returns>The decimated mesh.</returns>
        public static UMesh DecimateMeshes(UMesh[] meshes, UMatrix[] transforms, UMaterial[][] materials, float quality, bool recalculateNormals, out UMaterial[] resultMaterials, DecimationAlgorithm.StatusReportCallback statusCallback = null)
        {
            UTransform[] mergedBones;
            return DecimateMeshes(meshes, transforms, materials, null, quality, recalculateNormals, out resultMaterials, out mergedBones, statusCallback);
        }

        /// <summary>
        /// Decimates an array of meshes, and combines them into one mesh.
        /// </summary>
        /// <param name="meshes">The meshes to decimate.</param>
        /// <param name="transforms">The mesh transforms.</param>
        /// <param name="materials">The mesh materials, with submesh materials.</param>
        /// <param name="meshBones">The bones for each mesh.</param>
        /// <param name="quality">The desired quality.</param>
        /// <param name="recalculateNormals">If normals should be recalculated.</param>
        /// <param name="resultMaterials">The resulting materials array.</param>
        /// <param name="mergedBones">The output merged bones.</param>
        /// <param name="statusCallback">The optional status report callback.</param>
        /// <returns>The decimated mesh.</returns>
        public static UMesh DecimateMeshes(UMesh[] meshes, UMatrix[] transforms, UMaterial[][] materials, UTransform[][] meshBones, float quality, bool recalculateNormals, out UMaterial[] resultMaterials, out UTransform[] mergedBones, DecimationAlgorithm.StatusReportCallback statusCallback = null)
        {
            if (meshes == null)
                throw new ArgumentNullException("meshes");
            else if (meshes.Length == 0)
                throw new ArgumentException("You have to simplify at least one mesh.", "meshes");
            else if (transforms == null)
                throw new ArgumentNullException("transforms");
            else if (transforms.Length != meshes.Length)
                throw new ArgumentException("The array of transforms must match the length of the meshes array.", "transforms");
            else if (materials == null)
                throw new ArgumentNullException("materials");
            else if (materials.Length != meshes.Length)
                throw new ArgumentException("If materials are provided, the length of the array must match the length of the meshes array.", "materials");
            else if (meshBones != null && meshBones.Length != meshes.Length)
                throw new ArgumentException("If mesh bones are provided, the length of the array must match the length of the meshes array.", "meshBones");

            int totalVertexCount = 0;
            int totalSubMeshCount = 0;
            for (int meshIndex = 0; meshIndex < meshes.Length; meshIndex++)
            {
                UMesh mesh = meshes[meshIndex];
                totalVertexCount += mesh.vertexCount;
                totalSubMeshCount += mesh.subMeshCount;

                var meshMaterials = materials[meshIndex];
                if (meshMaterials == null)
                    throw new ArgumentException(string.Format("The mesh materials for index {0} is null!", meshIndex), "materials");
                else if (meshMaterials.Length != mesh.subMeshCount)
                    throw new ArgumentException(string.Format("The mesh materials at index {0} don't match the submesh count! ({1} != {2})", meshIndex, meshMaterials.Length, mesh.subMeshCount), "materials");

                for (int i = 0; i < meshMaterials.Length; i++)
                {
                    if (meshMaterials[i] == null)
                        throw new ArgumentException(string.Format("The mesh material index {0} at material array index {1} is null!", i, meshIndex), "materials");
                }
            }

            int totalTriangleCount = 0;
            var vertices = new List<Vector3d>(totalVertexCount);
            var indices = new List<int[]>(totalSubMeshCount);

            List<Vector3> normals = null;
            List<Vector4> tangents = null;
            List<Vector2> uv1 = null;
            List<Vector2> uv2 = null;
            List<Vector2> uv3 = null;
            List<Vector2> uv4 = null;
            List<Vector4> colors = null;
            List<BoneWeight> boneWeights = null;

            List<UMatrix> usedBindposes = null;
            List<UTransform> usedBones = null;
            List<UMaterial> usedMaterials = new List<UMaterial>(totalSubMeshCount);
            Dictionary<UMaterial, int> materialMap = new Dictionary<UMaterial, int>();
            int currentVertexCount = 0;
            for (int meshIndex = 0; meshIndex < meshes.Length; meshIndex++)
            {
                var mesh = meshes[meshIndex];
                var transform = transforms[meshIndex];
                var meshMaterials = materials[meshIndex];
                var bones = (meshBones != null ? meshBones[meshIndex] : null);

                int meshVertexCount = mesh.vertexCount;
                var meshVertices = mesh.vertices;
                var meshNormals = mesh.normals;
                var meshTangents = mesh.tangents;
                var meshUV1 = mesh.uv;
                var meshUV2 = mesh.uv2;
                var meshUV3 = mesh.uv3;
                var meshUV4 = mesh.uv4;
                var meshColors = mesh.colors;
                var meshBoneWeights = mesh.boneWeights;
                var meshBindposes = mesh.bindposes;

                if (bones != null && meshBoneWeights != null && meshBoneWeights.Length > 0 && meshBindposes != null && meshBindposes.Length > 0 && bones.Length == meshBindposes.Length)
                {
                    if (usedBindposes == null)
                    {
                        usedBindposes = new List<UMatrix>(meshBindposes);
                        usedBones = new List<UTransform>(bones);
                    }
                    else
                    {
                        bool bindPoseMismatch = false;
                        int[] boneIndices = new int[bones.Length];
                        for (int i = 0; i < bones.Length; i++)
                        {
                            int usedBoneIndex = usedBones.IndexOf(bones[i]);
                            if (usedBoneIndex == -1)
                            {
                                usedBoneIndex = usedBones.Count;
                                usedBones.Add(bones[i]);
                                usedBindposes.Add(meshBindposes[i]);
                            }
                            else
                            {
                                if (meshBindposes[i] != usedBindposes[usedBoneIndex])
                                {
                                    bindPoseMismatch = true;
                                }
                            }
                            boneIndices[i] = usedBoneIndex;
                        }

                        // If any bindpose is mismatching, we correct it first
                        if (bindPoseMismatch)
                        {
                            var correctedBindposes = new UMatrix[meshBindposes.Length];
                            for (int i = 0; i < meshBindposes.Length; i++)
                            {
                                int usedBoneIndex = boneIndices[i];
                                correctedBindposes[i] = usedBindposes[usedBoneIndex];
                            }
                            TransformVertices(meshVertices, meshBoneWeights, meshBindposes, correctedBindposes);
                        }

                        // Then we remap the bones
                        RemapBones(meshBoneWeights, boneIndices);
                    }
                }

                // Transforms the vertices
                TransformVertices(meshVertices, ref transform);

                AddToList(vertices, meshVertices, currentVertexCount, totalVertexCount);
                AddToList(ref normals, meshNormals, currentVertexCount, meshVertexCount, totalVertexCount, new Vector3(1f, 0f, 0f));
                AddToList(ref tangents, meshTangents, currentVertexCount, meshVertexCount, totalVertexCount, new Vector4(0f, 0f, 1f, 1f)); // Is the default value correct?
                AddToList(ref uv1, meshUV1, currentVertexCount, meshVertexCount, totalVertexCount, new Vector2());
                AddToList(ref uv2, meshUV2, currentVertexCount, meshVertexCount, totalVertexCount, new Vector2());
                AddToList(ref uv3, meshUV3, currentVertexCount, meshVertexCount, totalVertexCount, new Vector2());
                AddToList(ref uv4, meshUV4, currentVertexCount, meshVertexCount, totalVertexCount, new Vector2());
                AddToList(ref colors, meshColors, currentVertexCount, meshVertexCount, totalVertexCount);
                AddToList(ref boneWeights, meshBoneWeights, currentVertexCount, meshVertexCount, totalVertexCount);

                int subMeshCount = mesh.subMeshCount;
                for (int subMeshIndex = 0; subMeshIndex < subMeshCount; subMeshIndex++)
                {
                    var subMeshMaterial = meshMaterials[subMeshIndex];
                    int[] subMeshIndices = mesh.GetTriangles(subMeshIndex);
                    if (currentVertexCount > 0)
                    {
                        for (int i = 0; i < subMeshIndices.Length; i++)
                        {
                            subMeshIndices[i] += currentVertexCount;
                        }
                    }
                    totalTriangleCount += subMeshIndices.Length / 3;

                    int mergeWithIndex;
                    if (materialMap.TryGetValue(subMeshMaterial, out mergeWithIndex))
                    {
                        int[] currentIndices = indices[mergeWithIndex];
                        indices[mergeWithIndex] = MergeArrays(currentIndices, subMeshIndices);
                    }
                    else
                    {
                        materialMap.Add(subMeshMaterial, indices.Count);
                        usedMaterials.Add(subMeshMaterial);
                        indices.Add(subMeshIndices);
                    }
                }
                currentVertexCount += meshVertices.Length;
            }

            quality = UMath.Clamp01(quality);
            int targetTriangleCount = UMath.CeilToInt(totalTriangleCount * quality);
            var sourceMesh = new Mesh(vertices.ToArray(), indices.ToArray());

            if (normals != null)
            {
                sourceMesh.Normals = normals.ToArray();
            }
            if (tangents != null)
            {
                sourceMesh.Tangents = tangents.ToArray();
            }
            if (uv1 != null)
            {
                sourceMesh.UV1 = uv1.ToArray();
            }
            if (uv2 != null)
            {
                sourceMesh.UV2 = uv2.ToArray();
            }
            if (uv3 != null)
            {
                sourceMesh.UV3 = uv3.ToArray();
            }
            if (uv4 != null)
            {
                sourceMesh.UV4 = uv4.ToArray();
            }
            if (colors != null)
            {
                sourceMesh.Colors = colors.ToArray();
            }
            if (boneWeights != null)
            {
                sourceMesh.BoneWeights = boneWeights.ToArray();
            }

            var algorithm = MeshDecimation.CreateAlgorithm(Algorithm.Default);
            algorithm.MaxVertexCount = ushort.MaxValue;
            if (statusCallback != null)
            {
                algorithm.StatusReport += statusCallback;
            }

            var destMesh = MeshDecimation.DecimateMesh(algorithm, sourceMesh, targetTriangleCount);
            var newMeshVertices = FromSimplifyVertices(destMesh.Vertices);
            if (statusCallback != null)
            {
                algorithm.StatusReport -= statusCallback;
            }

            var bindposes = (usedBindposes != null ? usedBindposes.ToArray() : null);
            resultMaterials = usedMaterials.ToArray();
            mergedBones = (usedBones != null ? usedBones.ToArray() : null);
            return CreateMesh(bindposes, newMeshVertices, destMesh, recalculateNormals);
        }
        #endregion
        #endregion
    }
}