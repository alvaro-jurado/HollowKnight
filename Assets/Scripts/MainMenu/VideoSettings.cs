using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.Linq;

public class VideoSettings : MonoBehaviour
{
    public TMP_Text resolutionText;
    public Button applyButton;
    public Button leftArrow;
    public Button rightArrow;
    public Toggle fullscreenToggle;
    public Canvas mainCanvas;
    public Canvas videoCanvas;

    public GameObject confirmationWindow;
    public Button confirmButton;
    public Button cancelButton;
    public float revertTime = 10f;
    private Resolution previousResolution;
    private bool previousFullscreen;
    private Coroutine revertCoroutine;

    private Resolution[] resolutions;
    private int currentResolutionIndex;
    private int appliedResolutionIndex;

    private bool isFullscreen;

    void Start()
    {
        /*int maxWidth = Screen.currentResolution.width;
        int maxHeight = Screen.currentResolution.height;

        if (Screen.width > maxWidth || Screen.height > maxHeight)
        {
            Screen.SetResolution(maxWidth / 2, maxHeight / 2, false); // Ajustar resolución a la mitad
        }*/

        resolutions = Screen.resolutions
       .Distinct()
       .Where(r => r.refreshRateRatio.Equals(Screen.currentResolution.refreshRateRatio))
       .ToArray();

        SetDefaultResolution(1920, 1080);

        currentResolutionIndex = GetCurrentResolutionIndex();
        appliedResolutionIndex = currentResolutionIndex;


        isFullscreen = Screen.fullScreen;
        fullscreenToggle.isOn = isFullscreen;
        
        UpdateResolutionText();

        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        leftArrow.onClick.AddListener(PreviousResolution);
        rightArrow.onClick.AddListener(NextResolution);
        applyButton.onClick.AddListener(ApplyChanges);
    }

    private void SetDefaultResolution(int width, int height)
    {
        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width == width && resolutions[i].height == height)
            {
                currentResolutionIndex = i;
                break;
            }
        }

        if (currentResolutionIndex < 0 || currentResolutionIndex >= resolutions.Length)
        {
            currentResolutionIndex = 0;
        }

        Resolution defaultResolution = resolutions[currentResolutionIndex];
        Screen.SetResolution(defaultResolution.width, defaultResolution.height, Screen.fullScreen);

        UpdateResolutionText();
    }

    private int GetCurrentResolutionIndex()
    {
        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                return i;
            }
        }
        return 0;
    }

    private void UpdateResolutionText()
    {
        resolutionText.text = resolutions[currentResolutionIndex].width + " x " +
                              resolutions[currentResolutionIndex].height;
    }

    public void PreviousResolution()
    {
        currentResolutionIndex--;
        if (currentResolutionIndex < 0)
        {
            currentResolutionIndex = resolutions.Length - 1;
        }
        UpdateResolutionText();
    }

    public void NextResolution()
    {
        currentResolutionIndex++;
        if (currentResolutionIndex >= resolutions.Length)
        {
            currentResolutionIndex = 0;
        }     
        UpdateResolutionText();
    }

    public void SetFullscreen(bool fullscreen)
    {
        isFullscreen = fullscreen;
    }

    public void ApplyChanges()
    {
        previousResolution = resolutions[appliedResolutionIndex];
        previousFullscreen = isFullscreen;

        Resolution selectedResolution = resolutions[currentResolutionIndex];
        Screen.SetResolution(selectedResolution.width, selectedResolution.height, isFullscreen);

        appliedResolutionIndex = currentResolutionIndex;

        confirmationWindow.SetActive(true);

        if (revertCoroutine != null)
            StopCoroutine(revertCoroutine);
        revertCoroutine = StartCoroutine(RevertChangesAfterDelay());

        mainCanvas.GetComponent<Canvas>().enabled = false;
        videoCanvas.GetComponent<Canvas>().enabled = true;
    }

    public void ConfirmChanges()
    {
        confirmationWindow.SetActive(false);
        if (revertCoroutine != null)
            StopCoroutine(revertCoroutine);
    }

    public void CancelChanges()
    {
        Screen.SetResolution(previousResolution.width, previousResolution.height, previousFullscreen);

        confirmationWindow.SetActive(false);
        if (revertCoroutine != null)
            StopCoroutine(revertCoroutine);

        currentResolutionIndex = GetIndexForResolution(previousResolution);
        UpdateResolutionText();

        fullscreenToggle.onValueChanged.RemoveAllListeners();
        fullscreenToggle.isOn = previousFullscreen;
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }

    private int GetIndexForResolution(Resolution resolution)
    {
        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width == resolution.width &&
                resolutions[i].height == resolution.height)
            {
                return i;
            }
        }
        return 0;
    }

    private IEnumerator RevertChangesAfterDelay()
    {
        yield return new WaitForSeconds(revertTime);

        CancelChanges();
    }

}
