using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class StartMenu : MonoBehaviour
{
    [Header("Menus")]
    public GameObject startMenu;
    public GameObject endTutoMenu;
    public GameObject endMenu;

    [Header("Gameplay")]
    public MonoBehaviour flyScript;
    public FishSpawner fishSpawner;
    public GameObject hud;

    [Header("XR UI Rays - Components")]
    public XRRayInteractor leftRayInteractor;
    public XRRayInteractor rightRayInteractor;
    public XRInteractorLineVisual leftLineVisual;
    public XRInteractorLineVisual rightLineVisual;

    [Header("XR UI Rays - GameObjects")]
    public GameObject leftRayObject;
    public GameObject rightRayObject;

    private void Start()
    {
        ShowStartMenu();
    }

    public void ShowStartMenu()
    {
        if (startMenu != null) startMenu.SetActive(true);
        if (endMenu != null) endMenu.SetActive(false);
        if (endTutoMenu != null) endTutoMenu.SetActive(false);

        SetGameplayEnabled(false);
        SetRaysVisible(true);
    }

    public void StartGame()
    {
        if (startMenu != null) startMenu.SetActive(false);
        if (endMenu != null) endMenu.SetActive(false);
        if (endTutoMenu != null) endTutoMenu.SetActive(false);

        SetGameplayEnabled(true);
        SetRaysVisible(false);
    }

        public void ShowEndTuto()
    {
        if (endTutoMenu != null) endTutoMenu.SetActive(true);
        if (startMenu != null) startMenu.SetActive(false);

        SetGameplayEnabled(false);
        SetRaysVisible(true);

        Debug.Log("ShowEndTuto appelé : rays réactivés");
    }

    public void ShowEndMenu()
    {
        if (endMenu != null) endMenu.SetActive(true);
        if (startMenu != null) startMenu.SetActive(false);

        SetGameplayEnabled(false);
        SetRaysVisible(true);

        Debug.Log("ShowEndMenu appelé : rays réactivés");
    }

    public void ResumeAfterEndMenu()
    {
        if (endMenu != null) endMenu.SetActive(false);
        if (endTutoMenu !=null) endTutoMenu.SetActive(false);

        SetGameplayEnabled(true);
        SetRaysVisible(false);

        Debug.Log("ResumeAfterEndMenu appelé : rays cachés");
    }

    private void SetGameplayEnabled(bool enabled)
    {
        if (flyScript != null)
            flyScript.enabled = enabled;

        if (fishSpawner != null)
            fishSpawner.enabled = enabled;

        if (hud != null)
            hud.SetActive(enabled);
    }

    private void SetRaysVisible(bool visible)
    {
        if (leftRayObject != null)
            leftRayObject.SetActive(visible);

        if (rightRayObject != null)
            rightRayObject.SetActive(visible);

        if (leftRayInteractor != null)
            leftRayInteractor.enabled = visible;

        if (rightRayInteractor != null)
            rightRayInteractor.enabled = visible;

        if (leftLineVisual != null)
            leftLineVisual.enabled = visible;

        if (rightLineVisual != null)
            rightLineVisual.enabled = visible;
    }
}