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
        #region Fields
        private Vector3d[] vertices = null;
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
        /// Gets or sets the indices for this mesh.
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

        #region Public Methods
        #region Read File
        /// <summary>
        /// Reads an OBJ mesh from a file.
        /// Please note that this method only supports extremely simple OBJ meshes.
        /// </summary>
        /// <param name="path">The file path.</param>
        public void ReadFile(string path)
        {
            List<Vector3d> vertexList = new List<Vector3d>(20000);
            List<int> indexList = new List<int>(40000);
            using (StreamReader reader = File.OpenText(path))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Length == 0)
                        continue;

                    string[] lineSplit = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    string firstPart = lineSplit[0];
                    if (firstPart == "v")
                    {
                        double f0, f1, f2;
                        double.TryParse(lineSplit[1], NumberStyles.Float, CultureInfo.InvariantCulture, out f0);
                        double.TryParse(lineSplit[2], NumberStyles.Float, CultureInfo.InvariantCulture, out f1);
                        double.TryParse(lineSplit[3], NumberStyles.Float, CultureInfo.InvariantCulture, out f2);
                        vertexList.Add(new Vector3d(f0, f1, f2));
                    }
                    else if (firstPart == "f")
                    {
                        int split0 = lineSplit[1].IndexOf('/');
                        int split1 = lineSplit[2].IndexOf('/');
                        int split2 = lineSplit[3].IndexOf('/');
                        if (split0 != -1) lineSplit[1] = lineSplit[1].Substring(0, split0);
                        if (split1 != -1) lineSplit[2] = lineSplit[2].Substring(0, split1);
                        if (split2 != -1) lineSplit[3] = lineSplit[3].Substring(0, split2);

                        int i0, i1, i2;
                        int.TryParse(lineSplit[1], out i0);
                        int.TryParse(lineSplit[2], out i1);
                        int.TryParse(lineSplit[3], out i2);

                        if (i0 < 0)
                        {
                            i0 = vertexList.Count + i0;
                        }
                        else
                        {
                            --i0;
                        }
                        if (i1 < 0)
                        {
                            i1 = vertexList.Count + i1;
                        }
                        else
                        {
                            --i1;
                        }
                        if (i2 < 0)
                        {
                            i2 = vertexList.Count + i2;
                        }
                        else
                        {
                            --i2;
                        }

                        indexList.Add(i0);
                        indexList.Add(i1);
                        indexList.Add(i2);
                    }
                }
            }

            vertices = vertexList.ToArray();
            indices = indexList.ToArray();
        }
        #endregion

        #region Write File
        /// <summary>
        /// Writes this OBJ mesh to a file.
        /// </summary>
        /// <param name="path">The file path.</param>
        public void WriteFile(string path)
        {
            using (StreamWriter writer = File.CreateText(path))
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    var vertex = vertices[i];
                    writer.Write("v ");
                    writer.Write(vertex.x.ToString("g"));
                    writer.Write(' ');
                    writer.Write(vertex.y.ToString("g"));
                    writer.Write(' ');
                    writer.Write(vertex.z.ToString("g"));
                    writer.WriteLine();
                }

                for (int i = 0; i < indices.Length; i += 3)
                {
                    int v0 = indices[i] + 1;
                    int v1 = indices[i + 1] + 1;
                    int v2 = indices[i + 2] + 1;

                    writer.Write("f ");
                    writer.Write(v0);
                    writer.Write(' ');
                    writer.Write(v1);
                    writer.Write(' ');
                    writer.Write(v2);
                    writer.WriteLine();
                }
            }
        }
        #endregion
        #endregion
    }
}