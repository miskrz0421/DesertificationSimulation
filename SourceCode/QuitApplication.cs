using UnityEngine;

public class QuitApplication : MonoBehaviour
{
    public void Quit()
    {
        Debug.Log("Application Quit");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
