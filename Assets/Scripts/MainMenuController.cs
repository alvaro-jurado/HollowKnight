using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public Canvas mainCanvas;
    public Canvas optionsCanvas;
    public Canvas audioCanvas;
    public Canvas videoCanvas;
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

    public void Audio()
    {
        mainCanvas.GetComponent<Canvas>().enabled = false;
        optionsCanvas.GetComponent<Canvas>().enabled = false;
        videoCanvas.GetComponent<Canvas>().enabled = false;
        audioCanvas.GetComponent<Canvas>().enabled = true;
        fleurAnimator.SetTrigger("OpenFleur");
    }

    public void Video()
    {
        mainCanvas.GetComponent<Canvas>().enabled = false;
        optionsCanvas.GetComponent<Canvas>().enabled = false;
        videoCanvas.GetComponent<Canvas>().enabled = true;
        audioCanvas.GetComponent<Canvas>().enabled = false;
        fleurAnimator.SetTrigger("OpenFleur");
    }

    public void Back()
    {
        optionsCanvas.GetComponent<Canvas>().enabled = false;
        mainCanvas.GetComponent<Canvas>().enabled = true;
        videoCanvas.GetComponent<Canvas>().enabled = false;
        audioCanvas.GetComponent<Canvas>().enabled = false;
        fleurAnimator.SetTrigger("CloseFleur");
    }
    public void BackOptions()
    {
        optionsCanvas.GetComponent<Canvas>().enabled = true;
        mainCanvas.GetComponent<Canvas>().enabled = false;
        audioCanvas.GetComponent<Canvas>().enabled = false;
        videoCanvas.GetComponent<Canvas>().enabled = false;
        fleurAnimator.SetTrigger("CloseFleur");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
