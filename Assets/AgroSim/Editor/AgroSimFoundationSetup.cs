using System.IO;
using AgroSim.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AgroSim.Editor
{
    [InitializeOnLoad]
    public static class AgroSimFoundationSetup
    {
        private const string ScenePath = "Assets/AgroSim/Scenes/Gameplay.unity";

        static AgroSimFoundationSetup()
        {
            EditorApplication.delayCall += CreateInitialScene;
        }

        private static void CreateInitialScene()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += CreateInitialScene;
                return;
            }

            // Keep the template scene and any unsaved work open alongside Gameplay.
            if (File.Exists(ScenePath)) return;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene);
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.16f, 0.23f, 0.16f);
            cameraObject.AddComponent<AudioListener>();
            new GameObject("GameManager").AddComponent<GameManager>();
            EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new IOException("Could not save AgroSim Gameplay scene.");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("AgroSim foundation ready: Gameplay saved, GameManager attached, scripts compiled.");
        }
    }
}
