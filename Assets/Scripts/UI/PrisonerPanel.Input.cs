using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MMBGame
{
    public partial class PrisonerPanel
    {
        private void Update()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.wasPressedThisFrame || Camera.main == null)
            {
                return;
            }

            Vector2 sp = mouse.position.ReadValue();
            Vector3 wp = Camera.main.ScreenToWorldPoint(
                new Vector3(sp.x, sp.y, -Camera.main.transform.position.z));

            for (int i = 0; i < buttons.Count; i++)
            {
                if (buttons[i].col != null && buttons[i].col.OverlapPoint(wp))
                {
                    Action cb = buttons[i].callback;
                    Close();
                    cb?.Invoke();
                    return;
                }
            }
        }
    }
}
