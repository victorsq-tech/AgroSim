using System;
using System.Collections.Generic;
using UnityEngine;

namespace AgroSim.UI
{
    // Full-rect copies support tiling; original package sprites/importers remain intact.
    public sealed class FarmArtwork : IDisposable
    {
        private readonly CozyFarmEnvironmentSettings settings = Resources.Load<CozyFarmEnvironmentSettings>("CozyFarmEnvironment");
        private readonly Dictionary<Sprite, Sprite> tiled = new Dictionary<Sprite, Sprite>();
        public Sprite Grass => settings?.grass;
        public Sprite Road => settings?.road;
        public Sprite Floor => settings?.floor;
        public Sprite House(int index) => PrefabSprite(settings?.buildings, index);
        public Sprite Tree(int index) => PrefabSprite(settings?.trees, index % 4);
        private static Sprite PrefabSprite(GameObject[] prefabs, int index)
        {
            if(prefabs == null || index < 0 || index >= prefabs.Length || prefabs[index] == null) return null;
            return prefabs[index].GetComponent<SpriteRenderer>()?.sprite;
        }
        public void Tile(GameObject go, Sprite source, float density = 1.2f)
        {
            if(source == null) return;
            var sprite=TileSprite(source);
            var image = go.GetComponent<UnityEngine.UI.Image>();
            image.sprite = sprite; image.color = Color.white;
            image.type = UnityEngine.UI.Image.Type.Tiled; image.pixelsPerUnitMultiplier = density;
        }
        public Sprite TileSprite(Sprite source)
        {
            if(source==null) return null;
            if(!tiled.TryGetValue(source, out var sprite))
            {
                sprite = Sprite.Create(source.texture, source.rect, new Vector2(.5f,.5f), source.pixelsPerUnit, 0, SpriteMeshType.FullRect);
                sprite.name = "AgroSim tiled " + source.name;
                tiled.Add(source, sprite);
            }
            return sprite;
        }
        public static void Picture(GameObject go, Sprite sprite)
        {
            if(sprite == null) return;
            var image = go.GetComponent<UnityEngine.UI.Image>();
            image.sprite = sprite; image.color = Color.white; image.preserveAspect = true;
        }
        public void Dispose()
        {
            foreach(var sprite in tiled.Values) UnityEngine.Object.Destroy(sprite);
            tiled.Clear();
        }
    }
}
