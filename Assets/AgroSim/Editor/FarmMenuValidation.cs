using System;
using UnityEditor;
using UnityEngine;

namespace AgroSim.Editor
{
    [InitializeOnLoad] public static class FarmMenuValidation
    {
        static FarmMenuValidation() { EditorApplication.delayCall+=Validate; }
        [MenuItem("AgroSim/Validar navegação dos menus")]
        public static void Validate()
        {
            if(EditorApplication.isCompiling || EditorApplication.isUpdating) { EditorApplication.delayCall+=Validate; return; }
            try
            {
                var type=Type.GetType("AgroSim.UI.FarmNavigation, Assembly-CSharp");
                Require(type!=null,"Farm navigation is missing.");
                var screen=Type.GetType("AgroSim.UI.FarmScreen, Assembly-CSharp");
                var navigation=Activator.CreateInstance(type);
                Func<string,object> value=name=>Enum.Parse(screen,name);
                Func<string,bool> open=name=>(bool)type.GetMethod("Open").Invoke(navigation,new[]{value(name)});
                Func<string> current=()=>type.GetProperty("Current").GetValue(navigation).ToString();
                Require(current()=="MainMenu","Must launch at the main menu.");
                Require(!open("Workshop"),"Main menu must not bypass the farm map.");
                Require(open("FarmMap"),"Continue must reach map.");
                foreach(string area in new[]{"Garage","Workshop","Fuel","Field"})
                    Require(open(area) && open("FarmMap"),"Round trip failed: "+area);
                Require(!open("Operation"),"Map must not start an operation without preparation.");
                Require(open("Garage"),"Garage should open.");
                type.GetMethod("BeginOperation").Invoke(navigation,null);
                Require(current()=="Operation" && !open("MainMenu") && !open("FarmMap"),"Menus must not mask an active operation.");
                type.GetMethod("ReturnToGarage").Invoke(navigation,null);
                Require(current()=="Garage" && open("FarmMap") && open("MainMenu"),"Return path failed.");
                Debug.Log("AgroSim menu validation PASSED: initial menu, four map areas, round trips, preparation guard and operation lock.");
            }
            catch(Exception e) { Debug.LogError("AgroSim menu validation FAILED: "+(e.InnerException??e).Message); }
        }
        private static void Require(bool condition,string message) { if(!condition) throw new Exception(message); }
    }
}
