using UnityEngine;
using UnityEngine.EventSystems;

public class MobileLookInput : MonoBehaviour
{
    public RigidboyPlayerController player;

    [Range(0.5f, 20f)]
    public float sensitivity = 8f;

    private int lookFingerId = -1;

    void Update()
    {
        Vector2 lookDelta = Vector2.zero;

        // -------- MOBILE TOUCH --------
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            if (touch.phase == TouchPhase.Began &&
                touch.position.x > Screen.width * 0.5f &&
                lookFingerId == -1)
            {
                lookFingerId = touch.fingerId;
            }

            if (touch.fingerId == lookFingerId)
            {
                if (touch.phase == TouchPhase.Moved)
                    lookDelta = touch.deltaPosition;

                if (touch.phase == TouchPhase.Ended ||
                    touch.phase == TouchPhase.Canceled)
                    lookFingerId = -1;
            }
        }

        // -------- PC RIGHT CLICK DRAG --------
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButton(1)) // Right mouse held
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            lookDelta = new Vector2(mouseX, mouseY) * 30; 
            // 100 multiplier so it feels similar to touch delta
        }
#endif

        player.mobileLookInput = lookDelta * sensitivity * 0.01f;
    }
}