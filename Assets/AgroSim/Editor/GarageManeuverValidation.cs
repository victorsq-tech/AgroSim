using System;
using UnityEditor;
using UnityEngine;
namespace AgroSim.Editor
{
    [InitializeOnLoad] public static class GarageManeuverValidation
    {
        static GarageManeuverValidation() { EditorApplication.delayCall+=Validate; }
        [MenuItem("AgroSim/Validar manobra do galpão")] public static void Validate()
        {
            if(EditorApplication.isCompiling || EditorApplication.isUpdating) { EditorApplication.delayCall+=Validate; return; }
            try {
                var t=Type.GetType("AgroSim.Core.GarageManeuver, Assembly-CSharp");
                Check(t!=null,"Garage maneuver missing."); var m=Activator.CreateInstance(t);
                Func<string,bool> flag=n=>(bool)t.GetProperty(n).GetValue(m);
                Func<bool,bool> request=attach=>(bool)t.GetMethod("Request").Invoke(m,new object[]{attach});
                Action<float> tick=dt=>t.GetMethod("Tick").Invoke(m,new object[]{dt});
                Check(!flag("Ready")&&!flag("Attached"),"Parked tractor must not depart.");
                Check(request(true)&&flag("Busy")&&!flag("Ready"),"Attachment must start a maneuver.");
                Check(!request(false),"Conflicting request accepted during maneuver.");
                tick(2); Check(flag("Attached")&&!flag("Ready"),"Hitch must attach before reaching door.");
                tick(2); Check(flag("Ready")&&!flag("Busy"),"Door must release departure.");
                Check(request(false)&&!flag("Ready"),"Detachment must lock departure.");
                tick(2); Check(!flag("Attached")&&flag("Busy"),"Tool must be stored before parking.");
                tick(2); Check(!flag("Busy")&&!flag("Ready"),"Tractor must park.");
                request(true); tick(1); t.GetMethod("Reset").Invoke(m,null);
                Check(!flag("Busy")&&!flag("Attached")&&!flag("Ready"),"Recovery must restore stored equipment.");
                Debug.Log("AgroSim maneuver validation PASSED: attach, door guard, conflicting inputs, detach, parking and recovery.");
            } catch(Exception e) { Debug.LogError("AgroSim maneuver validation FAILED: "+(e.InnerException??e).Message); }
        }
        static void Check(bool value,string message) { if(!value) throw new Exception(message); }
    }
}
