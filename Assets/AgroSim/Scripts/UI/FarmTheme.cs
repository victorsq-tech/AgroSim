using UnityEngine;

namespace AgroSim.UI
{
    // Shared presentation values; no gameplay or persistence rules live here.
    public static class FarmTheme
    {
        public static readonly Color Background = new Color(.055f, .09f, .085f);
        public static readonly Color Card = new Color(.095f, .15f, .14f);
        public static readonly Color Green = new Color(.27f, .47f, .29f);
        public static readonly Color Muted = new Color(.72f, .8f, .73f);
        public static readonly Color Ink = new Color(.94f, .93f, .85f);
        public static readonly Color Accent = new Color(.86f, .7f, .36f);
        public static readonly Color Line = new Color(.38f, .46f, .36f);

        public static void StyleButton(UnityEngine.UI.Button button)
        {
            button.targetGraphic = button.GetComponent<UnityEngine.UI.Image>();
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.16f, 1.16f, 1.08f);
            colors.pressedColor = new Color(.73f, .8f, .72f);
            colors.selectedColor = new Color(1.12f, 1.1f, .92f);
            colors.disabledColor = new Color(.55f, .6f, .55f, .7f);
            colors.fadeDuration = .12f;
            button.colors = colors;
            Border(button.gameObject, new Color(.53f, .65f, .47f, .55f));
        }

        public static void Border(GameObject go, Color color)
        {
            var outline = go.GetComponent<UnityEngine.UI.Outline>() ?? go.AddComponent<UnityEngine.UI.Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(1, -1);
            outline.useGraphicAlpha = false;
        }
    }
}
