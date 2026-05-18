using UnityEngine;
using UnityEngine.InputSystem;

namespace MMBGame
{
    public partial class CommandActionButton
    {
        private void UpdatePointerState()
        {
            if (ActionResultPanel.IsOpen)
            {
                return;
            }

            if (visualFeedback == null || visualFeedback.Collider == null || Camera.main == null)
            {
                return;
            }

            Mouse mouse = Mouse.current;
            if (mouse == null)
            {
                return;
            }

            Vector2 screenPosition = mouse.position.ReadValue();
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -Camera.main.transform.position.z));
            bool containsPointer = visualFeedback.Collider.OverlapPoint(worldPosition);

            if (containsPointer != isHovered)
            {
                isHovered = containsPointer;
                visualFeedback.SetHovered(isHovered);
                if (isHovered)
                {
                    CommandActionTooltip.Show(actionName, transform);
                }
            }

            if (containsPointer && mouse.leftButton.wasPressedThisFrame)
            {
                SetPressed(true);
                CommandActionTooltip.Hide();
                HandleClick();
            }

            if (isPressed && !mouse.leftButton.isPressed)
            {
                SetPressed(false);
            }
        }

        private void SetPressed(bool pressed)
        {
            if (isPressed == pressed)
            {
                return;
            }

            isPressed = pressed;
            if (visualFeedback != null)
            {
                visualFeedback.SetPressed(pressed);
            }
        }
    }
}
