using System;
using System.IO;
using UnityEngine;

namespace AgroSim.Data
{
    public sealed class LocalOperationLog
    {
        [Serializable]
        private sealed class Record
        {
            public string operationId, runId, originCheckpointId, eventId, timestampUtc, kind, tractorId = "TR-01", fieldId = "FIELD-01", decision;
            public float progress, fuelLiters, wearPercent, productivityPercent;
        }
        private string operationId;
        private string runId, checkpointId;
        private int sequence;
        public string OperationId => operationId;
        public string FilePath { get; private set; }
        public string LastError { get; private set; }
        public void Begin()
        {
            Begin(Guid.NewGuid().ToString("N"),null,null,Path.Combine(Application.persistentDataPath,"AgroSim"));
        }
        public void Begin(string id,string run,string checkpoint,string directory)
        {
            operationId=id; runId=run; checkpointId=checkpoint; sequence=0;
            FilePath = Path.Combine(directory, operationId + ".jsonl");
            LastError = null;
        }
        public void Write(string kind, float progress, float fuel, float wear, float productivity, string decision = null)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                var record = new Record { operationId = operationId, runId=runId, originCheckpointId=checkpointId, eventId=operationId+":"+(++sequence), timestampUtc = DateTime.UtcNow.ToString("o"),
                    kind = kind, progress = progress, fuelLiters = fuel, wearPercent = wear,
                    productivityPercent = productivity, decision = decision };
                File.AppendAllText(FilePath, JsonUtility.ToJson(record) + Environment.NewLine);
                LastError = null;
            }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException)
            {
                LastError = "Não foi possível salvar o registro local.";
                Debug.LogWarning(LastError + " " + error.Message);
            }
        }
        public static bool WriteRecovery(string directory,string id,string run,string checkpoint,string kind)
        {
            string path=Path.Combine(directory,id+".jsonl"), eventId=id+":recovery";
            try
            {
                Directory.CreateDirectory(directory);
                if(File.Exists(path)) foreach(string line in File.ReadLines(path))
                {
                    try { if(line.TrimEnd().EndsWith("}") && JsonUtility.FromJson<Record>(line)?.eventId==eventId) return true; }
                    catch(ArgumentException) {} // A crash may leave one incomplete telemetry line.
                }
                var record=new Record {operationId=id,runId=run,originCheckpointId=checkpoint,eventId=eventId,kind=kind,timestampUtc=DateTime.UtcNow.ToString("o")};
                // A leading newline keeps a partially written last sample separate from the recovery record.
                byte[] bytes=System.Text.Encoding.UTF8.GetBytes(Environment.NewLine+JsonUtility.ToJson(record)+Environment.NewLine);
                using(var stream=new FileStream(path,FileMode.Append,FileAccess.Write,FileShare.Read))
                { stream.Write(bytes,0,bytes.Length); stream.Flush(true); }
                return true;
            }
            catch(Exception error) when(error is IOException || error is UnauthorizedAccessException)
            { Debug.LogWarning("AgroSim recovery log: "+error.Message); return false; }
        }
    }
}
