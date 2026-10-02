using System;
using AgroSim.Operations;
using UnityEditor;
using UnityEngine;

namespace AgroSim.Editor
{
    [InitializeOnLoad]
    public static class AgroSimOperationValidation
    {
        static AgroSimOperationValidation() { EditorApplication.delayCall += Validate; }
        [MenuItem("AgroSim/Validar ciclo VS01")]
        public static void Validate()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) { EditorApplication.delayCall += Validate; return; }
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var host = new GameObject("VS01 validation temporary");
            host.hideFlags = HideFlags.HideAndDontSave;
            try
            {
                var operation = host.AddComponent<PlaceholderOperation>();
                operation.Begin(); operation.Tick(20f);
                Require(operation.EventPending && Mathf.Approximately(operation.Progress,.45f),"Event must pause progress at 45%.");
                float fuel = operation.Fuel; operation.Tick(10f);
                Require(Mathf.Approximately(operation.Fuel,fuel),"Paused event must not consume fuel.");
                operation.ResolveEvent(true); operation.Tick(20f);
                Require(!operation.Running && operation.Progress==1f,"Adjusted operation must finish.");
                Require(Mathf.Abs(operation.Fuel-77.5f)<.01f && operation.Productivity==100f,"Adjusted result is incorrect.");
                operation.Begin(); operation.Tick(20f); operation.ResolveEvent(false); operation.Tick(20f);
                Require(Mathf.Abs(operation.Fuel-80.5f)<.01f && operation.Productivity==85f && Mathf.Abs(operation.Wear-8.6f)<.01f,"Continue result is incorrect.");
                operation.Reset();
                Require(!operation.Running && !operation.EventPending && operation.Fuel==100f && operation.Progress==0f,"Restart must reset the operation.");
                Debug.Log("AgroSim VS01 validation PASSED: event pause, both decisions, completion and restart.");
            }
            finally { UnityEngine.Object.DestroyImmediate(host); }
        }
        private static void Require(bool value,string message) { if(!value) throw new InvalidOperationException(message); }
    }
}
