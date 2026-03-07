using UnityEngine;

public class NumberMaterialChanger : MonoBehaviour
{
    public Renderer targetRenderer;
    public Material[] numberMaterials; 
    public Material nullMaterial;
    public Keypad keypad; 
    public int position = 0; 

    private void Start()
    {
        if (keypad == null)
        {
            keypad = FindObjectOfType<Keypad>(); 
        }

        UpdateMaterial();
    }

    private void Update()
    {
        UpdateMaterial();
    }

    public void UpdateMaterial()
    {
        if (targetRenderer == null || keypad == null || numberMaterials.Length < 10) return;

        string code = keypad.Code;

        if (position < 0 || position >= code.Length)
        {
            targetRenderer.material = nullMaterial; 
            return;
        }

        char numberChar = code[position];

        if (char.IsDigit(numberChar))
        {
            int index = numberChar - '0'; 
            targetRenderer.material = numberMaterials[index];
        }
        else
        {
            targetRenderer.material = nullMaterial;
        }
    }
}
