using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class uncrouch : MonoBehaviour
{
    public RigidboyPlayerController player;


    
    public void uncrouchplayer()
    {
        player.IsCrouched = false;


    }
    public void crouchplayer()
    {
        player.IsCrouched = true;


    }

    public void Switcher()
    {
        player.SwitchHand();
    }

    public void EndSwitch()
    {
        player.playeranimations.SetBool("switch", false);

        player.canSwitch = true;
    }


}
