using UnityEngine;

public class sol : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (Score.instance != null)
            {
                Score.instance.RegisterLeftNest();
            }
        }
    }
}