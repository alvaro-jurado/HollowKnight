using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public Canvas mainCanvas;
    public Canvas optionsCanvas;
    public Animator fleurAnimator;
    public void NewGame()
    {
        SceneManager.LoadScene("Godhome");
    }

    public void Options()
    {
        mainCanvas.GetComponent<Canvas>().enabled = false;
        optionsCanvas.GetComponent<Canvas>().enabled = true;
        fleurAnimator.SetTrigger("OpenFleur");
    }

    public void Back()
    {
        optionsCanvas.GetComponent<Canvas>().enabled = false;
        mainCanvas.GetComponent<Canvas>().enabled = true;
        fleurAnimator.SetTrigger("CloseFleur");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
