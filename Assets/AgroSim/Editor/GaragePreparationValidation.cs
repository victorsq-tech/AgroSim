using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AgroSim.Editor
{
    [InitializeOnLoad]
    public static class GaragePreparationValidation
    {
        static GaragePreparationValidation() { EditorApplication.delayCall += Validate; }
        [MenuItem("AgroSim/Validar preparação do galpão")]
        public static void Validate()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) { EditorApplication.delayCall += Validate; return; }
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            try
            {
                var type = Type.GetType("AgroSim.Core.GaragePreparation, Assembly-CSharp");
                Require(type != null, "Garage preparation rules are missing.");
                var preparation = Activator.CreateInstance(type);
                Func<string,bool> read = name => (bool)type.GetProperty(name).GetValue(preparation);
                Action<string> call = name => type.GetMethod(name).Invoke(preparation,null);
                Require(!read("CanStart"), "Empty garage must block start.");
                call("ToggleImplement");
                Require(!read("ImplementSelected"), "Cannot attach an implement without a tractor.");
                call("ToggleTractor");
                Require(read("TractorSelected") && !read("CanStart"), "Tractor alone must block start.");
                call("ToggleImplement"); Require(read("CanStart"), "Complete preparation must allow start.");
                call("ToggleImplement"); Require(!read("CanStart"), "Detaching must block start.");
                call("ToggleImplement"); call("ToggleTractor");
                Require(!read("TractorSelected") && !read("ImplementSelected") && !read("CanStart"), "Deselecting tractor must detach implement.");
                call("ToggleTractor"); Require(!read("CanStart"), "Reselecting tractor must not restore the implement.");
                call("ToggleImplement"); call("Reset");
                Require(!read("TractorSelected") && !read("ImplementSelected"), "Reset must clear selections.");
                Debug.Log("AgroSim garage validation PASSED: selection, deselection, attachment, detachment, prerequisites and reset.");
            }
            catch (Exception error) { Debug.LogError("AgroSim garage validation FAILED: " + error.Message); }
        }
        private static void Require(bool condition,string message) { if(!condition) throw new InvalidOperationException(message); }
    }
}
