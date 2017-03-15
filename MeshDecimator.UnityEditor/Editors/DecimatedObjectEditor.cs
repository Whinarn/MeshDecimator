using UnityEngine;
using UnityEditor;
using MeshDecimator.Unity;

namespace MeshDecimator.UnityEditor
{
    /// <summary>
    /// An decimated object editor.
    /// </summary>
    [CustomEditor(typeof(DecimatedObject))]
    public class DecimatedObjectEditor : Editor
    {
        #region Fields
        private SerializedProperty levelsProp = null;
        private SerializedProperty generatedProp = null;

        private bool isGeneratingNew = false;
        private bool[] settingsExpanded = null;

        private static readonly GUIContent settingsContent = new GUIContent("Settings");
        #endregion

        #region Properties
        private new DecimatedObject target
        {
            get { return base.target as DecimatedObject; }
        }
        #endregion

        #region Unity Events
        private void OnEnable()
        {
            levelsProp = serializedObject.FindProperty("levels");
            generatedProp = serializedObject.FindProperty("generated");

            isGeneratingNew = !generatedProp.boolValue;
        }
        #endregion

        #region Private Methods
        private void GenerateLODs()
        {
            EditorUtility.DisplayProgressBar("Generating LODs", "Preparations...", 0f);
            try
            {
                int levelCount = target.Levels.Length;
                target.GenerateLODs((lodLevel, iteration, originalTris, currentTris, targetTris) =>
                {
                    float reduction = (1f - ((float)currentTris / (float)originalTris)) * 100f;
                    string statusText;
                    if (targetTris >= 0)
                    {
                        statusText = string.Format("Level {0}/{1}, triangles {2}/{3} ({4:0.00}% reduction), target {5}", lodLevel + 1, levelCount, currentTris, originalTris, reduction, targetTris);
                    }
                    else
                    {
                        statusText = string.Format("Level {0}/{1}, triangles {2}/{3} ({4:0.00}% reduction)", lodLevel + 1, levelCount, currentTris, originalTris, reduction);
                    }
                    float progress = (float)lodLevel / (float)levelCount;
                    EditorUtility.DisplayProgressBar("Generating LODs", statusText, progress);
                });
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            generatedProp.boolValue = true;
            isGeneratingNew = false;
        }

        private void ResetLODs()
        {
            generatedProp.boolValue = false;
            isGeneratingNew = true;

            target.ResetLODs();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Renders the editor inspector GUI.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (isGeneratingNew)
            {
                int levelsCount = levelsProp.arraySize;
                if (settingsExpanded == null || settingsExpanded.Length != levelsCount)
                {
                    bool[] newExpanded = new bool[levelsCount];
                    if (settingsExpanded != null)
                    {
                        System.Array.Copy(settingsExpanded, 0, newExpanded, 0, Mathf.Min(settingsExpanded.Length, levelsCount));
                    }
                    settingsExpanded = newExpanded;
                }

                float minimumQuality = 1f;
                var buttonWidth = GUILayout.Width(16f);
                for (int i = 0; i < levelsCount; i++)
                {
                    var levelProp = levelsProp.GetArrayElementAtIndex(i);
                    var qualityProp = levelProp.FindPropertyRelative("quality");
                    GUILayout.Label(string.Format("Level {0}", (i + 1)), EditorStyles.boldLabel);
                    ++EditorGUI.indentLevel;
                    EditorGUILayout.BeginHorizontal();
                    float quality = qualityProp.floatValue;
                    if (quality > minimumQuality) quality = minimumQuality;
                    qualityProp.floatValue = EditorGUILayout.Slider(quality, 0.01f, 1f);
                    if (GUILayout.Button("-", buttonWidth))
                    {
                        levelsProp.DeleteArrayElementAtIndex(i);
                        serializedObject.ApplyModifiedProperties();
                        GUIUtility.ExitGUI();
                    }
                    EditorGUILayout.EndHorizontal();
                    minimumQuality = qualityProp.floatValue;

                    settingsExpanded[i] = EditorGUILayout.Foldout(settingsExpanded[i], settingsContent);
                    if (settingsExpanded[i])
                    {
                        int exitDepth = levelProp.depth;
                        var childProp = levelProp.Copy();
                        while (childProp.NextVisible(true) && childProp.depth > exitDepth)
                        {
                            if (!string.Equals(childProp.name, "quality"))
                            {
                                EditorGUILayout.PropertyField(childProp);
                            }
                        }
                    }
                    --EditorGUI.indentLevel;
                }

                if (GUILayout.Button("Add Level"))
                {
                    levelsProp.InsertArrayElementAtIndex(levelsCount);
                    var newLevel = levelsProp.GetArrayElementAtIndex(levelsCount);
                    if (newLevel != null)
                    {
                        var qualityProp = newLevel.FindPropertyRelative("quality");
                        if (qualityProp != null)
                        {
                            qualityProp.floatValue = minimumQuality * 0.65f;
                        }
                    }
                    serializedObject.ApplyModifiedProperties();
                    GUIUtility.ExitGUI();
                }

                GUILayout.Space(10f);
                if (GUILayout.Button("Generate LODs"))
                {
                    GenerateLODs();
                    serializedObject.ApplyModifiedProperties();
                    GUIUtility.ExitGUI();
                }
            }
            else
            {
                if (GUILayout.Button("Reconfigure LODs"))
                {
                    isGeneratingNew = true;
                }
                if (GUILayout.Button("Reset LODs"))
                {
                    ResetLODs();
                    serializedObject.ApplyModifiedProperties();
                    GUIUtility.ExitGUI();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
        #endregion
    }
}