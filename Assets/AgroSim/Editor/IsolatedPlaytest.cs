using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AgroSim.Editor
{
    // Editor-only manual QA. Both save and telemetry use this temporary directory.
    [InitializeOnLoad]
    public static class IsolatedPlaytest
    {
        private const string Key="AgroSim.PlaytestSaveDirectory";
        private const string LastKey="AgroSim.LastPlaytestDirectory";
        static IsolatedPlaytest()
        {
            EditorApplication.playModeStateChanged+=state=>
            {
                if(state==PlayModeStateChange.EnteredEditMode) SessionState.EraseString(Key);
            };
        }
        [MenuItem("AgroSim/Testes/Play com save isolado")]
        public static void Start()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) return;
            string directory=Path.Combine(Path.GetTempPath(),"AgroSim-playtest-"+Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            SessionState.SetString(LastKey,directory);
            SessionState.SetString(Key,directory);
            Debug.Log("AgroSim isolated playtest directory: "+directory);
            EditorApplication.isPlaying=true;
        }
        [MenuItem("AgroSim/Testes/Retomar último save de teste")]
        public static void Resume()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) return;
            string directory=SessionState.GetString(LastKey,string.Empty);
            string prefix=Path.Combine(Path.GetTempPath(),"AgroSim-playtest-");
            if(string.IsNullOrEmpty(directory) || !Path.GetFullPath(directory).StartsWith(prefix,StringComparison.OrdinalIgnoreCase) || !Directory.Exists(directory))
            { Debug.LogWarning("AgroSim: não há sessão de teste para retomar neste Editor."); return; }
            SessionState.SetString(Key,directory);
            Debug.Log("AgroSim resumed isolated playtest directory: "+directory);
            EditorApplication.isPlaying=true;
        }
    }
}
