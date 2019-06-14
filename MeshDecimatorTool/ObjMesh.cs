﻿#region License
/*
MIT License

Copyright(c) 2017-2018 Mattias Edlund

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using MeshDecimator.Math;

namespace MeshDecimatorTool
{
    /// <summary>
    /// A very simple OBJ mesh.
    /// </summary>
    public sealed class ObjMesh
    {
        #region Consts
        private const int VertexInitialCapacity = 20000;
        private const int IndexInitialCapacity = 40000;
        #endregion

        #region Structs
        private struct FaceIndex : IEquatable<FaceIndex>
        {
            public readonly int vertexIndex;
            public readonly int texCoordIndex;
            public readonly int normalIndex;
            private readonly int hashCode;

            public FaceIndex(int vertexIndex, int texCoordIndex, int normalIndex)
            {
                this.vertexIndex = vertexIndex;
                this.texCoordIndex = texCoordIndex;
                this.normalIndex = normalIndex;
                this.hashCode = (vertexIndex ^ texCoordIndex << 2 ^ normalIndex >> 2);
            }

            public override int GetHashCode()
            {
                return hashCode;
            }

            public override bool Equals(object obj)
            {
                if (obj is FaceIndex)
                {
                    var other = (FaceIndex)obj;
                    return (vertexIndex == other.vertexIndex && texCoordIndex == other.texCoordIndex && normalIndex == other.normalIndex);
                }
                return false;
            }

            public bool Equals(FaceIndex other)
            {
                return (vertexIndex == other.vertexIndex && texCoordIndex == other.texCoordIndex && normalIndex == other.normalIndex);
            }

            public override string ToString()
            {
                return string.Format("{{Vertex:{0}, TexCoord:{1}, Normal:{2}}}", vertexIndex, texCoordIndex, normalIndex);
            }
        }
        #endregion

        #region Fields
        private Vector3d[] vertices = null;
        private Vector3[] normals = null;
        private Vector2[] texCoords2D = null;
        private Vector3[] texCoords3D = null;
        private int[][] subMeshIndices = null;
        private string[] subMeshMaterials = null;

        private string[] materialLibraries = null;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the vertices for this mesh.
        /// </summary>
        public Vector3d[] Vertices
        {
            get { return vertices; }
            set { vertices = value; }
        }

        /// <summary>
        /// Gets or sets the normals for this mesh.
        /// </summary>
        public Vector3[] Normals
        {
            get { return normals; }
            set { normals = value; }
        }

        /// <summary>
        /// Gets or sets the 2D texture coordinates for this mesh.
        /// </summary>
        public Vector2[] TexCoords2D
        {
            get { return texCoords2D; }
            set
            {
                texCoords3D = null;
                texCoords2D = value;
            }
        }

        /// <summary>
        /// Gets or sets the 3D texture coordinates for this mesh.
        /// </summary>
        public Vector3[] TexCoords3D
        {
            get { return texCoords3D; }
            set
            {
                texCoords2D = null;
                texCoords3D = value;
            }
        }

        /// <summary>
        /// Gets the count of sub-meshes in this mesh.
        /// </summary>
        public int SubMeshCount
        {
            get { return (subMeshIndices != null ? subMeshIndices.Length : 0); }
        }

        /// <summary>
        /// Gets or sets the combined triangle indices for this mesh.
        /// Note that setting this will remove any existing sub-meshes and turn it into just one sub-mesh.
        /// </summary>
        [Obsolete("Prefer to use the 'SubMeshIndices' property instead.", false)]
        public int[] Indices
        {
            get
            {
                if (subMeshIndices == null)
                    return null;

                int combinedIndexCount = 0;
                for (int i = 0; i < subMeshIndices.Length; i++)
                {
                    combinedIndexCount += subMeshIndices[i].Length;
                }

                var combinedIndices = new int[combinedIndexCount];
                int offset = 0;
                for (int i = 0; i < subMeshIndices.Length; i++)
                {
                    var indices = subMeshIndices[i];
                    Array.Copy(indices, 0, combinedIndices, offset, indices.Length);
                    offset += indices.Length;
                }
                return combinedIndices;
            }
            set
            {
                if (value != null)
                {
                    subMeshIndices = new int[][] { value };
                }
                else
                {
                    subMeshIndices = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the indices divided by sub-meshes.
        /// </summary>
        public int[][] SubMeshIndices
        {
            get { return subMeshIndices; }
            set
            {
                if (value != null)
                {
                    for (int i = 0; i < value.Length; i++)
                    {
                        if (value[i] == null)
                            throw new ArgumentException(string.Format("The sub-mesh index array at index {0} is null.", i), "value");
                    }
                }

                subMeshIndices = value;
            }
        }

        /// <summary>
        /// Gets or sets the names of each sub-mesh material.
        /// </summary>
        public string[] SubMeshMaterials
        {
            get { return subMeshMaterials; }
            set
            {
                if (value != null)
                {
                    for (int i = 0; i < value.Length; i++)
                    {
                        if (value[i] == null)
                            throw new ArgumentException(string.Format("The sub-mesh material name at index {0} is null.", i), "value");
                    }
                }

                subMeshMaterials = value;
            }
        }

        /// <summary>
        /// Gets or sets the paths to material libraries used by this mesh.
        /// </summary>
        public string[] MaterialLibraries
        {
            get { return materialLibraries; }
            set { materialLibraries = value; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new OBJ mesh.
        /// </summary>
        public ObjMesh()
        {

        }

        /// <summary>
        /// Creates a new OBJ mesh.
        /// </summary>
        /// <param name="vertices">The mesh vertices.</param>
        /// <param name="indices">The mesh indices.</param>
        public ObjMesh(Vector3d[] vertices, int[] indices)
        {
            this.vertices = vertices;
            this.subMeshIndices = (indices != null ? new int[][] { indices } : null);
        }

        /// <summary>
        /// Creates a new OBJ mesh.
        /// </summary>
        /// <param name="vertices">The mesh vertices.</param>
        /// <param name="indices">The mesh indices.</param>
        public ObjMesh(Vector3d[] vertices, int[][] indices)
        {
            this.vertices = vertices;
            this.subMeshIndices = indices;
        }
        #endregion

        #region Public Methods
        #region Read File
        /// <summary>
        /// Reads an OBJ mesh from a file.
        /// Please note that this method only supports extremely simple OBJ meshes.
        /// </summary>
        /// <param name="path">The file path.</param>
        public void ReadFile(string path)
        {
            var materialLibraryList = new List<string>();
            var readVertexList = new List<Vector3d>(VertexInitialCapacity);
            List<Vector3> readNormalList = null;
            List<Vector3> readTexCoordList = null;
            var vertexList = new List<Vector3d>(VertexInitialCapacity);
            List<Vector3> normalList = null;
            List<Vector3> texCoordList = null;
            var triangleIndexList = new List<int>(IndexInitialCapacity);
            var subMeshIndicesList = new List<int[]>();
            var subMeshMaterialList = new List<string>();
            var faceTable = new Dictionary<FaceIndex, int>(IndexInitialCapacity);
            var tempFaceList = new List<int>(6);
            bool texCoordsAre3D = false;

            string currentGroup = null;
            string currentObject = null;
            string currentMaterial = null;
            int newFaceIndex = 0;
            using (StreamReader reader = File.OpenText(path))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Length == 0 || line[0] == '#')
                        continue;

                    string[] lineSplit = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    string firstPart = lineSplit[0];
                    if (string.Equals(firstPart, "v"))
                    {
                        if (lineSplit.Length < 4)
                            throw new InvalidDataException("Vertices needs at least 3 components.");

                        double f0, f1, f2;
                        double.TryParse(lineSplit[1], NumberStyles.Float, CultureInfo.InvariantCulture, out f0);
                        double.TryParse(lineSplit[2], NumberStyles.Float, CultureInfo.InvariantCulture, out f1);
                        double.TryParse(lineSplit[3], NumberStyles.Float, CultureInfo.InvariantCulture, out f2);
                        readVertexList.Add(new Vector3d(f0, f1, f2));
                    }
                    else if (string.Equals(firstPart, "vn"))
                    {
                        if (lineSplit.Length != 4)
                            throw new InvalidDataException("Normals must be 3 components.");

                        if (readNormalList == null)
                            readNormalList = new List<Vector3>(VertexInitialCapacity);

                        float f0, f1, f2;
                        float.TryParse(lineSplit[1], NumberStyles.Float, CultureInfo.InvariantCulture, out f0);
                        float.TryParse(lineSplit[2], NumberStyles.Float, CultureInfo.InvariantCulture, out f1);
                        float.TryParse(lineSplit[3], NumberStyles.Float, CultureInfo.InvariantCulture, out f2);
                        readNormalList.Add(new Vector3(f0, f1, f2));
                    }
                    else if (string.Equals(firstPart, "vt"))
                    {
                        if (lineSplit.Length < 3)
                            throw new InvalidDataException("Texture coordinates needs at least 2 components.");

                        if (readTexCoordList == null)
                            readTexCoordList = new List<Vector3>(VertexInitialCapacity);

                        float f0, f1, f2;
                        float.TryParse(lineSplit[1], NumberStyles.Float, CultureInfo.InvariantCulture, out f0);
                        float.TryParse(lineSplit[2], NumberStyles.Float, CultureInfo.InvariantCulture, out f1);
                        if (lineSplit.Length > 3)
                        {
                            float.TryParse(lineSplit[3], NumberStyles.Float, CultureInfo.InvariantCulture, out f2);

                            if (!texCoordsAre3D && f2 != 0f)
                            {
                                texCoordsAre3D = true;
                            }
                        }
                        else
                        {
                            f2 = 0f;
                        }
                        readTexCoordList.Add(new Vector3(f0, f1, f2));
                    }
                    else if (string.Equals(firstPart, "f"))
                    {
                        if (lineSplit.Length < 4)
                            throw new InvalidDataException("Faces must have at least three indices.");

                        int vertexIndex, texIndex, normalIndex;
                        tempFaceList.Clear();
                        for (int i = 1; i < lineSplit.Length; i++)
                        {
                            string word = lineSplit[i];
                            int slashCount = CountOccurrences(word, '/');
                            if (slashCount == 0)
                            {
                                int.TryParse(word, out vertexIndex);
                                vertexIndex = ShiftIndex(vertexIndex, readVertexList.Count);
                                texIndex = -1;
                                normalIndex = -1;
                            }
                            else if (slashCount == 1)
                            {
                                int splitIndex = word.IndexOf('/');
                                string word1 = word.Substring(0, splitIndex);
                                string word2 = word.Substring(splitIndex + 1);
                                int.TryParse(word1, out vertexIndex);
                                int.TryParse(word2, out texIndex);
                                vertexIndex = ShiftIndex(vertexIndex, readVertexList.Count);
                                texIndex = ShiftIndex(texIndex, readTexCoordList.Count);
                                normalIndex = -1;
                            }
                            else if (slashCount == 2)
                            {
                                int splitIndex1 = word.IndexOf('/');
                                int splitIndex2 = word.IndexOf('/', splitIndex1 + 1);
                                string word1 = word.Substring(0, splitIndex1);
                                string word2 = word.Substring(splitIndex1 + 1, splitIndex2 - splitIndex1 - 1);
                                string word3 = word.Substring(splitIndex2 + 1);
                                int.TryParse(word1, out vertexIndex);
                                bool hasTexCoord = int.TryParse(word2, out texIndex);
                                int.TryParse(word3, out normalIndex);
                                vertexIndex = ShiftIndex(vertexIndex, readVertexList.Count);
                                if (hasTexCoord)
                                {
                                    texIndex = ShiftIndex(texIndex, readTexCoordList.Count);
                                }
                                else
                                {
                                    texIndex = -1;
                                }
                                normalIndex = ShiftIndex(normalIndex, readNormalList.Count);
                            }
                            else
                            {
                                throw new InvalidDataException(string.Format("Invalid face data are supported (expected a maximum of two slashes, but found {0}.", slashCount));
                            }

                            int faceIndex;
                            var face = new FaceIndex(vertexIndex, texIndex, normalIndex);
                            if (faceTable.TryGetValue(face, out faceIndex))
                            {
                                tempFaceList.Add(faceIndex);
                            }
                            else
                            {
                                faceTable[face] = newFaceIndex;
                                tempFaceList.Add(newFaceIndex);
                                ++newFaceIndex;

                                vertexList.Add(readVertexList[vertexIndex]);

                                if (readNormalList != null)
                                {
                                    if (normalList == null)
                                        normalList = new List<Vector3>(VertexInitialCapacity);

                                    if (normalIndex >= 0 && normalIndex < readNormalList.Count)
                                    {
                                        normalList.Add(readNormalList[normalIndex]);
                                    }
                                    else
                                    {
                                        normalList.Add(Vector3.zero);
                                    }
                                }

                                if (readTexCoordList != null)
                                {
                                    if (texCoordList == null)
                                        texCoordList = new List<Vector3>(VertexInitialCapacity);

                                    if (texIndex >= 0 && texIndex < readTexCoordList.Count)
                                    {
                                        texCoordList.Add(readTexCoordList[texIndex]);
                                    }
                                    else
                                    {
                                        texCoordList.Add(Vector3.zero);
                                    }
                                }
                            }
                        }

                        // Convert into triangles (currently we only support triangles and quads)
                        int faceIndexCount = tempFaceList.Count;
                        if (faceIndexCount >= 3 && faceIndexCount < 5)
                        {
                            triangleIndexList.Add(tempFaceList[0]);
                            triangleIndexList.Add(tempFaceList[1]);
                            triangleIndexList.Add(tempFaceList[2]);

                            if (faceIndexCount > 3)
                            {
                                triangleIndexList.Add(tempFaceList[2]);
                                triangleIndexList.Add(tempFaceList[3]);
                                triangleIndexList.Add(tempFaceList[0]);
                            }
                        }
                    }
                    else if (string.Equals(firstPart, "g"))
                    {
                        string groupName = string.Join(" ", lineSplit, 1, lineSplit.Length - 1);
                        currentGroup = groupName;
                    }
                    else if (string.Equals(firstPart, "o"))
                    {
                        string objectName = string.Join(" ", lineSplit, 1, lineSplit.Length - 1);
                        currentObject = objectName;
                    }
                    else if (string.Equals(firstPart, "mtllib"))
                    {
                        string materialLibraryPath = string.Join(" ", lineSplit, 1, lineSplit.Length - 1);
                        materialLibraryList.Add(materialLibraryPath);
                    }
                    else if (string.Equals(firstPart, "usemtl"))
                    {
                        string materialName = string.Join(" ", lineSplit, 1, lineSplit.Length - 1);
                        currentMaterial = materialName;

                        if (triangleIndexList.Count > 0)
                        {
                            subMeshIndicesList.Add(triangleIndexList.ToArray());
                            triangleIndexList.Clear();

                            if (subMeshMaterialList.Count != subMeshIndicesList.Count)
                            {
                                subMeshMaterialList.Add("none");
                            }
                        }

                        subMeshMaterialList.Add(materialName);
                    }
                }
            }

            if (triangleIndexList.Count > 0)
            {
                subMeshIndicesList.Add(triangleIndexList.ToArray());
                triangleIndexList.Clear();

                if (currentMaterial == null)
                {
                    subMeshMaterialList.Add("none");
                }
            }

            int subMeshCount = subMeshIndicesList.Count;
            bool hasNormals = (readNormalList != null);
            bool hasTexCoords = (readTexCoordList != null);
            int vertexCount = vertexList.Count;
            var processedVertexList = new List<Vector3d>(vertexCount);
            var processedNormalList = (hasNormals ? new List<Vector3>(vertexCount) : null);
            var processedTexCoordList = (hasTexCoords ? new List<Vector3>(vertexCount) : null);
            var processedIndices = new List<int[]>(subMeshCount);
            var indexMappings = new Dictionary<int, int>(IndexInitialCapacity);

            for (int subMeshIndex = 0; subMeshIndex < subMeshCount; subMeshIndex++)
            {
                var indices = subMeshIndicesList[subMeshIndex];
                for (int i = 0; i < indices.Length; i++)
                {
                    int index = indices[i];
                    int mappedIndex;
                    if (indexMappings.TryGetValue(index, out mappedIndex))
                    {
                        indices[i] = mappedIndex;
                    }
                    else
                    {
                        processedVertexList.Add(vertexList[index]);
                        if (hasNormals)
                        {
                            processedNormalList.Add(normalList[index]);
                        }
                        if (hasTexCoords)
                        {
                            processedTexCoordList.Add(texCoordList[index]);
                        }

                        mappedIndex = processedVertexList.Count - 1;
                        indexMappings[index] = mappedIndex;
                        indices[i] = mappedIndex;
                    }
                }

                processedIndices.Add(indices);
            }

            vertices = processedVertexList.ToArray();
            normals = (processedNormalList != null ? processedNormalList.ToArray() : null);

            if (processedTexCoordList != null)
            {
                if (texCoordsAre3D)
                {
                    texCoords3D = processedTexCoordList.ToArray();
                }
                else
                {
                    int texCoordCount = processedTexCoordList.Count;
                    texCoords2D = new Vector2[texCoordCount];
                    for (int i = 0; i < texCoordCount; i++)
                    {
                        var texCoord = processedTexCoordList[i];
                        texCoords2D[i] = new Vector2(texCoord.x, texCoord.y);
                    }
                }
            }
            else
            {
                texCoords2D = null;
                texCoords3D = null;
            }

            /*
            vertices = new Vector3d[faceCount];
            for (int i = 0; i < faceCount; i++)
            {
                int vertexIndex = faceList[i].x;
                vertices[i] = vertexList[vertexIndex];
            }

            if (normalList != null)
            {
                int normalCount = normalList.Count;
                normals = new Vector3[faceCount];
                for (int i = 0; i < faceCount; i++)
                {
                    int normalIndex = faceList[i].z;
                    if (normalIndex >= 0 && normalIndex < normalCount)
                    {
                        normals[i] = normalList[normalIndex];
                    }
                }
            }
            else
            {
                normals = null;
            }

            if (texCoordList != null)
            {
                int texCoordCount = texCoordList.Count;
                if (texCoordsAre3D)
                {
                    texCoords3D = new Vector3[faceCount];
                    for (int i = 0; i < faceCount; i++)
                    {
                        int texCoordIndex = faceList[i].y;
                        if (texCoordIndex >= 0 && texCoordIndex < texCoordCount)
                        {
                            texCoords3D[i] = texCoordList[texCoordIndex];
                        }
                    }
                }
                else
                {
                    texCoords2D = new Vector2[faceCount];
                    for (int i = 0; i < faceCount; i++)
                    {
                        int texCoordIndex = faceList[i].y;
                        if (texCoordIndex >= 0 && texCoordIndex < texCoordCount)
                        {
                            var texCoord = texCoordList[texCoordIndex];
                            texCoords2D[i] = new Vector2(texCoord.x, texCoord.y);
                        }
                    }
                }
            }
            else
            {
                texCoords2D = null;
                texCoords3D = null;
            }
            */

            subMeshIndices = processedIndices.ToArray();
            subMeshMaterials = subMeshMaterialList.ToArray();
            materialLibraries = materialLibraryList.ToArray();
        }
        #endregion

        #region Write File
        /// <summary>
        /// Writes this OBJ mesh to a file.
        /// </summary>
        /// <param name="path">The file path.</param>
        public void WriteFile(string path)
        {
            if (vertices == null)
                throw new InvalidOperationException("There are no vertices to write for this mesh.");
            else if (subMeshIndices == null)
                throw new InvalidOperationException("There are no indices to write for this mesh.");
            else if (subMeshMaterials != null && subMeshMaterials.Length != subMeshIndices.Length)
                throw new InvalidOperationException("The number of sub-mesh material names does not match the count of sub-mesh index arrays.");

            // TODO: Optimize the output by sharing vertices, normals, etc

            using (StreamWriter writer = File.CreateText(path))
            {
                if (materialLibraries != null && materialLibraries.Length > 0)
                {
                    for (int i = 0; i < materialLibraries.Length; i++)
                    {
                        string materialLibraryPath = materialLibraries[i];
                        writer.Write("mtllib ");
                        writer.WriteLine(materialLibraryPath);
                    }

                    writer.WriteLine();
                }

                WriteVertices(writer, vertices);
                WriteNormals(writer, normals);
                WriteTextureCoords(writer, texCoords2D, texCoords3D);

                bool hasTexCoords = (texCoords2D != null || texCoords3D != null);
                bool hasNormals = (normals != null);
                WriteSubMeshes(writer, subMeshIndices, subMeshMaterials, hasTexCoords, hasNormals);
            }
        }
        #endregion
        #endregion

        #region Private Methods
        private static void WriteVertices(TextWriter writer, Vector3d[] vertices)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                var vertex = vertices[i];
                writer.Write("v ");
                writer.Write(vertex.x.ToString("g", CultureInfo.InvariantCulture));
                writer.Write(' ');
                writer.Write(vertex.y.ToString("g", CultureInfo.InvariantCulture));
                writer.Write(' ');
                writer.Write(vertex.z.ToString("g", CultureInfo.InvariantCulture));
                writer.WriteLine();
            }
        }

        private static void WriteNormals(TextWriter writer, Vector3[] normals)
        {
            if (normals == null)
                return;

            for (int i = 0; i < normals.Length; i++)
            {
                var normal = normals[i];
                writer.Write("vn ");
                writer.Write(normal.x.ToString("g", CultureInfo.InvariantCulture));
                writer.Write(' ');
                writer.Write(normal.y.ToString("g", CultureInfo.InvariantCulture));
                writer.Write(' ');
                writer.Write(normal.z.ToString("g", CultureInfo.InvariantCulture));
                writer.WriteLine();
            }
        }

        private static void WriteTextureCoords(TextWriter writer, Vector2[] texCoords2D, Vector3[] texCoords3D)
        {
            if (texCoords2D != null)
            {
                for (int i = 0; i < texCoords2D.Length; i++)
                {
                    var texCoord = texCoords2D[i];
                    writer.Write("vt ");
                    writer.Write(texCoord.x.ToString("g", CultureInfo.InvariantCulture));
                    writer.Write(' ');
                    writer.Write(texCoord.y.ToString("g", CultureInfo.InvariantCulture));
                    writer.WriteLine();
                }
            }
            else if (texCoords3D != null)
            {
                for (int i = 0; i < texCoords3D.Length; i++)
                {
                    var texCoord = texCoords3D[i];
                    writer.Write("vt ");
                    writer.Write(texCoord.x.ToString("g", CultureInfo.InvariantCulture));
                    writer.Write(' ');
                    writer.Write(texCoord.y.ToString("g", CultureInfo.InvariantCulture));
                    writer.Write(' ');
                    writer.Write(texCoord.z.ToString("g", CultureInfo.InvariantCulture));
                    writer.WriteLine();
                }
            }
        }

        private static void WriteSubMeshes(TextWriter writer, int[][] subMeshIndices, string[] subMeshMaterials, bool hasTexCoords, bool hasNormals)
        {
            for (int subMeshIndex = 0; subMeshIndex < subMeshIndices.Length; subMeshIndex++)
            {
                var indices = subMeshIndices[subMeshIndex];

                writer.WriteLine();
                writer.WriteLine("# Sub-mesh {0}", (subMeshIndex + 1));

                if (subMeshMaterials != null)
                {
                    string materialName = subMeshMaterials[subMeshIndex];
                    writer.Write("usemtl ");
                    writer.WriteLine(materialName);
                }

                WriteFaces(writer, indices, hasTexCoords, hasNormals);
            }
        }

        private static void WriteFaces(TextWriter writer, int[] indices, bool hasTexCoords, bool hasNormals)
        {
            for (int i = 0; i < indices.Length; i += 3)
            {
                int v0 = indices[i] + 1;
                int v1 = indices[i + 1] + 1;
                int v2 = indices[i + 2] + 1;

                writer.Write("f ");

                if (hasTexCoords && hasNormals)
                {
                    writer.Write(v0);
                    writer.Write('/');
                    writer.Write(v0);
                    writer.Write('/');
                    writer.Write(v0);
                    writer.Write(' ');
                    writer.Write(v1);
                    writer.Write('/');
                    writer.Write(v1);
                    writer.Write('/');
                    writer.Write(v1);
                    writer.Write(' ');
                    writer.Write(v2);
                    writer.Write('/');
                    writer.Write(v2);
                    writer.Write('/');
                    writer.Write(v2);
                }
                else if (hasTexCoords)
                {
                    writer.Write(v0);
                    writer.Write('/');
                    writer.Write(v0);
                    writer.Write(' ');
                    writer.Write(v1);
                    writer.Write('/');
                    writer.Write(v1);
                    writer.Write(' ');
                    writer.Write(v2);
                    writer.Write('/');
                    writer.Write(v2);
                }
                else if (hasNormals)
                {
                    writer.Write(v0);
                    writer.Write('/');
                    writer.Write('/');
                    writer.Write(v0);
                    writer.Write(' ');
                    writer.Write(v1);
                    writer.Write('/');
                    writer.Write('/');
                    writer.Write(v1);
                    writer.Write(' ');
                    writer.Write(v2);
                    writer.Write('/');
                    writer.Write('/');
                    writer.Write(v2);
                }
                else
                {
                    writer.Write(v0);
                    writer.Write(' ');
                    writer.Write(v1);
                    writer.Write(' ');
                    writer.Write(v2);
                }

                writer.WriteLine();
            }
        }

        private static int ShiftIndex(int value, int count)
        {
            if (value < 0)
            {
                return count + value;
            }
            else
            {
                return value - 1;
            }
        }

        private static int CountOccurrences(string text, char character)
        {
            int count = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == character)
                {
                    ++count;
                }
            }
            return count;
        }
        #endregion
    }
}