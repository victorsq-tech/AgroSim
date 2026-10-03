using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using AgroSim.Operations;

namespace AgroSim.Editor
{
    [InitializeOnLoad] public static class FieldProgressValidation
    {
        static FieldProgressValidation() { EditorApplication.delayCall+=Validate; }
        [MenuItem("AgroSim/Validar cinco operações")]
        public static void Validate()
        {
            if(EditorApplication.isCompiling || EditorApplication.isUpdating) { EditorApplication.delayCall+=Validate; return; }
            string root=Path.Combine(Path.GetTempPath(),"AgroSim-field-"+Guid.NewGuid().ToString("N"));
            try
            {
                var type=Type.GetType("AgroSim.Data.SaveGameStore, Assembly-CSharp");
                Require(type.GetProperty("NextOperationIndex")!=null,"Field progression is missing.");
                Func<object> open=()=>Activator.CreateInstance(type,new object[]{root});
                Func<object,string,object[],object> call=(s,n,a)=>type.GetMethod(n).Invoke(s,a);
                Func<object,int> stage=s=>(int)type.GetProperty("NextOperationIndex").GetValue(s);
                var store=open(); call(store,"EnsureSaved",Array.Empty<object>());
                string partial=(string)call(store,"BeginAttempt",Array.Empty<object>());
                call(store,"ConfirmFieldAttempt",new object[]{partial,95f,1f,85f});
                Require(stage(store)==0 && (float)type.GetProperty("Productivity").GetValue(store)==100,"Incomplete work must not advance field or productivity.");
                // Completing away from the garage must not advance the saved field.
                string interrupted=(string)call(store,"BeginAttempt",Array.Empty<object>());
                call(store,"CompleteAttempt",new object[]{interrupted}); store=open();
                Require(stage(store)==0,"Unconfirmed work advanced the field.");
                call(store,"RecoverAttempt",Array.Empty<object>()); Require(stage(store)==0,"Recovery changed the field.");
                for(int i=0;i<5;i++)
                {
                    string recovery=(string)call(store,"BeginAttempt",Array.Empty<object>());
                    Require((bool)call(store,"RecoverAttempt",Array.Empty<object>()),"Recovery failed.");
                    string recoveryJson=File.ReadAllText(Path.Combine(root,recovery+".jsonl"));
                    Require(recoveryJson.Contains("\"operationIndex\":"+i) && recoveryJson.Contains("\"operationType\":\"") && !recoveryJson.Contains("\"operationType\":\"\""),"Recovery must identify the operation.");
                    string id=(string)call(store,"BeginAttempt",Array.Empty<object>());
                    Require(id!=null,"Expected unlocked operation.");
                    call(store,"CompleteAttempt",new object[]{id}); Require(stage(store)==i,"Completion advanced before return.");
                    string before=File.ReadAllText(Path.Combine(root,"save.json"));
                    string backup=Path.Combine(root,"save.json.bak"); if(File.Exists(backup)) File.Delete(backup); Directory.CreateDirectory(backup);
                    Require(!(bool)call(store,"ConfirmFieldAttempt",new object[]{id,80f,4f,85f}),"Blocked save must fail.");
                    Require(stage(store)==i && File.ReadAllText(Path.Combine(root,"save.json"))==before,"Failed confirmation advanced the field.");
                    Directory.Delete(backup);
                    Require((bool)call(store,"ConfirmFieldAttempt",new object[]{id,80f,4f,85f}),"Confirmation retry failed.");
                    Require(stage(store)==i+1,"Wrong next operation.");
                    call(store,"ConfirmFieldAttempt",new object[]{id,80f,4f,85f}); Require(stage(store)==i+1,"Duplicate advanced twice.");
                    store=open(); Require(stage(store)==i+1,"Stage did not persist.");
                    Require((float)type.GetProperty("Productivity").GetValue(store)==85f,"Productivity did not persist.");
                }
                Require(call(store,"BeginAttempt",Array.Empty<object>())==null,"Harvest must close the cycle.");
                string modern=File.ReadAllText(Path.Combine(root,"save.json"));
                string legacy=modern.Replace("\"version\": 3","\"version\": 2");
                legacy=System.Text.RegularExpressions.Regex.Replace(legacy,"\\s*\"nextOperationIndex\"\\s*:\\s*[0-9]+,","");
                legacy=System.Text.RegularExpressions.Regex.Replace(legacy,",\\s*\"productivityPercent\"\\s*:\\s*[0-9.]+","");
                File.WriteAllText(Path.Combine(root,"save.json"),legacy); var migrated=open();
                Require(stage(migrated)==0 && (float)type.GetProperty("Productivity").GetValue(migrated)==100,"Legacy field must start at preparation.");
                foreach(string property in new[]{"Fuel","Wear","BalanceCents","SavedAtUtc","RunId","CheckpointId"})
                    Require(Equals(type.GetProperty(property).GetValue(migrated),type.GetProperty(property).GetValue(store)),"Migration changed "+property);
                call(migrated,"EnsureSaved",Array.Empty<object>());
                call(store,"NewGame",Array.Empty<object>()); Require(stage(store)==0,"New game did not reset field.");
                var visual=Type.GetType("AgroSim.Operations.FieldWorkVisual, Assembly-CSharp");
                var count=visual.GetMethod("WorkedCellCount");
                Require((int)count.Invoke(null,new object[]{0f})==0 && (int)count.Invoke(null,new object[]{1f})==96,"Wrong coverage endpoints.");
                Require((int)count.Invoke(null,new object[]{.45f})==43,"Event coverage is inaccurate.");
                Require(FieldWorkVisual.IsCellWorked(0,0,.02f) && !FieldWorkVisual.IsCellWorked(0,23,.02f),"First pass must move left to right.");
                Require(FieldWorkVisual.IsCellWorked(1,23,.27f) && !FieldWorkVisual.IsCellWorked(1,0,.27f),"Second pass must move right to left.");
                var host=new GameObject("Five operation simulation"); host.hideFlags=HideFlags.HideAndDontSave;
                try
                {
                    var operation=host.AddComponent<PlaceholderOperation>(); float productivity=100;
                    for(int i=0;i<OperationCatalog.Count;i++)
                    {
                        operation.Refuel(); operation.Repair(); operation.Configure(productivity); operation.Begin();
                        operation.Tick(30); Require(operation.EventPending && operation.Progress==.45f,"Event did not pause "+OperationCatalog.Get(i).Name);
                        operation.ResolveEvent(i!=1); operation.Tick(30);
                        Require(!operation.Running && operation.Progress==1,"Operation failed to finish.");
                        productivity=operation.Productivity;
                        Require(productivity==(i>=1 ? 85 : 100),"Decisions did not carry between operations.");
                    }
                }
                finally { UnityEngine.Object.DestroyImmediate(host); }
                Debug.Log("AgroSim field validation PASSED: five stages, unconfirmed recovery, persistent productivity, duplicate confirmation, final lock, reset and coverage.");
            }
            catch(Exception e) { Debug.LogError("AgroSim field validation FAILED: "+(e.InnerException??e).Message); }
            finally { if(Directory.Exists(root)) Directory.Delete(root,true); }
        }
        private static void Require(bool condition,string message) { if(!condition) throw new Exception(message); }
    }
}
