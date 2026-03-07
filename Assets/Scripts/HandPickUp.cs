using UnityEngine;

public class HandPickUp : MonoBehaviour
{
    public HandManager handmanager;
    public RigidboyPlayerController player;

    public HandType handToGive;

    public AudioSource globalaudio;
    public AudioClip pickupsx;

    public enum HandType
    {
        Red,
        Purple,
        Pressure,
        Conductive,
        Blue
    }

    private void Start()
    {
        if (PlayerAlreadyHasHand())
        {
            gameObject.SetActive(false);
        }
    }

    private bool PlayerAlreadyHasHand()
    {
        switch (handToGive)
        {
            case HandType.Red:
                return handmanager.hasRedHand;

            case HandType.Purple:
                return handmanager.hasPurpleHand;

            case HandType.Pressure:
                return handmanager.hasPressureHand;

            case HandType.Conductive:
                return handmanager.hasConductiveHand;

            case HandType.Blue:
                return handmanager.hasBlueHand;
        }

        return false;
    }

    public void PickupHand()
    {
        if (!handmanager.hasGrabPack)
            return;

        globalaudio.PlayOneShot(pickupsx, 1.0f);

        switch (handToGive)
        {
            case HandType.Red:
                handmanager.hasRedHand = true;
                player.handtoSwitch = "red";
                player.playeranimations.SetTrigger("Switch");
                break;

            case HandType.Purple:
                handmanager.hasPurpleHand = true;
                player.handtoSwitch = "purple";
                player.playeranimations.SetTrigger("Switch");

                break;

            case HandType.Pressure:
                handmanager.hasPressureHand = true;
                player.handtoSwitch = "flare";
                player.playeranimations.SetTrigger("Switch");

                break;

            case HandType.Conductive:
                handmanager.hasConductiveHand = true;
                player.handtoSwitch = "conductive";
                player.playeranimations.SetTrigger("Switch");

                break;

            case HandType.Blue:
                handmanager.hasBlueHand = true;
                break;
        }

        if (handToGive != HandType.Blue)
        {
            player.playeranimations.SetBool("switch", true);
        }

        gameObject.SetActive(false);
    }
}