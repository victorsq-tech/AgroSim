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
            public string operationId, timestampUtc, kind, tractorId = "TR-01", fieldId = "FIELD-01", decision;
            public float progress, fuelLiters, wearPercent, productivityPercent;
        }
        private string operationId;
        public string FilePath { get; private set; }
        public string LastError { get; private set; }
        public void Begin()
        {
            operationId = Guid.NewGuid().ToString("N");
            FilePath = Path.Combine(Application.persistentDataPath, "AgroSim", operationId + ".jsonl");
            LastError = null;
        }
        public void Write(string kind, float progress, float fuel, float wear, float productivity, string decision = null)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                var record = new Record { operationId = operationId, timestampUtc = DateTime.UtcNow.ToString("o"),
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
    }
}
