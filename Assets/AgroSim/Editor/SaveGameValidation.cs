using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AgroSim.Editor
{
    [InitializeOnLoad]
    public static class SaveGameValidation
    {
        static SaveGameValidation() { EditorApplication.delayCall+=Validate; }
        [MenuItem("AgroSim/Validar save e tentativas")]
        public static void Validate()
        {
            if(EditorApplication.isCompiling || EditorApplication.isUpdating) { EditorApplication.delayCall+=Validate; return; }
            if(EditorApplication.isPlayingOrWillChangePlaymode) return;
            var directory=Path.Combine(Path.GetTempPath(),"AgroSim-save-tests-"+Guid.NewGuid().ToString("N"));
            try
            {
                var type=Type.GetType("AgroSim.Data.SaveGameStore, Assembly-CSharp");
                Require(type!=null,"SaveGameStore is missing.");
                Func<object> open=()=>Activator.CreateInstance(type,new object[]{directory});
                var store=open();
                Func<object,string,object[],object> call=(target,name,args)=>type.GetMethod(name).Invoke(target,args);
                Func<object,string,object> read=(target,name)=>type.GetProperty(name).GetValue(target);
                Require((bool)call(store,"SaveCheckpoint",new object[]{80.5f,8.6f}),"Initial checkpoint must save.");
                string original=(string)read(store,"CheckpointId");
                string savedAt=(string)read(store,"SavedAtUtc");
                Require(DateTimeOffset.TryParse(savedAt,out _),"Checkpoint must record date and time.");
                string id=(string)call(store,"BeginAttempt",Array.Empty<object>());
                Require(!string.IsNullOrEmpty(id),"Attempt must have a unique ID.");
                var resumed=open();
                Require((float)read(resumed,"Fuel")==80.5f && (float)read(resumed,"Wear")==8.6f,"Restart must restore checkpoint, not attempt state.");
                Require((string)read(resumed,"PendingAttemptId")==id,"Interrupted attempt must be detectable.");
                Require((string)read(resumed,"SavedAtUtc")==savedAt,"Attempt metadata must preserve checkpoint date.");
                Require((bool)call(resumed,"RecoverAttempt",Array.Empty<object>()),"Recovery must close interrupted attempt.");
                Require((string)read(open(),"PendingAttemptId")==null,"Recovery must persist.");
                Require((string)read(open(),"SavedAtUtc")==savedAt,"Recovery must preserve checkpoint date.");
                string recoveredLog=File.ReadAllText(Path.Combine(directory,id+".jsonl"));
                Require(recoveredLog.Contains("interrupted") && recoveredLog.Contains(original),"Recovery must retain origin and terminal status.");
                string second=(string)call(resumed,"BeginAttempt",Array.Empty<object>());
                Require(second!=id,"Repeated operation must have a different ID.");
                Require((bool)call(resumed,"CompleteAttempt",new object[]{second}),"Completion metadata must save.");
                Require((string)read(open(),"PendingAttemptStatus")=="completed","Completed without garage confirmation must remain pending.");
                Require((bool)call(resumed,"ConfirmAttempt",new object[]{second,61f,17.2f}),"Garage confirmation must save condition.");
                string confirmed=(string)read(resumed,"CheckpointId");
                Require(confirmed!=original,"Confirmation must create a new checkpoint.");
                Require((bool)call(resumed,"ConfirmAttempt",new object[]{second,0f,100f}),"Duplicate confirmation must be harmless.");
                Require((float)read(resumed,"Fuel")==61f && (string)read(resumed,"CheckpointId")==confirmed,"Duplicate must not apply state twice.");
                Require(!(bool)call(resumed,"SaveCheckpoint",new object[]{float.NaN,0f}),"Non-finite data must be rejected.");
                Require((float)read(open(),"Fuel")==61f,"Rejected write must preserve previous save.");
                Require((bool)call(resumed,"SaveCheckpoint",new object[]{100f,17.2f}),"Refuel checkpoint must save.");
                Require((float)read(open(),"Wear")==17.2f,"Refuel must preserve wear.");
                Require((bool)call(resumed,"SaveCheckpoint",new object[]{100f,0f}),"Repair checkpoint must save.");
                string path=(string)read(resumed,"FilePath");
                File.WriteAllText(path,"{broken"); var corrupted=open();
                Require((float)read(corrupted,"Fuel")==100f && Directory.GetFiles(directory,"*.invalid-*").Length==1,"Corrupt save must be archived and restore safe defaults.");
                Require((bool)call(corrupted,"NewGame",Array.Empty<object>()),"New game must create a checkpoint.");
                Require((float)read(open(),"Fuel")==100f && (float)read(open(),"Wear")==0f,"New game must restore defaults.");
                var final=open(); string unfinished=(string)call(final,"BeginAttempt",Array.Empty<object>());
                call(final,"CompleteAttempt",new object[]{unfinished});
                var completed=open(); Require((bool)call(completed,"RecoverAttempt",Array.Empty<object>()),"Unconfirmed completion must recover.");
                Require(File.ReadAllText(Path.Combine(directory,unfinished+".jsonl")).Contains("completed_unconfirmed"),"Recovery must distinguish completed from interrupted.");
                Require((bool)call(completed,"RecoverAttempt",Array.Empty<object>()),"Recovery must be idempotent.");
                string before=File.ReadAllText(path);
                string beforeDate=(string)read(completed,"SavedAtUtc");
                if(File.Exists(path+".bak")) File.Delete(path+".bak");
                Directory.CreateDirectory(path+".bak");
                Require(!(bool)call(completed,"SaveCheckpoint",new object[]{50f,20f}),"Failed replacement must report failure.");
                Require(File.ReadAllText(path)==before && (float)read(completed,"Fuel")==100f,"Failed replacement must preserve disk and memory.");
                Require((string)read(completed,"SavedAtUtc")==beforeDate,"Failed write must preserve last save date.");
                Directory.Delete(path+".bak");
                File.WriteAllText(path,before.Replace("\"version\": 2","\"version\": 99"));
                var unknown=open(); Require((bool)read(unknown,"NeedsInitialSave"),"Unknown version must not be accepted.");
                // Construct an explicit invalid value independent of JSON float formatting.
                string outOfRange=System.Text.RegularExpressions.Regex.Replace(before,"\"fuelLiters\"\\s*:\\s*[0-9.]+","\"fuelLiters\": 101");
                File.WriteAllText(path,outOfRange);
                Require((bool)read(open(),"NeedsInitialSave"),"Out-of-range save must be rejected.");
                string missing=System.Text.RegularExpressions.Regex.Replace(before,"\"fuelLiters\"\\s*:\\s*[0-9.]+,?","");
                File.WriteAllText(path,missing);
                Require((bool)read(open(),"NeedsInitialSave"),"Missing machine state must be rejected.");
                Debug.Log("AgroSim save validation PASSED: checkpoint, attempts, recovery, completion, idempotency, invalid data, maintenance and new game.");
            }
            catch(Exception error) { Debug.LogError("AgroSim save validation FAILED: "+error.GetBaseException().Message); }
            finally { if(Directory.Exists(directory)) Directory.Delete(directory,true); }
        }
        private static void Require(bool value,string message) { if(!value)throw new InvalidOperationException(message); }
    }
}
