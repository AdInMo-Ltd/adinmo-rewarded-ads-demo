using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


// Declare type of Custom Editor
namespace Adinmo
{
    [CustomEditor(typeof(AdinmoTexture))]
    public class AdinmoTextureEditor : Editor
    {

        private SerializedProperty testImage;
        // OnInspector GUI
        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspector();
            AdinmoTexture adinmoTexture = (AdinmoTexture)target;
            SerializedObject serObj = new(adinmoTexture);
            List<string> labels = new();
            int displayMask = 0;

            for (int i = 0; i < 32; i++)
            {
                if (LayerMask.LayerToName(i) != "")
                {
                    int tempMask = 1 << labels.Count;
                    labels.Add(LayerMask.LayerToName(i));

                    if ((1 << i & adinmoTexture.layerMask) != 0)
                        displayMask |= tempMask;
                }
            }

            if (labels.Count > 0)
            {
                int newDisplayMask = EditorGUILayout.MaskField(new GUIContent("Occlusion test ignores Layers","Objects on selected layers won't block impression detection"), displayMask, labels.ToArray());
                if (newDisplayMask != -1 && newDisplayMask != displayMask)
                {
                    int newMask = 0;
                    for (int i = 0; i < labels.Count; i++)
                    {
                        if ((newDisplayMask & 1 << i) != 0)
                        {
                            newMask |= 1 << LayerMask.NameToLayer(labels[i]);
                        }
                    }
                    Debug.Log("old layer mask=" + adinmoTexture.layerMask + ", new=" + newMask);

                    serializedObject.FindProperty("layerMask").intValue = newMask;
                }
                else if (newDisplayMask == -1)
                {
                    Debug.LogError("Can't filter out all layers");
                }
                serializedObject.ApplyModifiedProperties();
            }
            string statusString=adinmoTexture.GetStatusString();
            if (!string.IsNullOrEmpty(statusString))
            {
                GUIStyle TextFieldStyles = new(EditorStyles.label)
                {
                    fontStyle = FontStyle.Bold
                };
                TextFieldStyles.normal.textColor = Color.red;
                EditorGUILayout.LabelField(statusString,TextFieldStyles);

            }
            if (Application.isPlaying)
            {
                if (adinmoTexture.GetObjectType() == AdinmoReplace.ObjectType.Image || !(AdinmoManager.s_manager!=null && AdinmoManager.s_manager.imageRenderDebug))
                {
                    EditorGUILayout.LabelField("Latest Sample", ("" + (adinmoTexture.LatestSample.sample * 100).ToString("F2") + "%"));
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.PrefixLabel("Sample Texture");
                    GUILayout.Button(adinmoTexture.SampleTexture, GUILayout.Width(48), GUILayout.Height(48));
                    EditorGUILayout.BeginVertical(GUILayout.Width(50));
                    GUILayout.Label("Latest Sample");
                    GUILayout.Label("" + (adinmoTexture.LatestSample.sample * 100).ToString("F2") + "%");

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
    }
}
