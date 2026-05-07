using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MMBGame
{
    public static class TextMeshFontApplier
    {
        private const string FONT_PATH = "Assets/Tangba14.ttf";
        private static Font cachedFont;

        public static void Apply(TextMesh textMesh)
        {
            if (textMesh == null)
            {
                return;
            }

            Font font = GetFont();
            if (font == null)
            {
                return;
            }

            textMesh.font = font;
            MeshRenderer renderer = textMesh.GetComponent<MeshRenderer>();
            if (renderer != null && font.material != null)
            {
                renderer.sharedMaterial = font.material;
            }
        }

        private static Font GetFont()
        {
            if (cachedFont != null)
            {
                return cachedFont;
            }

#if UNITY_EDITOR
            cachedFont = AssetDatabase.LoadAssetAtPath<Font>(FONT_PATH);
#endif
            return cachedFont;
        }
    }
}
