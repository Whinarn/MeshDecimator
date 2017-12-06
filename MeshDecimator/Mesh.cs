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
using MeshDecimator.Math;

namespace MeshDecimator
{
    /// <summary>
    /// A mesh.
    /// </summary>
    public sealed class Mesh
    {
        #region Fields
        private Vector3d[] vertices = null;
        private int[][] indices = null;
        private Vector3[] normals = null;
        private Vector4[] tangents = null;
        private Vector2[] uv1 = null;
        private Vector2[] uv2 = null;
        private Vector2[] uv3 = null;
        private Vector2[] uv4 = null;
        private Vector4[] colors = null;
        private BoneWeight[] boneWeights = null;

        private static readonly int[] emptyIndices = new int[0];
        #endregion

        #region Properties
        /// <summary>
        /// Gets the count of vertices of this mesh.
        /// </summary>
        public int VertexCount
        {
            get { return vertices.Length; }
        }

        /// <summary>
        /// Gets or sets the count of submeshes in this mesh.
        /// </summary>
        public int SubMeshCount
        {
            get { return indices.Length; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("value");

                int[][] newIndices = new int[value][];
                Array.Copy(indices, 0, newIndices, 0, MathHelper.Min(indices.Length, newIndices.Length));
                indices = newIndices;
            }
        }

        /// <summary>
        /// Gets the total count of triangles in this mesh.
        /// </summary>
        public int TriangleCount
        {
            get
            {
                int triangleCount = 0;
                for (int i = 0; i < indices.Length; i++)
                {
                    if (indices[i] != null)
                    {
                        triangleCount += indices[i].Length / 3;
                    }
                }
                return triangleCount;
            }
        }

        /// <summary>
        /// Gets or sets the vertices for this mesh. Note that this resets all other vertex attributes.
        /// </summary>
        public Vector3d[] Vertices
        {
            get { return vertices; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                vertices = value;
                ClearVertexAttributes();
            }
        }

        /// <summary>
        /// Gets or sets the indices for this mesh. Once set, the sub-mesh count gets set to 1.
        /// </summary>
        public int[] Indices
        {
            get
            {
                if (indices.Length == 1)
                {
                    return indices[0] ?? emptyIndices;
                }
                else
                {
                    List<int> indexList = new List<int>(TriangleCount * 3);
                    for (int i = 0; i < indices.Length; i++)
                    {
                        if (indices[i] != null)
                        {
                            indexList.AddRange(indices[i]);
                        }
                    }
                    return indexList.ToArray();
                }
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                else if ((value.Length % 3) != 0)
                    throw new ArgumentException("The index count must be multiple by 3.", "value");

                SubMeshCount = 1;
                SetIndices(0, value);
            }
        }

        /// <summary>
        /// Gets or sets the normals for this mesh.
        /// </summary>
        public Vector3[] Normals
        {
            get { return normals; }
            set
            {
                if (value != null && value.Length != vertices.Length)
                    throw new ArgumentException(string.Format("The vertex normals must be as many as the vertices. Assigned: {0}  Require: {1}", value.Length, vertices.Length));

                normals = value;
            }
        }

        /// <summary>
        /// Gets or sets the tangents for this mesh.
        /// </summary>
        public Vector4[] Tangents
        {
            get { return tangents; }
            set
            {
                if (value != null && value.Length != vertices.Length)
                    throw new ArgumentException(string.Format("The vertex tangents must be as many as the vertices. Assigned: {0}  Require: {1}", value.Length, vertices.Length));

                tangents = value;
            }
        }

        /// <summary>
        /// Gets or sets the first UV set for this mesh.
        /// </summary>
        public Vector2[] UV1
        {
            get { return uv1; }
            set
            {
                if (value != null && value.Length != vertices.Length)
                    throw new ArgumentException(string.Format("The vertex UVs must be as many as the vertices. Assigned: {0}  Require: {1}", value.Length, vertices.Length));

                uv1 = value;
            }
        }

        /// <summary>
        /// Gets or sets the second UV set for this mesh.
        /// </summary>
        public Vector2[] UV2
        {
            get { return uv2; }
            set
            {
                if (value != null && value.Length != vertices.Length)
                    throw new ArgumentException(string.Format("The vertex UVs must be as many as the vertices. Assigned: {0}  Require: {1}", value.Length, vertices.Length));

                uv2 = value;
            }
        }

        /// <summary>
        /// Gets or sets the third UV set for this mesh.
        /// </summary>
        public Vector2[] UV3
        {
            get { return uv3; }
            set
            {
                if (value != null && value.Length != vertices.Length)
                    throw new ArgumentException(string.Format("The vertex UVs must be as many as the vertices. Assigned: {0}  Require: {1}", value.Length, vertices.Length));

                uv3 = value;
            }
        }

        /// <summary>
        /// Gets or sets the fourth UV set for this mesh.
        /// </summary>
        public Vector2[] UV4
        {
            get { return uv4; }
            set
            {
                if (value != null && value.Length != vertices.Length)
                    throw new ArgumentException(string.Format("The vertex UVs must be as many as the vertices. Assigned: {0}  Require: {1}", value.Length, vertices.Length));

                uv4 = value;
            }
        }

        /// <summary>
        /// Gets or sets the vertex colors for this mesh.
        /// </summary>
        public Vector4[] Colors
        {
            get { return colors; }
            set
            {
                if (value != null && value.Length != vertices.Length)
                    throw new ArgumentException(string.Format("The vertex colors must be as many as the vertices. Assigned: {0}  Require: {1}", value.Length, vertices.Length));

                colors = value;
            }
        }

        /// <summary>
        /// Gets or sets the vertex bone weights for this mesh.
        /// </summary>
        public BoneWeight[] BoneWeights
        {
            get { return boneWeights; }
            set
            {
                if (value != null && value.Length != vertices.Length)
                    throw new ArgumentException(string.Format("The vertex bone weights must be as many as the vertices. Assigned: {0}  Require: {1}", value.Length, vertices.Length));

                boneWeights = value;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new mesh.
        /// </summary>
        /// <param name="vertices">The mesh vertices.</param>
        /// <param name="indices">The mesh indices.</param>
        public Mesh(Vector3d[] vertices, int[] indices)
        {
            if (vertices == null)
                throw new ArgumentNullException("vertices");
            else if (indices == null)
                throw new ArgumentNullException("indices");
            else if ((indices.Length % 3) != 0)
                throw new ArgumentException("The index count must be multiple by 3.", "indices");

            this.vertices = vertices;
            this.indices = new int[1][];
            this.indices[0] = indices;
        }

        /// <summary>
        /// Creates a new mesh.
        /// </summary>
        /// <param name="vertices">The mesh vertices.</param>
        /// <param name="indices">The mesh indices.</param>
        public Mesh(Vector3d[] vertices, int[][] indices)
        {
            if (vertices == null)
                throw new ArgumentNullException("vertices");
            else if (indices == null)
                throw new ArgumentNullException("indices");

            for (int i = 0; i < indices.Length; i++)
            {
                if (indices[i] != null && (indices[i].Length % 3) != 0)
                    throw new ArgumentException(string.Format("The index count must be multiple by 3 at sub-mesh index {0}.", i), "indices");
            }

            this.vertices = vertices;
            this.indices = indices;
        }
        #endregion

        #region Private Methods
        private void ClearVertexAttributes()
        {
            normals = null;
            tangents = null;
            uv1 = null;
            uv2 = null;
            uv3 = null;
            uv4 = null;
            colors = null;
            boneWeights = null;
        }
        #endregion

        #region Public Methods
        #region Recalculate Normals
        /// <summary>
        /// Recalculates the normals for this mesh smoothly.
        /// </summary>
        public void RecalculateNormals()
        {
            int vertexCount = vertices.Length;
            Vector3[] normals = new Vector3[vertexCount];

            int subMeshCount = this.indices.Length;
            for (int subMeshIndex = 0; subMeshIndex < subMeshCount; subMeshIndex++)
            {
                int[] indices = this.indices[subMeshIndex];
                if (indices == null)
                    continue;

                int indexCount = indices.Length;
                for (int i = 0; i < indexCount; i += 3)
                {
                    int i0 = indices[i];
                    int i1 = indices[i + 1];
                    int i2 = indices[i + 2];

                    var v0 = (Vector3)vertices[i0];
                    var v1 = (Vector3)vertices[i1];
                    var v2 = (Vector3)vertices[i2];

                    var nx = v1 - v0;
                    var ny = v2 - v0;
                    Vector3 normal;
                    Vector3.Cross(ref nx, ref ny, out normal);
                    normal.Normalize();

                    normals[i0] += normal;
                    normals[i1] += normal;
                    normals[i2] += normal;
                }
            }

            for (int i = 0; i < vertexCount; i++)
            {
                normals[i].Normalize();
            }

            this.normals = normals;
        }
        #endregion

        #region Recalculate Tangents
        /// <summary>
        /// Recalculates the tangents for this mesh.
        /// </summary>
        public void RecalculateTangents()
        {
            // Make sure we have the normals and the first UV set first
            if (normals == null || uv1 == null)
                return;

            int vertexCount = vertices.Length;
            
            Vector4[] tangents = new Vector4[vertexCount];
            Vector3[] tan1 = new Vector3[vertexCount];
            Vector3[] tan2 = new Vector3[vertexCount];

            int subMeshCount = this.indices.Length;
            for (int subMeshIndex = 0; subMeshIndex < subMeshCount; subMeshIndex++)
            {
                int[] indices = this.indices[subMeshIndex];
                if (indices == null)
                    continue;

                int indexCount = indices.Length;
                for (int i = 0; i < indexCount; i += 3)
                {
                    int i0 = indices[i];
                    int i1 = indices[i + 1];
                    int i2 = indices[i + 2];

                    var v0 = vertices[i0];
                    var v1 = vertices[i1];
                    var v2 = vertices[i2];

                    var w0 = uv1[i0];
                    var w1 = uv1[i1];
                    var w2 = uv1[i2];

                    float x1 = (float)(v1.x - v0.x);
                    float x2 = (float)(v2.x - v0.x);
                    float y1 = (float)(v1.y - v0.y);
                    float y2 = (float)(v2.y - v0.y);
                    float z1 = (float)(v1.z - v0.z);
                    float z2 = (float)(v2.z - v0.z);
                    float s1 = w1.x - w0.x;
                    float s2 = w2.x - w0.x;
                    float t1 = w1.y - w0.y;
                    float t2 = w2.y - w0.y;
                    float r = 1f / (s1 * t2 - s2 * t1);

                    var sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
                    var tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

                    tan1[i0] += sdir;
                    tan1[i1] += sdir;
                    tan1[i2] += sdir;
                    tan2[i0] += tdir;
                    tan2[i1] += tdir;
                    tan2[i2] += tdir;
                }
            }

            for (int i = 0; i < vertexCount; i++)
            {
                var n = normals[i];
                var t = tan1[i];

                var tmp = (t - n * Vector3.Dot(ref n, ref t));
                tmp.Normalize();

                Vector3 c;
                Vector3.Cross(ref n, ref t, out c);
                float dot = Vector3.Dot(ref c, ref tan2[i]);
                float w = (dot < 0f ? -1f : 1f);
                tangents[i] = new Vector4(tmp.x, tmp.y, tmp.z, w);
            }

            this.tangents = tangents;
        }
        #endregion

        #region Triangles
        /// <summary>
        /// Returns the count of triangles for a specific sub-mesh in this mesh.
        /// </summary>
        /// <param name="subMeshIndex">The sub-mesh index.</param>
        /// <returns>The triangle count.</returns>
        public int GetTriangleCount(int subMeshIndex)
        {
            if (subMeshIndex < 0 || subMeshIndex >= indices.Length)
                throw new IndexOutOfRangeException();

            return indices[subMeshIndex].Length / 3;
        }

        /// <summary>
        /// Returns the triangle indices of a specific sub-mesh in this mesh.
        /// </summary>
        /// <param name="subMeshIndex">The sub-mesh index.</param>
        /// <returns>The triangle indices.</returns>
        public int[] GetIndices(int subMeshIndex)
        {
            if (subMeshIndex < 0 || subMeshIndex >= indices.Length)
                throw new IndexOutOfRangeException();

            return indices[subMeshIndex] ?? emptyIndices;
        }

        /// <summary>
        /// Sets the triangle indices of a specific sub-mesh in this mesh.
        /// </summary>
        /// <param name="subMeshIndex">The sub-mesh index.</param>
        /// <param name="indices">The triangle indices.</param>
        public void SetIndices(int subMeshIndex, int[] indices)
        {
            if (subMeshIndex < 0 || subMeshIndex >= this.indices.Length)
                throw new IndexOutOfRangeException();
            else if (indices == null)
                throw new ArgumentNullException("indices");
            else if ((indices.Length % 3) != 0)
                throw new ArgumentException("The index count must be multiple by 3.", "indices");

            this.indices[subMeshIndex] = indices;
        }
        #endregion

        #region To String
        /// <summary>
        /// Returns the text-representation of this mesh.
        /// </summary>
        /// <returns>The text-representation.</returns>
        public override string ToString()
        {
            return string.Format("Vertices: {0}", vertices.Length);
        }
        #endregion
        #endregion
    }
}