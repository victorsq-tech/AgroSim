using System.IO;
using AgroSim.Core;
using AgroSim.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AgroSim.Editor
{
    [InitializeOnLoad]
    public static class AgroSimPlaceholderSetup
    {
        static AgroSimPlaceholderSetup() { EditorApplication.delayCall += Build; }
        private static void Build()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) { EditorApplication.delayCall += Build; return; }
            const string path = "Assets/AgroSim/Scenes/Gameplay.unity";
            var scene = SceneManager.GetSceneByPath(path);
            if (!scene.isLoaded) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            var template = SceneManager.GetSceneByPath("Assets/Scenes/SampleScene.unity");
            if (template.isLoaded && !template.isDirty) EditorSceneManager.CloseScene(template, true);
            foreach (var root in scene.GetRootGameObjects()) if (root.name == "VS01_Placeholders") return;
            // Preserve user work in a separate copy before adding the prototype.
            if (scene.isDirty) { Debug.LogWarning("Save Gameplay before building AgroSim placeholders."); return; }
            AssetDatabase.CopyAsset(path,"Assets/AgroSim/Scenes/Gameplay_FoundationBackup.unity");
            SceneManager.SetActiveScene(scene);
            var rootObject = new GameObject("VS01_Placeholders");
            const string spritePath = "Assets/AgroSim/Sprites/UI/PlaceholderSquare.png";
            if (!File.Exists(spritePath))
            {
                var texture = new Texture2D(16,16); var pixels = new Color[256];
                for(int i=0;i<pixels.Length;i++) pixels[i]=Color.white;
                texture.SetPixels(pixels); texture.Apply(); File.WriteAllBytes(spritePath,texture.EncodeToPNG());
                Object.DestroyImmediate(texture); AssetDatabase.ImportAsset(spritePath);
                var importer=(TextureImporter)AssetImporter.GetAtPath(spritePath);
                importer.textureType=TextureImporterType.Sprite; importer.spritePixelsPerUnit=16; importer.filterMode=FilterMode.Point;
                var settings=new TextureImporterSettings(); importer.ReadTextureSettings(settings); settings.spriteMeshType=SpriteMeshType.FullRect;
                importer.SetTextureSettings(settings); importer.SaveAndReimport();
            }
            var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            Shape(rootObject.transform,sprite,"FarmGround",new Vector2(0,0),new Vector2(22,13),new Color(.32f,.48f,.25f),-10);
            Shape(rootObject.transform,sprite,"Road",new Vector2(-2.7f,-.5f),new Vector2(1.1f,8),new Color(.65f,.57f,.4f),-5);
            Shape(rootObject.transform,sprite,"Garage",new Vector2(-5,1.2f),new Vector2(3.1f,2.3f),new Color(.65f,.68f,.63f),-2);
            Shape(rootObject.transform,sprite,"GarageRoof",new Vector2(-5,2.2f),new Vector2(3.5f,.5f),new Color(.38f,.2f,.16f),-1);
            Shape(rootObject.transform,sprite,"Field01",new Vector2(3,.1f),new Vector2(7,5.2f),new Color(.43f,.29f,.15f),-4);
            for(int i=0;i<8;i++) Shape(rootObject.transform,sprite,"SoilRow"+i,new Vector2(3,-2.1f+i*.6f),new Vector2(6.6f,.08f),new Color(.57f,.39f,.21f),-3);
            var tractor=Shape(rootObject.transform,sprite,"TR-01",new Vector2(-5,-1),new Vector2(.9f,.55f),new Color(.9f,.72f,.14f),5);
            Shape(tractor.transform,sprite,"Cabin",new Vector2(.12f,0),new Vector2(.32f,.4f),new Color(.35f,.7f,.8f),6);
            for(int i=0;i<4;i++) Shape(tractor.transform,sprite,"Wheel"+i,new Vector2(i<2 ? -.29f:.29f,i%2==0 ? -.3f:.3f),new Vector2(.23f,.19f),new Color(.1f,.12f,.1f),7);
            var implement=Shape(tractor.transform,sprite,"GradeAradora",new Vector2(-.8f,0),new Vector2(.45f,.75f),new Color(.65f,.2f,.17f),4);
            PrefabUtility.SaveAsPrefabAsset(implement,"Assets/AgroSim/Prefabs/Implements/GradeAradoraPlaceholder.prefab");
            PrefabUtility.SaveAsPrefabAsset(tractor,"Assets/AgroSim/Prefabs/Tractors/TR01Placeholder.prefab");
            var runner=rootObject.AddComponent<PlaceholderSlice>(); runner.tractor=tractor.transform;
            foreach(var obj in scene.GetRootGameObjects()) { var gm=obj.GetComponent<GameManager>(); if(gm!=null) runner.manager=gm; }
            if(runner.manager==null) runner.manager=new GameObject("GameManager").AddComponent<GameManager>();
            foreach(var obj in scene.GetRootGameObjects()) { var camera=obj.GetComponent<Camera>(); if(camera!=null) camera.orthographicSize=6f; }
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Debug.Log("AgroSim placeholders ready: scene saved, tractor and implement prefabs created.");
        }
        private static GameObject Shape(Transform parent,Sprite sprite,string name,Vector2 position,Vector2 size,Color color,int order)
        {
            var go=new GameObject(name); go.transform.SetParent(parent,false); go.transform.localPosition=position;
            // Dimensions belong to the renderer, keeping children independent of parent scale.
            var renderer=go.AddComponent<SpriteRenderer>(); renderer.sprite=sprite; renderer.drawMode=SpriteDrawMode.Sliced;
            renderer.size=size; renderer.color=color; renderer.sortingOrder=order; return go;
        }
    }
}
