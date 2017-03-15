using UnityEngine;
using UnityEngine.Rendering;

namespace MeshDecimator.Unity
{
    /// <summary>
    /// An object to be decimated.
    /// </summary>
    [AddComponentMenu("MeshDecimator/Decimated Object")]
    public sealed class DecimatedObject : MonoBehaviour
    {
        #region Fields
        [SerializeField]
        private LODSettings[] levels = null;

        [SerializeField]
        private bool generated = false;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the LOD levels of this object.
        /// </summary>
        public LODSettings[] Levels
        {
            get { return levels; }
            set { levels = value; }
        }

        /// <summary>
        /// Gets if this decimated object has been generated.
        /// </summary>
        public bool IsGenerated
        {
            get { return generated; }
        }
        #endregion

        #region Unity Events
        private void Reset()
        {
            levels = new LODSettings[]
            {
             new LODSettings(0.8f, SkinQuality.Auto, true, ShadowCastingMode.On),
             new LODSettings(0.65f, SkinQuality.Bone2, true, ShadowCastingMode.Off, MotionVectorGenerationMode.Object, false),
             new LODSettings(0.4f, SkinQuality.Bone1, false, ShadowCastingMode.Off, MotionVectorGenerationMode.Object, false)
            };
            ResetLODs();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Generates the LODs for this object.
        /// </summary>
        /// <param name="statusCallback">The status report callback.</param>
        public void GenerateLODs(LODStatusReportCallback statusCallback = null)
        {
            if (levels != null)
            {
                LODGenerator.GenerateLODs(gameObject, levels, statusCallback);
            }
            generated = true;
        }

        /// <summary>
        /// Resets the LODs for this object.
        /// </summary>
        public void ResetLODs()
        {
            LODGenerator.DestroyLODs(gameObject);
            generated = false;
        }
        #endregion
    }
}