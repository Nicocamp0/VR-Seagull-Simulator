using TMPro;
using UnityEngine;

public class Score : MonoBehaviour
{
    public static Score instance;

    [Header("Score")]
    public int currentScore = 0;
    public TMP_Text scoreText;

    [Header("Objectifs")]
    public TMP_Text objectiveText;

    [Header("Conditions de victoire")]
    public int scoreToUnlockBoat = 10;
    public int scoreToWin = 30;

    [Header("Téléportation")]
    public Transform playerTransform;
    public Transform spawnInit;

    [Header("Menus")]
    public GameObject endMenuCanvas;
    public GameObject endTutoCanvas;
    public MonoBehaviour flyScript;
    public FishSpawner fishSpawner;
    public GameObject hud;
    public StartMenu startMenuManager;

    [Header("Suivi des actions")]
    public bool hasLeftNest = false;
    public bool hasFlown = false;
    public bool hasCaughtCoconut = false;
    public bool hasBrokenCoconut = false;
    public bool hasEatenCoconut = false;
    public bool frenzyActivated = false;

    private int currentStep = 0;
    private bool gameWon = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (endMenuCanvas != null)
            endMenuCanvas.SetActive(false);
        if (endTutoCanvas != null)
            endTutoCanvas.SetActive(false);

        UpdateUI();
        UpdateObjective();
    }

    public void AddScore(int value)
    {
        currentScore += value;
        UpdateUI();
        RefreshObjectives();
    }

    public void RegisterLeftNest()
    {
        hasLeftNest = true;
        RefreshObjectives();
    }

    public void RegisterHasFlown()
    {
        hasFlown = true;
        RefreshObjectives();
    }

    public void RegisterCaughtCoconut()
    {
        hasCaughtCoconut = true;
        RefreshObjectives();
    }

    public void RegisterBrokenCoconut()
    {
        hasBrokenCoconut = true;
        RefreshObjectives();
    }

    public void RegisterCoconutEaten()
    {
        hasEatenCoconut = true;
        RefreshObjectives();
    }

    public void RegisterFrenzyActivated()
    {
        frenzyActivated = true;
        RefreshObjectives();
    }

    private void RefreshObjectives()
    {
        if (gameWon)
            return;

        if (currentStep == 0 && hasLeftNest)
            currentStep = 1;

        if (currentStep == 1 && hasFlown)
            currentStep = 2;

        if (currentStep == 2 && hasCaughtCoconut)
            currentStep = 3;

        if (currentStep == 3 && hasBrokenCoconut)
            currentStep = 4;

        if (currentStep == 4 && hasEatenCoconut)
        {
            currentStep = 5;
            TeleportPlayerToSpawnInit();
            if (startMenuManager != null)
            {
                startMenuManager.ShowEndTuto();
            }
            else if (endTutoCanvas != null)
            {
                endTutoCanvas.SetActive(true);
            }
        }

        if (currentStep == 5 && currentScore >= scoreToUnlockBoat)
            currentStep = 6;

        if (currentStep == 6 && frenzyActivated)
            currentStep = 7;

        if (currentStep == 7 && currentScore >= scoreToWin)
        {
            gameWon = true;
            ShowEndMenu();
        }

        UpdateObjective();
    }

    private void TeleportPlayerToSpawnInit()
    {
        if (playerTransform == null || spawnInit == null)
            return;

        CharacterController cc = playerTransform.GetComponent<CharacterController>();
        if (cc != null)
            cc.enabled = false;

        playerTransform.position = spawnInit.position;
        playerTransform.rotation = spawnInit.rotation;

        if (cc != null)
            cc.enabled = true;
    }

    private void ShowEndMenu()
    {
        if (startMenuManager != null)
        {
            startMenuManager.ShowEndMenu();
            return;
        }

        if (endMenuCanvas != null)
            endMenuCanvas.SetActive(true);

        if (flyScript != null)
            flyScript.enabled = false;

        if (fishSpawner != null)
            fishSpawner.enabled = false;

        if (hud != null)
            hud.SetActive(false);
    }

    public void ResumeAfterWin()
    {
        if (startMenuManager != null)
        {
            startMenuManager.ResumeAfterEndMenu();
            return;
        }
        if (endMenuCanvas != null)
            endMenuCanvas.SetActive(false);

        if (endTutoCanvas != null)
            endTutoCanvas.SetActive(false);

        if (flyScript != null)
            flyScript.enabled = true;

        if (fishSpawner != null)
            fishSpawner.enabled = true;

        if (hud != null)
            hud.SetActive(true);
    }

    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Score : " + currentScore;
    }

    private void UpdateObjective()
    {
        if (objectiveText == null)
            return;

        if (gameWon)
        {
            objectiveText.text = "Victoire !";
            return;
        }

        switch (currentStep)
        {
            case 0:
                objectiveText.text = "Objectif : Sors du nid";
                break;
            case 1:
                objectiveText.text = "Objectif : Envole-toi";
                break;
            case 2:
                objectiveText.text = "Objectif : Attrape une noix de coco";
                break;
            case 3:
                objectiveText.text = "Objectif : Lâche la coco de haut avec trigger gauche pour la casser";
                break;
            case 4:
                objectiveText.text = "Objectif : Mange la coco";
                break;
            case 5:
                objectiveText.text = "Objectif : Atteins " + scoreToUnlockBoat + " points";
                break;
            case 6:
                objectiveText.text = "Objectif : Explore le bateau pour trouver comment attirer les poissons hors de l'eau";
                break;
            case 7:
                objectiveText.text = "Objectif final : Atteins " + scoreToWin + " points";
                break;
            default:
                objectiveText.text = "Explore";
                break;
        }
    }
}