using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class MobilePressEvents : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public UnityEvent onPress;
    public UnityEvent onRelease;

    private bool pressed;

    private void Awake()
    {
        if (onPress == null)
        {
            onPress = new UnityEvent();
        }

        if (onRelease == null)
        {
            onRelease = new UnityEvent();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pressed = true;
        Debug.Log($"[MobilePressEvents:{name}] PointerDown id={eventData.pointerId} position={eventData.position}");

        if (onPress != null)
        {
            onPress.Invoke();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log($"[MobilePressEvents:{name}] PointerUp id={eventData.pointerId} position={eventData.position}");
        Release();
    }

    private void Release()
    {
        if (!pressed)
        {
            Debug.Log($"[MobilePressEvents:{name}] Release ignored because pressed=false");
            return;
        }

        pressed = false;
        Debug.Log($"[MobilePressEvents:{name}] Release invoked");

        if (onRelease != null)
        {
            onRelease.Invoke();
        }
    }
}
