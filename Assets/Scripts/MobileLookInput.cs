using UnityEngine;
using UnityEngine.EventSystems;

public class MobileLookInput : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public RigidboyPlayerController player;
    public LaunchHand[] hands;
    public float lookMultiplier = 0.02f;

    private int activePointerId = int.MinValue;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (activePointerId != int.MinValue)
        {
            return;
        }

        activePointerId = eventData.pointerId;
        UpdateAimPosition(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerId != activePointerId)
        {
            return;
        }

        if (player != null)
        {
            player.AddLookInput(eventData.delta * lookMultiplier);
        }

        UpdateAimPosition(eventData.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId != activePointerId)
        {
            return;
        }

        UpdateAimPosition(eventData.position);
        activePointerId = int.MinValue;
    }

    private void UpdateAimPosition(Vector2 screenPosition)
    {
        if (hands == null)
        {
            return;
        }

        foreach (LaunchHand hand in hands)
        {
            if (hand != null)
            {
                hand.SetAimScreenPosition(screenPosition);
            }
        }
    }
}
