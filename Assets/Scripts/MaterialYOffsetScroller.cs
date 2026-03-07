using UnityEngine;

public class MaterialYOffsetScroller : MonoBehaviour
{
    public Renderer targetRenderer;
    public float scrollSpeed = 0.1f;
    private Material material;
    private Vector2 currentOffset;

    private void Start()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }

        if (targetRenderer != null)
        {
            material = targetRenderer.material;
            currentOffset = material.mainTextureOffset;
        }
    }

    private void Update()
    {
        if (material != null)
        {
            currentOffset.y += scrollSpeed * Time.deltaTime;
            material.mainTextureOffset = currentOffset;
        }
    }
}
