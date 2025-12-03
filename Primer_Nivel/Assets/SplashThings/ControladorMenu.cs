using UnityEngine;
using System.Collections;

public class ControladorMenu : MonoBehaviour
{
    public float waitTime = 4.0f;
    public Animator[] animators; // Lista de animators

    void Start()
    {
        StartCoroutine(WaitAndLoadMenu());
    }

    IEnumerator WaitAndLoadMenu()
    {
        yield return new WaitForSeconds(waitTime);
        LoadMenu();
    }

    private void LoadMenu()
    {
        // Activa el trigger en cada Animator de la lista
        foreach (Animator anim in animators)
        {
            if (anim != null)
            {
                anim.SetTrigger("Inicio");
            }
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
