using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakable : MonoBehaviour
{
    public GameObject ParticleSystem;
    public MeshRenderer renderer;
    public BoxCollider collider;

    public GameObject objToDeactivate;


    public bool SwitchMaterials = false;

    public Material pristine;
    public Material damaged;
    public Material broken;

    public MeshRenderer[] renderers;

    public bool isbreakable = true;



    public void breakObject()
    {
        if (isbreakable)
        {
            ParticleSystem.SetActive(true);

            if (renderer != null)
            {
                renderer.enabled = false;

            }
            collider.enabled = false;

            if (objToDeactivate != null)
            {
                objToDeactivate.SetActive(false);
            }

            foreach (MeshRenderer render in renderers)
            {
                render.enabled = false;
            }
        }


    }
}
