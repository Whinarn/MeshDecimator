using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using MeshDecimator.Algorithms;

namespace MeshDecimator.Unity
{
    using Mesh = UnityEngine.Mesh;

    #region Delegates
    /// <summary>
    /// A callback for LOD status reports.
    /// </summary>
    /// <param name="lodLevel">The LOD level.</param>
    /// <param name="iteration">The current iteration for this LOD level, starting at zero.</param>
    /// <param name="originalTris">The original count of triangles.</param>
    /// <param name="currentTris">The current count of triangles for this LOD level.</param>
    /// <param name="targetTris">The target count of triangles for this LOD level.</param>
    public delegate void LODStatusReportCallback(int lodLevel, int iteration, int originalTris, int currentTris, int targetTris);
    #endregion

    #region LOD Settings
    /// <summary>
    /// LOD Level Settings.
    /// </summary>
    [System.Serializable]
    public struct LODSettings
    {
        #region Fields
        /// <summary>
        /// The LOD level quality between 0 and 1.
        /// </summary>
        [Range(0.01f, 1f)]
        public float quality;
        /// <summary>
        /// The LOD level skin quality.
        /// </summary>
        public SkinQuality skinQuality;
        /// <summary>
        /// If the LOD level receives shadows.
        /// </summary>
        public bool receiveShadows;
        /// <summary>
        /// The LOD level shadow casting mode.
        /// </summary>
        public ShadowCastingMode shadowCasting;
        /// <summary>
        /// The LOD level motion vectors generation mode.
        /// </summary>
        public MotionVectorGenerationMode motionVectors;
        /// <summary>
        /// If the LOD level uses skinned motion vectors.
        /// </summary>
        public bool skinnedMotionVectors;

        /// <summary>
        /// The LOD level light probe usage.
        /// </summary>
        public LightProbeUsage lightProbeUsage;
        /// <summary>
        /// The LOD level reflection probe usage.
        /// </summary>
        public ReflectionProbeUsage reflectionProbeUsage;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates new LOD Level Settings.
        /// </summary>
        /// <param name="quality">The LOD level quality.</param>
        public LODSettings(float quality)
        {
            this.quality = quality;
            this.skinQuality = SkinQuality.Auto;
            this.receiveShadows = true;
            this.shadowCasting = ShadowCastingMode.On;
            this.motionVectors = MotionVectorGenerationMode.Object;
            this.skinnedMotionVectors = true;

            this.lightProbeUsage = LightProbeUsage.BlendProbes;
            this.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
        }

        /// <summary>
        /// Creates new LOD Level Settings.
        /// </summary>
        /// <param name="quality">The quality.</param>
        /// <param name="skinQuality">The skin quality.</param>
        public LODSettings(float quality, SkinQuality skinQuality)
        {
            this.quality = quality;
            this.skinQuality = skinQuality;
            this.receiveShadows = true;
            this.shadowCasting = ShadowCastingMode.On;
            this.motionVectors = MotionVectorGenerationMode.Object;
            this.skinnedMotionVectors = true;

            this.lightProbeUsage = LightProbeUsage.BlendProbes;
            this.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
        }

        /// <summary>
        /// Creates new LOD Level Settings.
        /// </summary>
        /// <param name="quality">The quality.</param>
        /// <param name="skinQuality">The skin quality.</param>
        /// <param name="receiveShadows">If receiving shadows.</param>
        /// <param name="shadowCasting">The shadow casting mode.</param>
        public LODSettings(float quality, SkinQuality skinQuality, bool receiveShadows, ShadowCastingMode shadowCasting)
        {
            this.quality = quality;
            this.skinQuality = skinQuality;
            this.receiveShadows = receiveShadows;
            this.shadowCasting = shadowCasting;
            this.motionVectors = MotionVectorGenerationMode.Object;
            this.skinnedMotionVectors = true;

            this.lightProbeUsage = LightProbeUsage.BlendProbes;
            this.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
        }

        /// <summary>
        /// Creates new LOD Level Settings.
        /// </summary>
        /// <param name="quality">The quality.</param>
        /// <param name="skinQuality">The skin quality.</param>
        /// <param name="receiveShadows">If receiving shadows.</param>
        /// <param name="shadowCasting">The shadow casting mode.</param>
        /// <param name="motionVectors">The motion vector generation mode.</param>
        /// <param name="skinnedMotionVectors">If motion vectors are skinned.</param>
        public LODSettings(float quality, SkinQuality skinQuality, bool receiveShadows, ShadowCastingMode shadowCasting, MotionVectorGenerationMode motionVectors, bool skinnedMotionVectors)
        {
            this.quality = quality;
            this.skinQuality = skinQuality;
            this.receiveShadows = receiveShadows;
            this.shadowCasting = shadowCasting;
            this.motionVectors = motionVectors;
            this.skinnedMotionVectors = skinnedMotionVectors;

            this.lightProbeUsage = LightProbeUsage.BlendProbes;
            this.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
        }
        #endregion
    }
    #endregion

    /// <summary>
    /// A LOD generator API.
    /// </summary>
    public static class LODGenerator
    {
        #region Consts
        private const string ParentGameObjectName = "_LOD_";
        #endregion

        #region Private Methods
        #region Combine Renderers
        private static Renderer[] CombineRenderers(MeshRenderer[] meshRenderers, SkinnedMeshRenderer[] skinnedRenderers)
        {
            int totalRendererCount = meshRenderers.Length + skinnedRenderers.Length;
            var newRenderers = new Renderer[totalRendererCount];
            System.Array.Copy(meshRenderers, 0, newRenderers, 0, meshRenderers.Length);
            System.Array.Copy(skinnedRenderers, 0, newRenderers, meshRenderers.Length, skinnedRenderers.Length);
            return newRenderers;
        }
        #endregion

        #region Generate Static LOD
        private static Mesh GenerateStaticLOD(Transform transform, MeshRenderer[] renderers, float quality, out Material[] materials, DecimationAlgorithm.StatusReportCallback statusCallback)
        {
            if (renderers.Length == 1)
            {
                var renderer = renderers[0];
                var rendererTransform = renderer.transform;
                var meshFilter = renderer.GetComponent<MeshFilter>();
                var mesh = meshFilter.sharedMesh;
                var meshTransform = transform.worldToLocalMatrix * rendererTransform.localToWorldMatrix;
                materials = renderer.sharedMaterials;
                return MeshDecimatorUtility.DecimateMesh(mesh, meshTransform, quality, false, statusCallback);
            }
            else
            {
                var sourceMeshes = new Mesh[renderers.Length];
                var transforms = new Matrix4x4[renderers.Length];
                var meshMaterials = new Material[renderers.Length][];
                for (int i = 0; i < renderers.Length; i++)
                {
                    var renderer = renderers[i];
                    var rendererTransform = renderer.transform;
                    var meshFilter = renderer.GetComponent<MeshFilter>();
                    sourceMeshes[i] = meshFilter.sharedMesh;
                    transforms[i] = transform.worldToLocalMatrix * rendererTransform.localToWorldMatrix;
                    meshMaterials[i] = renderer.sharedMaterials;
                }
                return MeshDecimatorUtility.DecimateMeshes(sourceMeshes, transforms, meshMaterials, quality, false, out materials, statusCallback);
            }
        }
        #endregion

        #region Generate Skinned LOD
        private static Mesh GenerateSkinnedLOD(Transform transform, SkinnedMeshRenderer[] renderers, float quality, out Material[] materials, out Transform[] mergedBones, DecimationAlgorithm.StatusReportCallback statusCallback)
        {
            if (renderers.Length == 1)
            {
                var renderer = renderers[0];
                var rendererTransform = renderer.transform;
                var mesh = renderer.sharedMesh;
                var meshTransform = transform.worldToLocalMatrix * rendererTransform.localToWorldMatrix;
                materials = renderer.sharedMaterials;
                mergedBones = renderer.bones;
                return MeshDecimatorUtility.DecimateMesh(mesh, meshTransform, quality, false, statusCallback);
            }
            else
            {
                var sourceMeshes = new Mesh[renderers.Length];
                var transforms = new Matrix4x4[renderers.Length];
                var meshMaterials = new Material[renderers.Length][];
                var meshBones = new Transform[renderers.Length][];
                for (int i = 0; i < renderers.Length; i++)
                {
                    var renderer = renderers[i];
                    var rendererTransform = renderer.transform;
                    sourceMeshes[i] = renderer.sharedMesh;
                    transforms[i] = transform.worldToLocalMatrix * rendererTransform.localToWorldMatrix;
                    meshMaterials[i] = renderer.sharedMaterials;
                    meshBones[i] = renderer.bones;
                }
                return MeshDecimatorUtility.DecimateMeshes(sourceMeshes, transforms, meshMaterials, meshBones, quality, false, out materials, out mergedBones, statusCallback);
            }
        }
        #endregion

        #region Find Root Bone
        private static Transform FindRootBone(Transform transform, Transform[] transforms)
        {
            Transform result = null;
            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i].parent == transform)
                {
                    result = transforms[i];

                    // If we found a transform directly under our root with more than one child, we assume it to be root
                    if (transforms[i].childCount > 0)
                        break;
                }
            }
            return result;
        }
        #endregion

        #region Setup LOD Renderer
        private static void SetupLODRenderer(Renderer renderer, LODSettings settings)
        {
            renderer.shadowCastingMode = settings.shadowCasting;
            renderer.receiveShadows = settings.receiveShadows;
            renderer.motionVectorGenerationMode = settings.motionVectors;

            renderer.lightProbeUsage = settings.lightProbeUsage;
            renderer.reflectionProbeUsage = settings.reflectionProbeUsage;

            SkinnedMeshRenderer skinnedRenderer = renderer as SkinnedMeshRenderer;
            if (skinnedRenderer != null)
            {
                skinnedRenderer.skinnedMotionVectors = settings.skinnedMotionVectors;
                skinnedRenderer.quality = settings.skinQuality;
            }
        }
        #endregion
        #endregion

        #region Public Methods
        #region Generate LODs
        /// <summary>
        /// Generates the LODs and sets up a LOD Group for the specified game object.
        /// </summary>
        /// <param name="gameObj">The game object to set up.</param>
        /// <param name="levels">The LOD levels.</param>
        /// <param name="statusCallback">The optional status report callback.</param>
        public static void GenerateLODs(GameObject gameObj, LODSettings[] levels, LODStatusReportCallback statusCallback = null)
        {
            DestroyLODs(gameObj);

            var transform = gameObj.transform;
            var meshRenderers = gameObj.GetComponentsInChildren<MeshRenderer>();
            var skinnedRenderers = gameObj.GetComponentsInChildren<SkinnedMeshRenderer>();

            // Check if there's anything to do
            if (meshRenderers.Length == 0 && skinnedRenderers.Length == 0)
                return;

            var lodsParentObj = new GameObject(ParentGameObjectName);
            var lodsParent = lodsParentObj.transform;
            lodsParent.parent = transform;
            lodsParent.localPosition = Vector3.zero;
            lodsParent.localRotation = Quaternion.identity;
            lodsParent.localScale = Vector3.one;

            var lodGroup = gameObj.GetComponent<LODGroup>();
            if (lodGroup == null)
            {
                lodGroup = gameObj.AddComponent<LODGroup>();
            }

            float screenRelativeTransitionHeight = 0.5f;
            LOD[] lodLevels = new LOD[levels.Length + 1];
            var firstLevelRenderers = CombineRenderers(meshRenderers, skinnedRenderers);
            lodLevels[0] = new LOD(screenRelativeTransitionHeight, firstLevelRenderers);
            screenRelativeTransitionHeight *= 0.5f;

            float minimumQuality = 1f;
            for (int i = 0; i < levels.Length; i++)
            {
                var level = levels[i];
                float quality = Mathf.Clamp(level.quality, 0.01f, minimumQuality);
                screenRelativeTransitionHeight *= quality * quality;
                GameObject lodObj = new GameObject(string.Format("Level{0}", i));
                var lodParent = lodObj.transform;
                lodParent.parent = lodsParent;
                lodParent.localPosition = Vector3.zero;
                lodParent.localRotation = Quaternion.identity;
                lodParent.localScale = Vector3.one;

                DecimationAlgorithm.StatusReportCallback levelStatusCallback = null;
                if (statusCallback != null)
                {
                    levelStatusCallback = (iteration, originalTris, currentTris, targetTris) =>
                    {
                        statusCallback.Invoke(i, iteration, originalTris, currentTris, targetTris);
                    };
                }

                Renderer[] lodRenderers = null;
                if (skinnedRenderers.Length == 0)
                {
                    Material[] materials;
                    Mesh lodMesh = GenerateStaticLOD(transform, meshRenderers, quality, out materials, levelStatusCallback);
                    lodMesh.name = string.Format("{0}_static{1}", gameObj.name, i);
                    var meshFilter = lodObj.AddComponent<MeshFilter>();
                    meshFilter.sharedMesh = lodMesh;
                    var lodRenderer = lodObj.AddComponent<MeshRenderer>();
                    lodRenderer.sharedMaterials = materials;
                    SetupLODRenderer(lodRenderer, level);
                    lodRenderers = new Renderer[] { lodRenderer };
                }
                else if (meshRenderers.Length == 0)
                {
                    Transform[] bones;
                    Material[] materials;
                    Mesh lodMesh = GenerateSkinnedLOD(transform, skinnedRenderers, quality, out materials, out bones, levelStatusCallback);
                    lodMesh.name = string.Format("{0}_skinned{1}", gameObj.name, i);
                    Transform rootBone = FindRootBone(transform, bones);

                    var lodRenderer = lodObj.AddComponent<SkinnedMeshRenderer>();
                    lodRenderer.sharedMesh = lodMesh;
                    lodRenderer.sharedMaterials = materials;
                    lodRenderer.rootBone = rootBone;
                    lodRenderer.bones = bones;
                    SetupLODRenderer(lodRenderer, level);
                    lodRenderers = new Renderer[] { lodRenderer };
                }
                else
                {
                    MeshRenderer lodMeshRenderer = null;
                    SkinnedMeshRenderer lodSkinnedRenderer = null;
                    {
                        Material[] materials;
                        Mesh lodMesh = GenerateStaticLOD(transform, meshRenderers, quality, out materials, levelStatusCallback);
                        lodMesh.name = string.Format("{0}_static{1}", gameObj.name, i);
                        GameObject staticObj = new GameObject("Static", typeof(MeshFilter), typeof(MeshRenderer));
                        staticObj.transform.parent = lodParent;
                        staticObj.transform.localPosition = Vector3.zero;
                        staticObj.transform.localRotation = Quaternion.identity;
                        staticObj.transform.localScale = Vector3.one;

                        var meshFilter = staticObj.GetComponent<MeshFilter>();
                        meshFilter.sharedMesh = lodMesh;
                        var lodRenderer = staticObj.GetComponent<MeshRenderer>();
                        lodRenderer.sharedMaterials = materials;
                        SetupLODRenderer(lodRenderer, level);
                        lodMeshRenderer = lodRenderer;
                    }
                    {
                        Transform[] bones;
                        Material[] materials;
                        Mesh lodMesh = GenerateSkinnedLOD(transform, skinnedRenderers, quality, out materials, out bones, levelStatusCallback);
                        lodMesh.name = string.Format("{0}_skinned{1}", gameObj.name, i);
                        Transform rootBone = FindRootBone(transform, bones);

                        GameObject staticObj = new GameObject("Skinned", typeof(SkinnedMeshRenderer));
                        staticObj.transform.parent = lodParent;
                        staticObj.transform.localPosition = Vector3.zero;
                        staticObj.transform.localRotation = Quaternion.identity;
                        staticObj.transform.localScale = Vector3.one;

                        var lodRenderer = staticObj.GetComponent<SkinnedMeshRenderer>();
                        lodRenderer.sharedMesh = lodMesh;
                        lodRenderer.sharedMaterials = materials;
                        lodRenderer.rootBone = rootBone;
                        lodRenderer.bones = bones;
                        SetupLODRenderer(lodRenderer, level);
                        lodSkinnedRenderer = lodRenderer;
                    }

                    lodRenderers = new Renderer[] { lodMeshRenderer, lodSkinnedRenderer };
                }

                minimumQuality = quality;
                lodLevels[i + 1] = new LOD(screenRelativeTransitionHeight, lodRenderers);
            }

            lodGroup.SetLODs(lodLevels);
        }
        #endregion

        #region Destroy LODs
        /// <summary>
        /// Destroys the generated LODs for the specified game object.
        /// </summary>
        /// <param name="gameObj">The game object to destroy LODs for.</param>
        public static void DestroyLODs(GameObject gameObj)
        {
            if (gameObj == null)
                throw new System.ArgumentNullException("gameObj");

            var targetTransform = gameObj.transform;
            var lodsParent = targetTransform.FindChild(ParentGameObjectName);
            if (lodsParent != null)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(lodsParent.gameObject);
                }
                else
                {
                    Object.DestroyImmediate(lodsParent.gameObject);
                }

                // Also destroy any LOD group on the game object, but only if we found generated LODs on it
                var lodGroup = gameObj.GetComponent<LODGroup>();
                if (lodGroup != null)
                {
                    if (Application.isPlaying)
                    {
                        Object.Destroy(lodGroup);
                    }
                    else
                    {
                        Object.DestroyImmediate(lodGroup);
                    }
                }
            }
        }
        #endregion
        #endregion
    }
}