using UnityEngine;

public class buttonwhalesound : MonoBehaviour
{
    private AudioSource audioSource;

    public string beakTag = "Player";
    public FishSpawner fishSpawner;

    private bool alreadyActivated = false;

    void Start()
    {
        audioSource = GetComponentInChildren<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(beakTag))
            return;

        Debug.Log("Bouton touché par le joueur");

        if (!alreadyActivated)
        {
            alreadyActivated = true;

            if (fishSpawner != null)
            {
                Debug.Log("Activation du frenzy");
                fishSpawner.ActivatePermanentFrenzy();
            }
            else
            {
                Debug.LogWarning("fishSpawner non assigné sur le bouton !");
            }

            if (Score.instance != null)
            {
                Score.instance.RegisterFrenzyActivated();
            }
        }

        if (audioSource != null)
        {
            audioSource.Play();
        }
    }
}