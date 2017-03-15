#region Original License
/////////////////////////////////////////////
//
// Mesh Simplification Tutorial
//
// (C) by Sven Forstmann in 2014
//
// License : MIT
// http://opensource.org/licenses/MIT
//
//https://github.com/sp4cerat/Fast-Quadric-Mesh-Simplification
#endregion

using System;
using System.Collections.Generic;
using MeshDecimator.Collections;
using MeshDecimator.Math;

namespace MeshDecimator.Algorithms
{
    /// <summary>
    /// The fast quadratic mesh simplification algorithm.
    /// </summary>
    public sealed class FastQuadraticMeshSimplification : DecimationAlgorithm
    {
        #region Consts
        private const double DoubleEpsilon = 1.0E-3;
        #endregion

        #region Classes
        #region Triangle
        private struct Triangle
        {
            #region Fields
            public int v0;
            public int v1;
            public int v2;
            public int subMeshIndex;

            //public double area;

            public double err0;
            public double err1;
            public double err2;
            public double err3;

            public bool deleted;
            public bool dirty;
            public Vector3d n;
            #endregion

            #region Properties
            public int this[int index]
            {
                get
                {
                    return (index == 0 ? v0 : (index == 1 ? v1 : v2));
                }
                set
                {
                    switch (index)
                    {
                        case 0:
                            v0 = value;
                            break;
                        case 1:
                            v1 = value;
                            break;
                        case 2:
                            v2 = value;
                            break;
                        default:
                            throw new IndexOutOfRangeException();
                    }
                }
            }
            #endregion

            #region Constructor
            public Triangle(int v0, int v1, int v2, int subMeshIndex)
            {
                this.v0 = v0;
                this.v1 = v1;
                this.v2 = v2;
                this.subMeshIndex = subMeshIndex;

                //area = 0;
                err0 = err1 = err2 = err3 = 0;
                deleted = dirty = false;
                n = new Vector3d();
            }
            #endregion

            #region Public Methods
            public void GetErrors(double[] err)
            {
                err[0] = err0;
                err[1] = err1;
                err[2] = err2;
            }
            #endregion
        }
        #endregion

        #region Vertex
        private struct Vertex
        {
            public Vector3d p;
            public int tstart;
            public int tcount;
            public SymmetricMatrix q;
            public bool border;
            public bool linked;

            public Vertex(Vector3d p)
            {
                this.p = p;
                this.tstart = 0;
                this.tcount = 0;
                this.q = new SymmetricMatrix();
                this.border = true;
                this.linked = false;
            }
        }
        #endregion

        #region Ref
        private struct Ref
        {
            public int tid;
            public int tvertex;

            public void Set(int tid, int tvertex)
            {
                this.tid = tid;
                this.tvertex = tvertex;
            }
        }
        #endregion
        #endregion

        #region Fields
        private double agressiveness = 7.0;

        private int subMeshCount = 0;
        private ResizableArray<Triangle> triangles = null;
        private ResizableArray<Vertex> vertices = null;
        private ResizableArray<Ref> refs = null;

        private ResizableArray<Vector3> vertNormals = null;
        private ResizableArray<Vector4> vertTangents = null;
        private ResizableArray<Vector2> vertUV1 = null;
        private ResizableArray<Vector2> vertUV2 = null;
        private ResizableArray<Vector2> vertUV3 = null;
        private ResizableArray<Vector2> vertUV4 = null;
        private ResizableArray<Vector4> vertColors = null;
        private ResizableArray<BoneWeight> vertBoneWeights = null;

        private int remainingVertices = 0;

        // Pre-allocated buffer for error values
        private double[] errArr = new double[3];
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the agressiveness of this algorithm. Higher number equals higher quality, but more expensive to run.
        /// </summary>
        public double Agressiveness
        {
            get { return agressiveness; }
            set { agressiveness = value; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new fast quadratic mesh simplification algorithm.
        /// </summary>
        public FastQuadraticMeshSimplification()
        {
            triangles = new ResizableArray<Triangle>(0);
            vertices = new ResizableArray<Vertex>(0);
            refs = new ResizableArray<Ref>(0);
        }
        #endregion

        #region Private Methods
        #region Initialize Vertex Attribute
        private ResizableArray<T> InitializeVertexAttribute<T>(T[] attributeValues)
        {
            if (attributeValues != null && attributeValues.Length == vertices.Length)
            {
                var newArray = new ResizableArray<T>(0);
                newArray.Resize(attributeValues.Length);
                var newArrayData = newArray.Data;
                Array.Copy(attributeValues, 0, newArrayData, 0, attributeValues.Length);
                return newArray;
            }
            return null;
        }
        #endregion

        #region Calculate Error
        private double VertexError(ref SymmetricMatrix q, double x, double y, double z)
        {
            return  q.m0*x*x + 2*q.m1*x*y + 2*q.m2*x*z + 2*q.m3*x + q.m4*y*y
                + 2*q.m5*y*z + 2*q.m6*y +     q.m7*z*z + 2*q.m8*z + q.m9;
        }

        private double CalculateError(int i0, int i1, out Vector3d result)
        {
            // compute interpolated vertex
            var vertices = this.vertices.Data;
            Vertex v0 = vertices[i0];
            Vertex v1 = vertices[i1];
            SymmetricMatrix q = v0.q + v1.q;
            bool border = (v0.border & v1.border);
            double error = 0.0;
            double det = q.Determinant1();
            if (det != 0.0 && !border)
            {
                // q_delta is invertible
                result = new Vector3d(
                    -1.0 / det * q.Determinant2(),  // vx = A41/det(q_delta)
                    1.0 / det * q.Determinant3(),   // vy = A42/det(q_delta)
                    -1.0 / det * q.Determinant4()); // vz = A43/det(q_delta)
                error = VertexError(ref q, result.x, result.y, result.z);
            }
            else
            {
                // det = 0 -> try to find best result
                Vector3d p1 = v0.p;
                Vector3d p2 = v1.p;
                Vector3d p3 = (p1 + p2) * 0.5f;
                double error1 = VertexError(ref q, p1.x, p1.y, p1.z);
                double error2 = VertexError(ref q, p2.x, p2.y, p2.z);
                double error3 = VertexError(ref q, p3.x, p3.y, p3.z);
                error = MathHelper.Min(error1, error2, error3);
                if (error == error3)
                    result = p3;
                else if (error == error2)
                    result = p2;
                else if (error == error1)
                    result = p1;
                else
                    result = p3;
            }
            return error;
        }
        #endregion

        #region Flipped
        /// <summary>
        /// Check if a triangle flips when this edge is removed
        /// </summary>
        private bool Flipped(Vector3d p, int i0, int i1, ref Vertex v0, ResizableArray<bool> deleted)
        {
            int tcount = v0.tcount;
            var refs = this.refs.Data;
            var triangles = this.triangles.Data;
            var vertices = this.vertices.Data;
            for (int k = 0; k < tcount; k++)
            {
                Ref r = refs[v0.tstart + k];
                Triangle t = triangles[r.tid];
                if (t.deleted)
                    continue;

                int s = r.tvertex;
                int id1 = t[(s + 1) % 3];
                int id2 = t[(s + 2) % 3];
                if (id1 == i1 || id2 == i1)
                {
                    deleted[k] = true;
                    continue;
                }

                Vector3d d1 = vertices[id1].p - p;
                d1.Normalize();
                Vector3d d2 = vertices[id2].p - p;
                d2.Normalize();
                double dot = Vector3d.Dot(ref d1, ref d2);
                if (System.Math.Abs(dot) > 0.999)
                    return true;

                Vector3d n;
                Vector3d.Cross(ref d1, ref d2, out n);
                n.Normalize();
                deleted[k] = false;
                dot = Vector3d.Dot(ref n, ref t.n);
                if (dot < 0.2)
                    return true;
            }

            return false;
        }
        #endregion

        #region Calculate Area
        private double CalculateArea(int i0, int i1, int i2)
        {
            var vertices = this.vertices.Data;
            return MathHelper.TriangleArea(ref vertices[i0].p, ref vertices[i1].p, ref vertices[i2].p);
        }
        #endregion

        #region Update Triangles
        /// <summary>
        /// Update triangle connections and edge error after a edge is collapsed.
        /// </summary>
        private void UpdateTriangles(int i0, ref Vertex v, ResizableArray<bool> deleted, ref int deletedTriangles)
        {
            Vector3d p;
            int tcount = v.tcount;
            var triangles = this.triangles.Data;
            for (int k = 0; k < tcount; k++)
            {
                Ref r = refs[v.tstart + k];
                int tid = r.tid;
                Triangle t = triangles[tid];
                if (t.deleted)
                    continue;

                if (deleted[k])
                {
                    triangles[tid].deleted = true;
                    ++deletedTriangles;
                    continue;
                }

                t[r.tvertex] = i0;
                t.dirty = true;
                //t.area = CalculateArea(t.v0, t.v1, t.v2);
                t.err0 = CalculateError(t.v0, t.v1, out p);
                t.err1 = CalculateError(t.v1, t.v2, out p);
                t.err2 = CalculateError(t.v2, t.v0, out p);
                t.err3 = MathHelper.Min(t.err0, t.err1, t.err2);
                triangles[tid] = t;
                refs.Add(r);
            }
        }
        #endregion

        #region Merge Vertices
        private void MergeVertices(int i0, int i1)
        {
            if (vertNormals != null)
            {
                vertNormals[i0] = (vertNormals[i0] + vertNormals[i1]) * 0.5f;
            }
            if (vertTangents != null)
            {
                vertTangents[i0] = (vertTangents[i0] + vertTangents[i1]) * 0.5f;
            }
            if (vertUV1 != null)
            {
                vertUV1[i0] = (vertUV1[i0] + vertUV1[i1]) * 0.5f;
            }
            if (vertUV2 != null)
            {
                vertUV2[i0] = (vertUV2[i0] + vertUV2[i1]) * 0.5f;
            }
            if (vertUV3 != null)
            {
                vertUV3[i0] = (vertUV3[i0] + vertUV3[i1]) * 0.5f;
            }
            if (vertUV4 != null)
            {
                vertUV4[i0] = (vertUV4[i0] + vertUV4[i1]) * 0.5f;
            }
            if (vertColors != null)
            {
                vertColors[i0] = (vertColors[i0] + vertColors[i1]) * 0.5f;
            }
            if (vertBoneWeights != null)
            {
                var vertBoneWeights = this.vertBoneWeights.Data;
                BoneWeight.Merge(ref vertBoneWeights[i0], ref vertBoneWeights[i1]);
            }
        }
        #endregion

        #region Remove Vertex Pass
        /// <summary>
        /// Remove vertices and mark deleted triangles
        /// </summary>
        private void RemoveVertexPass(int startTrisCount, int targetTrisCount, double threshold, ResizableArray<bool> deleted0, ResizableArray<bool> deleted1, ref int deletedTris)
        {
            var triangles = this.triangles.Data;
            int triangleCount = this.triangles.Length;
            var vertices = this.vertices.Data;

            int maxVertexCount = this.maxVertexCount;
            if (maxVertexCount <= 0)
                maxVertexCount = int.MaxValue;

            Vertex v0, v1;
            Vector3d p;
            for (int i = 0; i < triangleCount; i++)
            {
                var t = triangles[i];
                if (t.dirty || t.deleted || t.err3 > threshold)
                    continue;

                t.GetErrors(errArr);
                for (int j = 0; j < 3; j++)
                {
                    if (errArr[j] > threshold)
                        continue;

                    int i0 = t[j];
                    int i1 = t[(j + 1) % 3];
                    v0 = vertices[i0];
                    v1 = vertices[i1];

                    // Border check
                    if (v0.border != v1.border)
                        continue;

                    // Keep linked vertices
                    if (keepLinkedVertices && (v0.linked || v1.linked))
                        continue;

                    // If borders should be kept
                    if (keepBorders && (v0.border || v1.border))
                        continue;

                    // Compute vertex to collapse to
                    CalculateError(i0, i1, out p);
                    deleted0.Resize(v0.tcount); // normals temporarily
                    deleted1.Resize(v1.tcount); // normals temporarily

                    // Don't remove if flipped
                    if (Flipped(p, i0, i1, ref v0, deleted0))
                        continue;
                    if (Flipped(p, i1, i0, ref v1, deleted1))
                        continue;

                    // Not flipped, so remove edge
                    v0.p = p;
                    v0.q += v1.q;
                    vertices[i0] = v0;
                    MergeVertices(i0, i1);

                    int tstart = refs.Length;
                    UpdateTriangles(i0, ref v0, deleted0, ref deletedTris);
                    UpdateTriangles(i0, ref v1, deleted1, ref deletedTris);

                    int tcount = refs.Length - tstart;
                    if (tcount <= v0.tcount)
                    {
                        // save ram
                        if (tcount > 0)
                        {
                            var refsArr = refs.Data;
                            Array.Copy(refsArr, tstart, refsArr, v0.tstart, tcount);
                        }
                    }
                    else
                    {
                        // append
                        vertices[i0].tstart = tstart;
                    }

                    vertices[i0].tcount = tcount;
                    --remainingVertices;
                    break;
                }

                // Check if we are already done
                if ((startTrisCount - deletedTris) <= targetTrisCount && remainingVertices < maxVertexCount)
                    break;
            }
        }
        #endregion

        #region Update Mesh
        /// <summary>
        /// Compact triangles, compute edge error and build reference list.
        /// </summary>
        /// <param name="iteration">The iteration index.</param>
        private void UpdateMesh(int iteration)
        {
            var triangles = this.triangles.Data;
            var vertices = this.vertices.Data;

            int triangleCount = this.triangles.Length;
            int vertexCount = this.vertices.Length;
            if (iteration > 0) // compact triangles
            {
                int dst = 0;
                for (int i = 0; i < triangleCount; i++)
                {
                    var triangle = triangles[i];
                    if (!triangle.deleted)
                    {
                        if (dst != i)
                        {
                            triangles[dst] = triangle;
                        }
                        dst++;
                    }
                }
                this.triangles.Resize(dst);
                triangles = this.triangles.Data;
                triangleCount = dst;
            }

            // Init Quadrics by Plane & Edge Errors
            //
            // required at the beginning ( iteration == 0 )
            // recomputing during the simplification is not required,
            // but mostly improves the result for closed meshes
            if (iteration == 0)
            {
                for (int i = 0; i < vertexCount; i++)
                {
                    vertices[i].q = new SymmetricMatrix();
                }

                Vector3d n, p0, p1, p2, p10, p20, dummy;
                SymmetricMatrix sm;
                for (int i = 0; i < triangleCount; i++)
                {
                    var triangle = triangles[i];
                    var vert0 = vertices[triangle.v0];
                    var vert1 = vertices[triangle.v1];
                    var vert2 = vertices[triangle.v2];
                    p0 = vert0.p;
                    p1 = vert1.p;
                    p2 = vert2.p;
                    p10 = p1 - p0;
                    p20 = p2 - p0;
                    Vector3d.Cross(ref p10, ref p20, out n);
                    n.Normalize();
                    triangles[i].n = n;

                    sm = new SymmetricMatrix(n.x, n.y, n.z, -Vector3d.Dot(ref n, ref p0));
                    vert0.q += sm;
                    vert1.q += sm;
                    vert2.q += sm;
                    vertices[triangle.v0] = vert0;
                    vertices[triangle.v1] = vert1;
                    vertices[triangle.v2] = vert2;
                }

                for (int i = 0; i < triangleCount; i++)
                {
                    // Calc Edge Error
                    var triangle = triangles[i];
                    //triangle.area = CalculateArea(triangle.v0, triangle.v1, triangle.v2);
                    triangle.err0 = CalculateError(triangle.v0, triangle.v1, out dummy);
                    triangle.err1 = CalculateError(triangle.v1, triangle.v2, out dummy);
                    triangle.err2 = CalculateError(triangle.v2, triangle.v0, out dummy);
                    triangle.err3 = MathHelper.Min(triangle.err0, triangle.err1, triangle.err2);
                    triangles[i] = triangle;
                }
            }

            // Init Reference ID list
            for (int i = 0; i < vertexCount; i++)
            {
                var vertex = vertices[i];
                vertex.tstart = 0;
                vertex.tcount = 0;
                vertices[i] = vertex;
            }

            for (int i = 0; i < triangleCount; i++)
            {
                var triangle = triangles[i];
                ++vertices[triangle.v0].tcount;
                ++vertices[triangle.v1].tcount;
                ++vertices[triangle.v2].tcount;
            }

            int tstart = 0;
            remainingVertices = 0;
            for (int i = 0; i < vertexCount; i++)
            {
                var vertex = vertices[i];
                vertex.tstart = tstart;
                tstart += vertex.tcount;
                if (vertex.tcount > 0)
                {
                    ++remainingVertices;
                    vertex.tcount = 0;
                }
                vertices[i] = vertex;
            }

            // Write References
            this.refs.Resize(tstart);
            var refs = this.refs.Data;
            for (int i = 0; i < triangleCount; i++)
            {
                var triangle = triangles[i];
                var vert0 = vertices[triangle.v0];
                var vert1 = vertices[triangle.v1];
                var vert2 = vertices[triangle.v2];

                refs[vert0.tstart + vert0.tcount].Set(i, 0);
                refs[vert1.tstart + vert1.tcount].Set(i, 1);
                refs[vert2.tstart + vert2.tcount].Set(i, 2);
                ++vert0.tcount;
                ++vert1.tcount;
                ++vert2.tcount;

                vertices[triangle.v0] = vert0;
                vertices[triangle.v1] = vert1;
                vertices[triangle.v2] = vert2;
            }

            // Identify boundary : vertices[].border=0,1
            if (iteration == 0)
            {
                List<int> vcount = new List<int>();
                List<int> vids = new List<int>();
                for (int i = 0; i < vertexCount; i++)
                {
                    vertices[i].border = false;
                }

                int ofs;
                int id;
                for (int i = 0; i < vertexCount; i++)
                {
                    var vertex = vertices[i];
                    vcount.Clear();
                    vids.Clear();

                    int tcount = vertex.tcount;
                    for (int j = 0; j < tcount; j++)
                    {
                        int k = refs[vertex.tstart + j].tid;
                        Triangle t = triangles[k];
                        for (k = 0; k < 3; k++)
                        {
                            ofs = 0;
                            id = t[k];
                            while (ofs < vcount.Count)
                            {
                                if (vids[ofs] == id)
                                    break;

                                ++ofs;
                            }

                            if (ofs == vcount.Count)
                            {
                                vcount.Add(1);
                                vids.Add(id);
                            }
                            else
                            {
                                ++vcount[ofs];
                            }
                        }
                    }

                    int vcountCount = vcount.Count;
                    for (int j = 0; j < vcountCount; j++)
                    {
                        if (vcount[j] == 1)
                        {
                            id = vids[j];
                            vertices[id].border = true;
                        }
                    }
                }
            }
        }
        #endregion

        #region Compact Mesh
        /// <summary>
        /// Finally compact mesh before exiting.
        /// </summary>
        private void CompactMesh()
        {
            int dst = 0;
            var vertices = this.vertices.Data;
            int vertexCount = this.vertices.Length;
            for (int i = 0; i < vertexCount; i++)
            {
                vertices[i].tcount = 0;
            }

            var triangles = this.triangles.Data;
            int triangleCount = this.triangles.Length;
            for (int i = 0; i < triangleCount; i++)
            {
                var triangle = triangles[i];
                if (!triangle.deleted)
                {
                    triangles[dst++] = triangle;
                    vertices[triangle.v0].tcount = 1;
                    vertices[triangle.v1].tcount = 1;
                    vertices[triangle.v2].tcount = 1;
                }
            }
            this.triangles.Resize(dst);
            triangles = this.triangles.Data;
            triangleCount = dst;

            var vertNormals = (this.vertNormals != null ? this.vertNormals.Data : null);
            var vertTangents = (this.vertTangents != null ? this.vertTangents.Data : null);
            var vertUV1 = (this.vertUV1 != null ? this.vertUV1.Data : null);
            var vertUV2 = (this.vertUV2 != null ? this.vertUV2.Data : null);
            var vertUV3 = (this.vertUV3 != null ? this.vertUV3.Data : null);
            var vertUV4 = (this.vertUV4 != null ? this.vertUV4.Data : null);
            var vertColors = (this.vertColors != null ? this.vertColors.Data : null);
            var vertBoneWeights = (this.vertBoneWeights != null ? this.vertBoneWeights.Data : null);
            dst = 0;
            for (int i = 0; i < vertexCount; i++)
            {
                var vert = vertices[i];
                if (vert.tcount > 0)
                {
                    vert.tstart = dst;
                    vertices[i] = vert;

                    if (dst != i)
                    {
                        vertices[dst].p = vert.p;
                        if (vertNormals != null) vertNormals[dst] = vertNormals[i];
                        if (vertTangents != null) vertTangents[dst] = vertTangents[i];
                        if (vertUV1 != null) vertUV1[dst] = vertUV1[i];
                        if (vertUV2 != null) vertUV2[dst] = vertUV2[i];
                        if (vertUV3 != null) vertUV3[dst] = vertUV3[i];
                        if (vertUV4 != null) vertUV4[dst] = vertUV4[i];
                        if (vertColors != null) vertColors[dst] = vertColors[i];
                        if (vertBoneWeights != null) vertBoneWeights[dst] = vertBoneWeights[i];
                    }
                    ++dst;
                }
            }
            for (int i = 0; i < triangleCount; i++)
            {
                var triangle = triangles[i];
                triangle.v0 = vertices[triangle.v0].tstart;
                triangle.v1 = vertices[triangle.v1].tstart;
                triangle.v2 = vertices[triangle.v2].tstart;
                triangles[i] = triangle;
            }

            this.vertices.Resize(dst);
            if (vertNormals != null) this.vertNormals.Resize(dst);
            if (vertTangents != null) this.vertTangents.Resize(dst);
            if (vertUV1 != null) this.vertUV1.Resize(dst);
            if (vertUV2 != null) this.vertUV2.Resize(dst);
            if (vertUV3 != null) this.vertUV3.Resize(dst);
            if (vertUV4 != null) this.vertUV4.Resize(dst);
            if (vertColors != null) this.vertColors.Resize(dst);
            if (vertBoneWeights != null) this.vertBoneWeights.Resize(dst);
        }
        #endregion
        #endregion

        #region Public Methods
        #region Initialize
        /// <summary>
        /// Initializes the algorithm with the original mesh.
        /// </summary>
        /// <param name="mesh">The mesh.</param>
        public override void Initialize(Mesh mesh)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            int meshSubMeshCount = mesh.SubMeshCount;
            int meshTriangleCount = mesh.TriangleCount;
            var meshVertices = mesh.Vertices;
            var meshNormals = mesh.Normals;
            var meshTangents = mesh.Tangents;
            var meshUV1 = mesh.UV1;
            var meshUV2 = mesh.UV2;
            var meshUV3 = mesh.UV3;
            var meshUV4 = mesh.UV4;
            var meshColors = mesh.Colors;
            var meshBoneWeights = mesh.BoneWeights;
            subMeshCount = meshSubMeshCount;

            vertices.Resize(meshVertices.Length);
            var vertArr = vertices.Data;
            for (int i = 0; i < meshVertices.Length; i++)
            {
                vertArr[i] = new Vertex(meshVertices[i]);
            }

            if (keepLinkedVertices)
            {
                // Find links between vertices
                // TODO: Is it possible to optimize this further?
                int vertexCount = vertArr.Length;
                for (int i = 0; i < vertexCount; i++)
                {
                    bool hasLinked = false;
                    var p0 = meshVertices[i];
                    for (int j = i + 1; j < vertexCount; j++)
                    {
                        if (hasLinked && vertArr[j].linked)
                            continue;

                        double xDiff = meshVertices[j].x - p0.x;
                        if ((xDiff * xDiff) > Vector3d.Epsilon)
                            continue;

                        double yDiff = meshVertices[j].y - p0.y;
                        if ((yDiff * yDiff) > Vector3d.Epsilon)
                            continue;

                        double zDiff = meshVertices[j].z - p0.z;
                        if ((zDiff * zDiff) > Vector3d.Epsilon)
                            continue;

                        hasLinked = true;
                        vertArr[i].linked = true;
                        vertArr[j].linked = true;
                    }
                }
            }

            triangles.Resize(meshTriangleCount);
            var trisArr = triangles.Data;
            int triangleIndex = 0;
            for (int subMeshIndex = 0; subMeshIndex < meshSubMeshCount; subMeshIndex++)
            {
                int[] subMeshIndices = mesh.GetIndices(subMeshIndex);
                int subMeshTriangleCount = subMeshIndices.Length / 3;
                for (int i = 0; i < subMeshTriangleCount; i++)
                {
                    int offset = i * 3;
                    int v0 = subMeshIndices[offset];
                    int v1 = subMeshIndices[offset + 1];
                    int v2 = subMeshIndices[offset + 2];
                    trisArr[triangleIndex++] = new Triangle(v0, v1, v2, subMeshIndex);
                }
            }

            vertNormals = InitializeVertexAttribute(meshNormals);
            vertTangents = InitializeVertexAttribute(meshTangents);
            vertUV1 = InitializeVertexAttribute(meshUV1);
            vertUV2 = InitializeVertexAttribute(meshUV2);
            vertUV3 = InitializeVertexAttribute(meshUV3);
            vertUV4 = InitializeVertexAttribute(meshUV4);
            vertColors = InitializeVertexAttribute(meshColors);
            vertBoneWeights = InitializeVertexAttribute(meshBoneWeights);
        }
        #endregion

        #region Decimate Mesh
        /// <summary>
        /// Decimates the mesh.
        /// </summary>
        /// <param name="targetTrisCount">The target triangle count.</param>
        public override void DecimateMesh(int targetTrisCount)
        {
            if (targetTrisCount < 0)
                throw new ArgumentOutOfRangeException("targetTrisCount");

            int deletedTris = 0;
            ResizableArray<bool> deleted0 = new ResizableArray<bool>(20);
            ResizableArray<bool> deleted1 = new ResizableArray<bool>(20);
            var triangles = this.triangles.Data;
            int triangleCount = this.triangles.Length;
            int startTrisCount = triangleCount;
            var vertices = this.vertices.Data;

            int maxVertexCount = this.maxVertexCount;
            if (maxVertexCount <= 0)
                maxVertexCount = int.MaxValue;

            for (int iteration = 0; iteration < 100; iteration++)
            {
                ReportStatus(iteration, startTrisCount, (startTrisCount - deletedTris), targetTrisCount);
                if ((startTrisCount - deletedTris) <= targetTrisCount && remainingVertices < maxVertexCount)
                    break;

                // Update mesh once in a while
                if ((iteration % 5) == 0)
                {
                    UpdateMesh(iteration);
                    triangles = this.triangles.Data;
                    triangleCount = this.triangles.Length;
                    vertices = this.vertices.Data;
                }

                // Clear dirty flag
                for (int i = 0; i < triangleCount; i++)
                {
                    triangles[i].dirty = false;
                }

                // All triangles with edges below the threshold will be removed
                //
                // The following numbers works well for most models.
                // If it does not, try to adjust the 3 parameters
                double threshold = 0.000000001 * System.Math.Pow(iteration + 3, agressiveness);

                if (verbose && (iteration % 5) == 0)
                {
                    Logging.LogVerbose("iteration {0} - triangles {1} threshold {2}", iteration, (startTrisCount - deletedTris), threshold);
                }

                // Remove vertices & mark deleted triangles
                RemoveVertexPass(startTrisCount, targetTrisCount, threshold, deleted0, deleted1, ref deletedTris);
            }

            CompactMesh();
        }
        #endregion

        #region Decimate Mesh Lossless
        /// <summary>
        /// Decimates the mesh without losing any quality.
        /// </summary>
        public override void DecimateMeshLossless()
        {
            int deletedTris = 0;
            ResizableArray<bool> deleted0 = new ResizableArray<bool>(0);
            ResizableArray<bool> deleted1 = new ResizableArray<bool>(0);
            var triangles = this.triangles.Data;
            int triangleCount = this.triangles.Length;
            int startTrisCount = triangleCount;
            var vertices = this.vertices.Data;

            ReportStatus(0, startTrisCount, startTrisCount, -1);
            for (int iteration = 0; iteration < 9999; iteration++)
            {
                // Update mesh constantly
                UpdateMesh(iteration);
                triangles = this.triangles.Data;
                triangleCount = this.triangles.Length;
                vertices = this.vertices.Data;

                ReportStatus(iteration, startTrisCount, triangleCount, -1);

                // Clear dirty flag
                for (int i = 0; i < triangleCount; i++)
                {
                    triangles[i].dirty = false;
                }

                // All triangles with edges below the threshold will be removed
                //
                // The following numbers works well for most models.
                // If it does not, try to adjust the 3 parameters
                double threshold = DoubleEpsilon;

                if (verbose)
                {
                    Logging.LogVerbose("Lossless iteration {0}", iteration);
                }

                // Remove vertices & mark deleted triangles
                RemoveVertexPass(startTrisCount, 0, threshold, deleted0, deleted1, ref deletedTris);

                if (deletedTris <= 0)
                    break;

                deletedTris = 0;
            }

            CompactMesh();
        }
        #endregion

        #region To Mesh
        /// <summary>
        /// Returns the resulting mesh.
        /// </summary>
        /// <returns>The resulting mesh.</returns>
        public override Mesh ToMesh()
        {
            int vertexCount = this.vertices.Length;
            int triangleCount = this.triangles.Length;
            var vertices = new Vector3d[vertexCount];
            var indices = new int[subMeshCount][];

            var vertArr = this.vertices.Data;
            for (int i = 0; i < vertexCount; i++)
            {
                vertices[i] = vertArr[i].p;
            }

            // First get the sub-mesh offse-ts
            var triArr = this.triangles.Data;
            int[] subMeshOffsets = new int[subMeshCount];
            int lastSubMeshOffset = -1;
            for (int i = 0; i < triangleCount; i++)
            {
                var triangle = triArr[i];
                if (triangle.subMeshIndex != lastSubMeshOffset)
                {
                    for (int j = lastSubMeshOffset + 1; j < triangle.subMeshIndex; j++)
                    {
                        subMeshOffsets[j] = i - 1;
                    }
                    subMeshOffsets[triangle.subMeshIndex] = i;
                    lastSubMeshOffset = triangle.subMeshIndex;
                }
            }
            for (int i = lastSubMeshOffset + 1; i < subMeshCount; i++)
            {
                subMeshOffsets[i] = triangleCount;
            }

            // Then setup the sub-meshes
            for (int subMeshIndex = 0; subMeshIndex < subMeshCount; subMeshIndex++)
            {
                int startOffset = subMeshOffsets[subMeshIndex];
                int endOffset = ((subMeshIndex + 1) < subMeshCount ? subMeshOffsets[subMeshIndex + 1] : triangleCount) - 1;
                int subMeshTriangleCount = endOffset - startOffset + 1;
                if (subMeshTriangleCount < 0) subMeshTriangleCount = 0;
                int[] subMeshIndices = new int[subMeshTriangleCount * 3];
                for (int triangleIndex = startOffset; triangleIndex <= endOffset; triangleIndex++)
                {
                    var triangle = triArr[triangleIndex];
                    int offset = (triangleIndex - startOffset) * 3;
                    subMeshIndices[offset] = triangle.v0;
                    subMeshIndices[offset + 1] = triangle.v1;
                    subMeshIndices[offset + 2] = triangle.v2;
                }

                indices[subMeshIndex] = subMeshIndices;
            }

            Mesh newMesh = new Mesh(vertices, indices);

            if (vertNormals != null)
            {
                vertNormals.TrimExcess();
                newMesh.Normals = vertNormals.Data;
            }
            if (vertTangents != null)
            {
                vertTangents.TrimExcess();
                newMesh.Tangents = vertTangents.Data;
            }
            if (vertUV1 != null)
            {
                vertUV1.TrimExcess();
                newMesh.UV1 = vertUV1.Data;
            }
            if (vertUV2 != null)
            {
                vertUV2.TrimExcess();
                newMesh.UV2 = vertUV2.Data;
            }
            if (vertUV3 != null)
            {
                vertUV3.TrimExcess();
                newMesh.UV3 = vertUV3.Data;
            }
            if (vertUV4 != null)
            {
                vertUV4.TrimExcess();
                newMesh.UV4 = vertUV4.Data;
            }
            if (vertColors != null)
            {
                vertColors.TrimExcess();
                newMesh.Colors = vertColors.Data;
            }
            if (vertBoneWeights != null)
            {
                vertBoneWeights.TrimExcess();
                newMesh.BoneWeights = vertBoneWeights.Data;
            }

            return newMesh;
        }
        #endregion
        #endregion
    }
}