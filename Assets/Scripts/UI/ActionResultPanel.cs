using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MMBGame
{
    public class ActionResultPanel : MonoBehaviour
    {
        private static ActionResultPanel instance;

        private const int SORT_BACKDROP = 1999;
        private const int SORT_BG = 2000;
        private const int SORT_BUTTON = 2002;
        private const int SORT_TEXT = 2003;
        private const float PANEL_W = 11.8f;
        private const float PANEL_H = 5.8f;
        private const float BUTTON_W = 2.35f;
        private const float BUTTON_H = 0.82f;

        private readonly List<GameObject> objects = new List<GameObject>();
        private GameObject panelRoot;
        private BoxCollider2D confirmCollider;
        private Action onConfirm;

        public static bool IsOpen { get; private set; }

        public static void Show(string title, string body, Action confirmCallback)
        {
            if (instance == null)
            {
                instance = new GameObject("ActionResultPanel").AddComponent<ActionResultPanel>();
            }

            instance.Open(title, body, confirmCallback);
        }

        private void Open(string title, string body, Action confirmCallback)
        {
            Close();
            DestroyExistingPanelObjects();
            IsOpen = true;
            onConfirm = confirmCallback;

            Vector3 center = RuntimeUiFactory.GetScreenCenter();
            CreateRoot(center);
            CreateBackdrop(center);
            CreateBackground(center);
            CreateLabel(title, new Vector3(center.x, center.y + 1.75f, center.z - 0.3f), SORT_TEXT, 0.16f);
            CreateLabel(body, new Vector3(center.x, center.y + 0.1f, center.z - 0.3f), SORT_TEXT, 0.08f);
            CreateConfirmButton(center);
        }

        private void Update()
        {
            Mouse mouse = Mouse.current;
            if (!IsOpen || mouse == null || !mouse.leftButton.wasPressedThisFrame || Camera.main == null)
            {
                return;
            }

            Vector2 screenPosition = mouse.position.ReadValue();
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -Camera.main.transform.position.z));
            if (confirmCollider != null && confirmCollider.OverlapPoint(worldPosition))
            {
                Action callback = onConfirm;
                Close();
                callback?.Invoke();
            }
        }

        private void Close()
        {
            IsOpen = false;
            onConfirm = null;
            confirmCollider = null;
            if (panelRoot != null)
            {
                Destroy(panelRoot);
                panelRoot = null;
            }

            for (int i = objects.Count - 1; i >= 0; i--)
            {
                if (objects[i] != null)
                {
                    Destroy(objects[i]);
                }
            }

            objects.Clear();
        }

        private void CreateRoot(Vector3 center)
        {
            panelRoot = new GameObject("ActionResultPanelRoot");
            panelRoot.transform.position = center;
        }

        private void CreateBackdrop(Vector3 center)
        {
            GameObject backdropObject = new GameObject("ActionResultBackdrop");
            backdropObject.transform.SetParent(panelRoot.transform, true);
            backdropObject.transform.position = new Vector3(center.x, center.y, center.z - 0.15f);
            backdropObject.transform.localScale = RuntimeUiFactory.GetScreenWorldSize(new Vector3(20f, 12f, 1f));

            SpriteRenderer renderer = backdropObject.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeUiFactory.GetPixelSprite();
            renderer.color = new Color(0f, 0f, 0f, 0.86f);
            renderer.sortingOrder = SORT_BACKDROP;
            objects.Add(backdropObject);
        }

        private void CreateBackground(Vector3 center)
        {
            GameObject backgroundObject = new GameObject("ActionResultBG");
            backgroundObject.transform.SetParent(panelRoot.transform, true);
            backgroundObject.transform.position = new Vector3(center.x, center.y, center.z - 0.2f);
            backgroundObject.transform.localScale = new Vector3(PANEL_W, PANEL_H, 1f);
            SpriteRenderer renderer = backgroundObject.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeUiFactory.GetPixelSprite();
            renderer.color = new Color(0.12f, 0.09f, 0.07f, 0.95f);
            renderer.sortingOrder = SORT_BG;
            objects.Add(backgroundObject);
        }

        private void CreateConfirmButton(Vector3 center)
        {
            GameObject buttonObject = new GameObject("ConfirmButton");
            buttonObject.transform.SetParent(panelRoot.transform, true);
            buttonObject.transform.position = new Vector3(center.x, center.y - 1.85f, center.z - 0.3f);
            buttonObject.transform.localScale = new Vector3(BUTTON_W, BUTTON_H, 1f);

            SpriteRenderer renderer = buttonObject.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeUiFactory.GetPixelSprite();
            renderer.color = new Color(0.28f, 0.36f, 0.32f, 1f);
            renderer.sortingOrder = SORT_BUTTON;

            confirmCollider = buttonObject.AddComponent<BoxCollider2D>();
            confirmCollider.size = Vector2.one;
            objects.Add(buttonObject);

            CreateLabel("확인", new Vector3(center.x, center.y - 1.82f, center.z - 0.4f), SORT_TEXT, 0.08f);
        }

        private void CreateLabel(string text, Vector3 position, int sortOrder, float characterSize)
        {
            RuntimeUiFactory.CreateLabel("ActionResultLabel", text, position, sortOrder, characterSize, objects, panelRoot.transform, 38);
        }

        private void DestroyExistingPanelObjects()
        {
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            for (int i = 0; i < allObjects.Length; i++)
            {
                GameObject target = allObjects[i];
                if (target == null)
                {
                    continue;
                }

                if (target.name == "ActionResultPanelRoot" ||
                    target.name == "ActionResultBackdrop" ||
                    target.name == "ActionResultBG" ||
                    target.name == "ConfirmButton" ||
                    target.name == "ActionResultLabel")
                {
                    Destroy(target);
                }
            }
        }

    }
}

