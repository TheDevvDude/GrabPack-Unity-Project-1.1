using UnityEngine;
using UnityEngine.EventSystems;

public class MobileJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public RectTransform background;
    public RectTransform handle;
    public MobileMoveInput moveInput;
    public float handleRange = 75f;

    private Canvas canvas;
    private Camera eventCamera;
    private int activePointerId = int.MinValue;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            eventCamera = canvas.worldCamera;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (activePointerId != int.MinValue)
        {
            return;
        }

        activePointerId = eventData.pointerId;
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerId != activePointerId)
        {
            return;
        }

        if (background == null || handle == null || moveInput == null)
        {
            return;
        }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position, eventCamera, out Vector2 localPoint))
        {
            Vector2 normalized = localPoint / handleRange;
            normalized = Vector2.ClampMagnitude(normalized, 1f);

            handle.anchoredPosition = normalized * handleRange;
            moveInput.SetInput(normalized);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId != activePointerId)
        {
            return;
        }

        activePointerId = int.MinValue;

        if (handle != null)
        {
            handle.anchoredPosition = Vector2.zero;
        }

        if (moveInput != null)
        {
            moveInput.ResetInput();
        }
    }
}
