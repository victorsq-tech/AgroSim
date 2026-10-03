using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using AgroSim.Operations;

namespace AgroSim.Editor
{
    [InitializeOnLoad] public static class ReturnTelemetryValidation
    {
        static ReturnTelemetryValidation() { EditorApplication.delayCall+=Validate; }
        [MenuItem("AgroSim/Validar telemetria após retorno")]
        public static void Validate()
        {
            if(EditorApplication.isCompiling || EditorApplication.isUpdating) { EditorApplication.delayCall+=Validate; return; }
            if(EditorApplication.isPlayingOrWillChangePlaymode) return;
            string root=Path.Combine(Path.GetTempPath(),"AgroSim-return-"+Guid.NewGuid().ToString("N"));
            var host=new GameObject("Return telemetry test"); host.SetActive(false); host.hideFlags=HideFlags.HideAndDontSave;
            try
            {
                var type=Type.GetType("AgroSim.UI.PlaceholderSlice, Assembly-CSharp");
                var ui=host.AddComponent(type); const BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
                var operation=host.AddComponent<PlaceholderOperation>();
                // Machine has returned; operation was reset to the previous checkpoint.
                operation.Configure(100); operation.RestoreMachine(80,4);
                type.GetField("operation",flags).SetValue(ui,operation);
                type.GetField("outcomeCompleted",flags).SetValue(ui,true);
                type.GetField("outcomeProductivity",flags).SetValue(ui,85f);
                var log=type.GetField("log",flags).GetValue(ui);
                var logType=log.GetType(); string id=Guid.NewGuid().ToString("N");
                logType.GetMethod("Begin",new[]{typeof(string),typeof(string),typeof(string),typeof(string)}).Invoke(log,new object[]{id,id,id,root});
                type.GetMethod("Record",flags).Invoke(ui,new object[]{"confirmed",null});
                string json=File.ReadAllText(Path.Combine(root,id+".jsonl"));
                if(!json.Contains("\"progress\":1.0") || !json.Contains("\"productivityPercent\":85.0")) throw new Exception("Confirmation retry must log the preserved outcome.");
                Debug.Log("AgroSim return telemetry validation PASSED: confirmation uses outcome after reset.");
            }
            catch(Exception e) { Debug.LogError("AgroSim return telemetry validation FAILED: "+(e.InnerException??e).Message); }
            finally { UnityEngine.Object.DestroyImmediate(host); if(Directory.Exists(root)) Directory.Delete(root,true); }
        }
    }
}
