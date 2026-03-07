using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetSceneButton : MonoBehaviour
{
    public void ResetScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}