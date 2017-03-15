using System;
using MeshDecimator.Algorithms;

namespace MeshDecimator
{
    #region Algorithm
    /// <summary>
    /// The decimation algorithms.
    /// </summary>
    public enum Algorithm
    {
        /// <summary>
        /// The default algorithm.
        /// </summary>
        Default,
        /// <summary>
        /// The fast quadratic mesh simplification algorithm.
        /// </summary>
        FastQuadraticMesh
    }
    #endregion

    /// <summary>
    /// The mesh decimation API.
    /// </summary>
    public static class MeshDecimation
    {
        #region Public Methods
        #region Create Algorithm
        /// <summary>
        /// Creates a specific decimation algorithm.
        /// </summary>
        /// <param name="algorithm">The desired algorithm.</param>
        /// <returns>The decimation algorithm.</returns>
        public static DecimationAlgorithm CreateAlgorithm(Algorithm algorithm)
        {
            DecimationAlgorithm alg = null;

            switch (algorithm)
            {
                case Algorithm.Default:
                case Algorithm.FastQuadraticMesh:
                    alg = new FastQuadraticMeshSimplification();
                    break;
                default:
                    throw new ArgumentException("The specified algorithm is not supported.", "algorithm");
            }

            return alg;
        }
        #endregion

        #region Decimate Mesh
        /// <summary>
        /// Decimates a mesh.
        /// </summary>
        /// <param name="mesh">The mesh to decimate.</param>
        /// <param name="targetTriangleCount">The target triangle count.</param>
        /// <returns>The decimated mesh.</returns>
        public static Mesh DecimateMesh(Mesh mesh, int targetTriangleCount)
        {
            return DecimateMesh(Algorithm.Default, mesh, targetTriangleCount);
        }

        /// <summary>
        /// Decimates a mesh.
        /// </summary>
        /// <param name="algorithm">The desired algorithm.</param>
        /// <param name="mesh">The mesh to decimate.</param>
        /// <param name="targetTriangleCount">The target triangle count.</param>
        /// <returns>The decimated mesh.</returns>
        public static Mesh DecimateMesh(Algorithm algorithm, Mesh mesh, int targetTriangleCount)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            var decimationAlgorithm = CreateAlgorithm(algorithm);
            return DecimateMesh(decimationAlgorithm, mesh, targetTriangleCount);
        }

        /// <summary>
        /// Decimates a mesh.
        /// </summary>
        /// <param name="algorithm">The decimation algorithm.</param>
        /// <param name="mesh">The mesh to decimate.</param>
        /// <param name="targetTriangleCount">The target triangle count.</param>
        /// <returns>The decimated mesh.</returns>
        public static Mesh DecimateMesh(DecimationAlgorithm algorithm, Mesh mesh, int targetTriangleCount)
        {
            if (algorithm == null)
                throw new ArgumentNullException("algorithm");
            else if (mesh == null)
                throw new ArgumentNullException("mesh");

            int currentTriangleCount = mesh.TriangleCount;
            if (targetTriangleCount > currentTriangleCount)
                targetTriangleCount = currentTriangleCount;
            else if (targetTriangleCount < 0)
                targetTriangleCount = 0;

            algorithm.Initialize(mesh);
            algorithm.DecimateMesh(targetTriangleCount);
            return algorithm.ToMesh();
        }
        #endregion

        #region Decimate Mesh Lossless
        /// <summary>
        /// Decimates a mesh without losing any quality.
        /// </summary>
        /// <param name="mesh">The mesh to decimate.</param>
        /// <returns>The decimated mesh.</returns>
        public static Mesh DecimateMeshLossless(Mesh mesh)
        {
            return DecimateMeshLossless(Algorithm.Default, mesh);
        }

        /// <summary>
        /// Decimates a mesh without losing any quality.
        /// </summary>
        /// <param name="algorithm">The desired algorithm.</param>
        /// <param name="mesh">The mesh to decimate.</param>
        /// <returns>The decimated mesh.</returns>
        public static Mesh DecimateMeshLossless(Algorithm algorithm, Mesh mesh)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            var decimationAlgorithm = CreateAlgorithm(algorithm);
            return DecimateMeshLossless(decimationAlgorithm, mesh);
        }

        /// <summary>
        /// Decimates a mesh without losing any quality.
        /// </summary>
        /// <param name="algorithm">The decimation algorithm.</param>
        /// <param name="mesh">The mesh to decimate.</param>
        /// <returns>The decimated mesh.</returns>
        public static Mesh DecimateMeshLossless(DecimationAlgorithm algorithm, Mesh mesh)
        {
            if (algorithm == null)
                throw new ArgumentNullException("algorithm");
            else if (mesh == null)
                throw new ArgumentNullException("mesh");

            int currentTriangleCount = mesh.TriangleCount;
            algorithm.Initialize(mesh);
            algorithm.DecimateMeshLossless();
            return algorithm.ToMesh();
        }
        #endregion
        #endregion
    }
}