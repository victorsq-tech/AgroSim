using System.Linq;
using AgroSim.UI;
using UnityEditor;
using UnityEngine;
namespace AgroSim.Editor
{
    [InitializeOnLoad] public static class CozyFarmEnvironmentSetup
    {
        private const string Package="Assets/Skyden_Games/Cozy Farm";
        private const string SettingsPath="Assets/AgroSim/Resources/CozyFarmEnvironment.asset";
        static CozyFarmEnvironmentSetup() { EditorApplication.delayCall+=Configure; }
        [MenuItem("AgroSim/Configurar ambiente Cozy Farm")]
        public static void Configure()
        {
            if(EditorApplication.isCompiling || EditorApplication.isUpdating) { EditorApplication.delayCall+=Configure; return; }
            if(EditorApplication.isPlayingOrWillChangePlaymode) return;
            var grass=AssetDatabase.LoadAllAssetsAtPath(Package+"/Textures/Ground Tiles.png").OfType<Sprite>().FirstOrDefault(s=>s.name=="Ground Tiles_0");
            var trees=Enumerable.Range(0,5).Select(i=>AssetDatabase.LoadAssetAtPath<GameObject>(Package+"/Prefabs/Props/Tree_"+i+".prefab")).ToArray();
            if(grass==null || trees.Any(t=>t==null || t.GetComponent<SpriteRenderer>()?.sprite==null))
            { Debug.LogWarning("AgroSim: Cozy Farm grass/tree references unavailable; environment was not configured."); return; }
            var settings=AssetDatabase.LoadAssetAtPath<CozyFarmEnvironmentSettings>(SettingsPath);
            if(settings==null)
            {
                if(AssetDatabase.LoadMainAssetAtPath(SettingsPath)!=null) { Debug.LogWarning("AgroSim: existing environment asset preserved."); return; }
                if(!AssetDatabase.IsValidFolder("Assets/AgroSim/Resources")) AssetDatabase.CreateFolder("Assets/AgroSim","Resources");
                settings=ScriptableObject.CreateInstance<CozyFarmEnvironmentSettings>(); settings.grass=grass; settings.trees=trees;
                AssetDatabase.CreateAsset(settings,SettingsPath); AssetDatabase.SaveAssets();
            }
            if(settings.grass!=null && settings.trees!=null && settings.trees.Length==5 && settings.trees.All(t=>t!=null))
                Debug.Log("AgroSim Cozy Farm references ready: grass and five tree prefabs. Source assets preserved.");
        }
    }
}
