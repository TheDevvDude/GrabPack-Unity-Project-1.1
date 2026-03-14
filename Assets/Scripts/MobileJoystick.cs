using UnityEngine;
using UnityEngine.EventSystems;

public class MobileJoystick : MonoBehaviour,
    IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public RectTransform background;
    public RectTransform handle;

    [Range(0.5f, 2f)]
    public float handleLimit = 1f;

    private Vector2 inputVector;

    public Vector2 GetInput()
    {
        return inputVector;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 position;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background,
            eventData.position,
            eventData.pressEventCamera,
            out position))
        {
            position.x /= background.sizeDelta.x;
            position.y /= background.sizeDelta.y;

            inputVector = new Vector2(position.x * 2, position.y * 2);

            inputVector = (inputVector.magnitude > 1)
                ? inputVector.normalized
                : inputVector;

            handle.anchoredPosition =
                inputVector * (background.sizeDelta.x / 2) * handleLimit;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputVector = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
    }
}