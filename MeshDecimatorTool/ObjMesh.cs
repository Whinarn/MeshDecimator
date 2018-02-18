#region License
/*
MIT License

Copyright(c) 2017 Mattias Edlund

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

        #region Fields
        private Vector3d[] vertices = null;
        private Vector3[] normals = null;
        private Vector2[] texCoords2D = null;
        private Vector3[] texCoords3D = null;
        private int[] indices = null;
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
        /// Gets or sets the triangle indices for this mesh.
        /// </summary>
        public int[] Indices
        {
            get { return indices; }
            set { indices = value; }
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
            this.indices = indices;
        }
        #endregion

        #region Private Methods
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

        #region Public Methods
        #region Read File
        /// <summary>
        /// Reads an OBJ mesh from a file.
        /// Please note that this method only supports extremely simple OBJ meshes.
        /// </summary>
        /// <param name="path">The file path.</param>
        public void ReadFile(string path)
        {
            var vertexList = new List<Vector3d>(VertexInitialCapacity);
            var normalList = new List<Vector3>(VertexInitialCapacity);
            var texCoordList = new List<Vector3>(VertexInitialCapacity);
            var faceList = new List<Vector3i>(IndexInitialCapacity);
            var triangleIndexList = new List<int>(IndexInitialCapacity);
            var tempFaceList = new List<int>(6);
            bool texCoordsAre3D = false;

            int newVertexIndex = 0;
            using (StreamReader reader = File.OpenText(path))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Length == 0)
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
                        vertexList.Add(new Vector3d(f0, f1, f2));
                    }
                    else if (string.Equals(firstPart, "vn"))
                    {
                        if (lineSplit.Length != 4)
                            throw new InvalidDataException("Normals must be 3 components.");

                        float f0, f1, f2;
                        float.TryParse(lineSplit[1], NumberStyles.Float, CultureInfo.InvariantCulture, out f0);
                        float.TryParse(lineSplit[2], NumberStyles.Float, CultureInfo.InvariantCulture, out f1);
                        float.TryParse(lineSplit[3], NumberStyles.Float, CultureInfo.InvariantCulture, out f2);
                        normalList.Add(new Vector3(f0, f1, f2));
                    }
                    else if (string.Equals(firstPart, "vt"))
                    {
                        if (lineSplit.Length < 3)
                            throw new InvalidDataException("Texture coordinates needs at least 2 components.");

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
                        texCoordList.Add(new Vector3(f0, f1, f2));
                    }
                    else if (string.Equals(firstPart, "f"))
                    {
                        if (lineSplit.Length < 4)
                            throw new InvalidDataException("Faces must have at least three indices.");

                        tempFaceList.Clear();
                        for (int i = 1; i < lineSplit.Length; i++)
                        {
                            string word = lineSplit[i];
                            int slashCount = CountOccurrences(word, '/');
                            if (slashCount == 0)
                            {
                                int index;
                                int.TryParse(word, out index);
                                index = ShiftIndex(index, vertexList.Count);
                                faceList.Add(new Vector3i(index, -1, -1));
                            }
                            else if (slashCount == 1)
                            {
                                int index, texIndex;
                                int splitIndex = word.IndexOf('/');
                                string word1 = word.Substring(0, splitIndex);
                                string word2 = word.Substring(splitIndex);
                                int.TryParse(word1, out index);
                                int.TryParse(word2, out texIndex);
                                index = ShiftIndex(index, vertexList.Count);
                                texIndex = ShiftIndex(texIndex, texCoordList.Count);
                                faceList.Add(new Vector3i(index, texIndex, -1));
                            }
                            else if (slashCount == 2)
                            {
                                int index, texIndex, normalIndex;
                                int splitIndex1 = word.IndexOf('/');
                                int splitIndex2 = word.IndexOf('/', splitIndex1 + 1);
                                string word1 = word.Substring(0, splitIndex1);
                                string word2 = word.Substring(splitIndex1, splitIndex2 - splitIndex1);
                                string word3 = word.Substring(splitIndex2);
                                int.TryParse(word1, out index);
                                bool hasTexCoord = int.TryParse(word2, out texIndex);
                                int.TryParse(word3, out normalIndex);
                                index = ShiftIndex(index, vertexList.Count);
                                if (hasTexCoord)
                                {
                                    texIndex = ShiftIndex(texIndex, texCoordList.Count);
                                }
                                else
                                {
                                    texIndex = -1;
                                }
                                normalIndex = ShiftIndex(normalIndex, normalList.Count);
                                faceList.Add(new Vector3i(index, texIndex, normalIndex));
                            }
                            else
                            {
                                throw new InvalidDataException(string.Format("Invalid face data are supported (expected a maximum of two slashes, but found {0}.", slashCount));
                            }

                            tempFaceList.Add(newVertexIndex++);
                        }

                        // Convert into triangles
                        int faceIndexCount = tempFaceList.Count;
                        for (int i = 1; i < (faceIndexCount - 1); i++)
                        {
                            triangleIndexList.Add(tempFaceList[0]);
                            triangleIndexList.Add(tempFaceList[i]);
                            triangleIndexList.Add(tempFaceList[i + 1]);
                        }
                    }
                }
            }

            int faceCount = faceList.Count;
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

            indices = triangleIndexList.ToArray();
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
            else if (indices == null)
                throw new InvalidOperationException("There are no indices to write for this mesh.");

            using (StreamWriter writer = File.CreateText(path))
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

                if (normals != null)
                {
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

                bool hasTexCoords = (texCoords2D != null || texCoords3D != null);
                bool hasNormals = (normals != null);
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
        }
        #endregion
        #endregion
    }
}