using UnityEngine;

public class CableMaterials : MonoBehaviour
{
    [SerializeField] private MeshRenderer[] cableMeshes;
    [SerializeField] private Material glowingMaterial;
    [SerializeField] private Material normalMaterial;

    public void SetPowered(bool powered)
    {
        foreach (var mesh in cableMeshes)
        {
            if (mesh != null)
                mesh.sharedMaterial = powered ? glowingMaterial : normalMaterial;
        }
    }
}