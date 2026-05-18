using System.Collections.Generic;
using UnityEngine;

namespace MMBGame
{
    public static class RuntimeUiFactory
    {
        private static Sprite pixelSprite;

        public static Sprite GetPixelSprite()
        {
            if (pixelSprite != null)
            {
                return pixelSprite;
            }

            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            pixelSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            return pixelSprite;
        }

        public static Vector3 GetScreenCenter()
        {
            if (Camera.main == null)
            {
                return Vector3.zero;
            }

            return Camera.main.ScreenToWorldPoint(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, -Camera.main.transform.position.z));
        }

        public static Vector3 GetScreenWorldSize(Vector3 fallback)
        {
            if (Camera.main == null)
            {
                return fallback;
            }

            Vector3 min = Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f, -Camera.main.transform.position.z));
            Vector3 max = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, -Camera.main.transform.position.z));
            return new Vector3(Mathf.Abs(max.x - min.x), Mathf.Abs(max.y - min.y), 1f);
        }

        public static TextMesh CreateLabel(string objectName, string text, Vector3 position, int sortOrder, float characterSize, List<GameObject> trackedObjects, Transform parent = null, int fontSize = 36)
        {
            GameObject labelObject = new GameObject(objectName);
            if (parent != null)
            {
                labelObject.transform.SetParent(parent, true);
            }

            labelObject.transform.position = position;

            TextMesh textMesh = labelObject.AddComponent<TextMesh>();
            textMesh.text = text;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.fontSize = fontSize;
            textMesh.characterSize = characterSize;
            textMesh.lineSpacing = 1f;
            textMesh.color = Color.white;
            TextMeshFontApplier.Apply(textMesh);

            MeshRenderer renderer = labelObject.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = sortOrder;
            }

            trackedObjects?.Add(labelObject);
            return textMesh;
        }
    }
}
