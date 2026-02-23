using FunctionalBlocks;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FunctionalBlocks.Editor
{
    [CustomEditor(typeof(GraphAsset))]
    public class GraphAssetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var dataProp = serializedObject.FindProperty("data");
            if (dataProp == null)
            {
                EditorGUILayout.HelpBox("Missing 'data' field in GraphAsset.", MessageType.Error);
                serializedObject.ApplyModifiedProperties();
                return;
            }

            var formatVersion = dataProp.FindPropertyRelative("formatVersion");
            var startBlockId = dataProp.FindPropertyRelative("startBlockId");
            var variables = dataProp.FindPropertyRelative("variables");
            var blocks = dataProp.FindPropertyRelative("blocks");

            EditorGUILayout.LabelField("Graph", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(formatVersion);
            EditorGUILayout.PropertyField(startBlockId);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Variables", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(variables, true);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Blocks", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(blocks, true);

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("JSON", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Export JSON..."))
                    ExportJson((GraphAsset)target);

                if (GUILayout.Button("Import JSON..."))
                    ImportJson((GraphAsset)target);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static void ExportJson(GraphAsset graph)
        {
            if (graph == null || graph.data == null)
            {
                EditorUtility.DisplayDialog("Export JSON", "GraphAsset or its data is null.", "OK");
                return;
            }

            // Default file name
            string defaultName = $"Graph_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}";

            string path = EditorUtility.SaveFilePanel(
                "Export Graph to JSON",
                Application.dataPath,
                defaultName,
                "json"
            );

            if (string.IsNullOrWhiteSpace(path))
                return;

            try
            {
                string json = JsonUtility.ToJson(graph.data, true);
                File.WriteAllText(path, json);
                EditorUtility.RevealInFinder(path);
                EditorUtility.DisplayDialog("Export JSON", "JSON exported.", "OK");
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
                EditorUtility.DisplayDialog("Export JSON", "Error saving JSON. Check the console.", "OK");
            }
        }

        private static void ImportJson(GraphAsset graph)
        {
            if (graph == null)
            {
                EditorUtility.DisplayDialog("Import JSON", "GraphAsset is null.", "OK");
                return;
            }

            string path = EditorUtility.OpenFilePanel(
                "Import Graph from JSON",
                Application.dataPath,
                "json"
            );

            if (string.IsNullOrWhiteSpace(path))
                return;

            try
            {
                string json = File.ReadAllText(path);

                Undo.RecordObject(graph, "Import Graph JSON");

                if (graph.data == null)
                    graph.data = new GraphData();

                JsonUtility.FromJsonOverwrite(json, graph.data);

                EditorUtility.SetDirty(graph);
                AssetDatabase.SaveAssets();

                EditorUtility.DisplayDialog("Import JSON", "JSON imported into GraphAsset.", "OK");
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
                EditorUtility.DisplayDialog("Import JSON", "Error importing JSON (format or data). Check the console.", "OK");
            }
        }
    }
}