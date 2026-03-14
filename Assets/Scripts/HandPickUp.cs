using UnityEngine;

public class HandPickUp : MonoBehaviour
{
    public HandManager handmanager;
    public RigidboyPlayerController player;

    public HandType handToGive;

    public AudioSource globalaudio;
    public AudioClip pickupsx;

    public GameObject Bluehand;

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
                player.UpdateHandButtons();
                player.handtoSwitch = "red";
                player.playeranimations.SetTrigger("Switch");
                player.MobileSwitchRed();

                break;

            case HandType.Purple:
                handmanager.hasPurpleHand = true;
                player.UpdateHandButtons();
                player.handtoSwitch = "purple";
                player.playeranimations.SetTrigger("Switch");
                player.MobileSwitchPurple();

                break;

            case HandType.Pressure:
                handmanager.hasPressureHand = true;
                player.UpdateHandButtons();
                player.handtoSwitch = "flare";
                player.playeranimations.SetTrigger("Switch");
                player.MobileSwitchFlare();

                break;

            case HandType.Conductive:
                handmanager.hasConductiveHand = true;
                player.UpdateHandButtons();
                player.handtoSwitch = "conductive";
                player.playeranimations.SetTrigger("Switch");
                player.MobileSwitchConductive();

                break;

            case HandType.Blue:
                handmanager.hasBlueHand = true;
                Bluehand.SetActive(true);
                break;
        }

        if (handToGive != HandType.Blue)
        {
            player.playeranimations.SetBool("switch", true);
        }

        gameObject.SetActive(false);
    }
}