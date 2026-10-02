using System;
using AgroSim.Operations;
using UnityEditor;
using UnityEngine;

namespace AgroSim.Editor
{
    [InitializeOnLoad]
    public static class GarageMaintenanceValidation
    {
        static GarageMaintenanceValidation() { EditorApplication.delayCall += Validate; }
        [MenuItem("AgroSim/Validar manutenção do galpão")]
        public static void Validate()
        {
            if(EditorApplication.isCompiling || EditorApplication.isUpdating) { EditorApplication.delayCall+=Validate; return; }
            if(EditorApplication.isPlayingOrWillChangePlaymode) return;
            var host=new GameObject("Maintenance validation"); host.hideFlags=HideFlags.HideAndDontSave;
            try
            {
                var op=host.AddComponent<PlaceholderOperation>(); op.Begin(); op.Tick(5f);
                float fuel=op.Fuel, wear=op.Wear; op.Begin();
                Require(Mathf.Approximately(op.Fuel,fuel) && Mathf.Approximately(op.Wear,wear),"Starting another operation must preserve fuel and wear.");
                op.Reset(); Require(Mathf.Approximately(op.Fuel,fuel) && Mathf.Approximately(op.Wear,wear),"Returning to garage must preserve machine condition.");
                typeof(PlaceholderOperation).GetMethod("Refuel").Invoke(op,null);
                Require(op.Fuel==100f && Mathf.Approximately(op.Wear,wear),"Refuel must only refill fuel.");
                typeof(PlaceholderOperation).GetMethod("Repair").Invoke(op,null);
                Require(op.Wear==0f && op.Fuel==100f,"Repair must remove wear.");
                typeof(PlaceholderOperation).GetMethod("RestoreMachine").Invoke(op,new object[]{.1f,99.9f});
                op.Begin(); op.Tick(5f);
                Require(!op.Running && !op.EventPending && op.Progress<1f,"Low fuel must interrupt the operation.");
                typeof(PlaceholderOperation).GetMethod("RestoreMachine").Invoke(op,new object[]{100f,100f});
                op.Begin(); Require(!op.Running,"A worn-out tractor must not begin.");
                Debug.Log("AgroSim maintenance validation PASSED: persistence between operations, refuel, repair, exhaustion and start guard.");
            }
            catch(Exception error) { Debug.LogError("AgroSim maintenance validation FAILED: "+error.Message); }
            finally { UnityEngine.Object.DestroyImmediate(host); }
        }
        private static void Require(bool value,string message) { if(!value)throw new InvalidOperationException(message); }
    }
}
