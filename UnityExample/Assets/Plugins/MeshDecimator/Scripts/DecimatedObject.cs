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