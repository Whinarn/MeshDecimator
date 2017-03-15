using System;

namespace MeshDecimator.Algorithms
{
    /// <summary>
    /// A decimation algorithm.
    /// </summary>
    public abstract class DecimationAlgorithm
    {
        #region Delegates
        /// <summary>
        /// A callback for decimation status reports.
        /// </summary>
        /// <param name="iteration">The current iteration, starting at zero.</param>
        /// <param name="originalTris">The original count of triangles.</param>
        /// <param name="currentTris">The current count of triangles.</param>
        /// <param name="targetTris">The target count of triangles.</param>
        public delegate void StatusReportCallback(int iteration, int originalTris, int currentTris, int targetTris);
        #endregion

        #region Fields
        /// <summary>
        /// If borders should be kept.
        /// </summary>
        protected bool keepBorders = false;

        /// <summary>
        /// If linked vertices should be kept.
        /// </summary>
        protected bool keepLinkedVertices = true;

        /// <summary>
        /// The maximum vertex count, if any. Zero means no limitation is imposed.
        /// </summary>
        protected int maxVertexCount = 0;

        /// <summary>
        /// If verbose information should be printed in the console.
        /// </summary>
        protected bool verbose = false;

        private StatusReportCallback statusReportInvoker = null;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets if borders should be kept.
        /// </summary>
        public bool KeepBorders
        {
            get { return keepBorders; }
            set { keepBorders = value; }
        }

        /// <summary>
        /// Gets or sets if linked vertices should be kept.
        /// </summary>
        public bool KeepLinkedVertices
        {
            get { return keepLinkedVertices; }
            set { keepLinkedVertices = value; }
        }

        /// <summary>
        /// Gets or sets the maximum vertex count. Set to zero for no limitation.
        /// </summary>
        public int MaxVertexCount
        {
            get { return maxVertexCount; }
            set { maxVertexCount = Math.MathHelper.Max(value, 0); }
        }

        /// <summary>
        /// Gets or sets if verbose information should be printed in the console.
        /// </summary>
        public bool Verbose
        {
            get { return verbose; }
            set { verbose = value; }
        }
        #endregion

        #region Events
        /// <summary>
        /// An event for status reports for this algorithm.
        /// </summary>
        public event StatusReportCallback StatusReport
        {
            add { statusReportInvoker += value; }
            remove { statusReportInvoker -= value; }
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Reports the current status of the decimation.
        /// </summary>
        /// <param name="iteration">The current iteration, starting at zero.</param>
        /// <param name="originalTris">The original count of triangles.</param>
        /// <param name="currentTris">The current count of triangles.</param>
        /// <param name="targetTris">The target count of triangles.</param>
        protected void ReportStatus(int iteration, int originalTris, int currentTris, int targetTris)
        {
            var statusReportInvoker = this.statusReportInvoker;
            if (statusReportInvoker != null)
            {
                statusReportInvoker.Invoke(iteration, originalTris, currentTris, targetTris);
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Initializes the algorithm with the original mesh.
        /// </summary>
        /// <param name="mesh">The mesh.</param>
        public abstract void Initialize(Mesh mesh);

        /// <summary>
        /// Decimates the mesh.
        /// </summary>
        /// <param name="targetTrisCount">The target triangle count.</param>
        public abstract void DecimateMesh(int targetTrisCount);

        /// <summary>
        /// Decimates the mesh without losing any quality.
        /// </summary>
        public abstract void DecimateMeshLossless();

        /// <summary>
        /// Returns the resulting mesh.
        /// </summary>
        /// <returns>The resulting mesh.</returns>
        public abstract Mesh ToMesh();
        #endregion
    }
}