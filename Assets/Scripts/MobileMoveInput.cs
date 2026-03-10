using UnityEngine;

public class MobileMoveInput : MonoBehaviour
{
    public RigidboyPlayerController player;

    public void SetInput(Vector2 value)
    {
        if (player != null)
        {
            player.SetMoveInput(value);
        }
    }

    public void SetInputFromFloats(float x, float y)
    {
        SetInput(new Vector2(x, y));
    }

    public void ResetInput()
    {
        SetInput(Vector2.zero);
    }
}
