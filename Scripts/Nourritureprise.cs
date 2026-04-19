using UnityEngine;

public class Nourritureprise : MonoBehaviour
{
    public Transform beakHoldPoint;
    public string nourritureTag = "nourriture";
    public float pickupCooldown = 2f;

    private Nourriture heldnourriture;

    private void OnTriggerEnter(Collider other)
    {
        if (heldnourriture != null) return;
        if (!other.CompareTag(nourritureTag)) return;

        Nourriture nourriture = other.GetComponentInParent<Nourriture>();
        if (nourriture == null)
        {
            nourriture = other.GetComponentInChildren<Nourriture>();
        }

        if (nourriture == null) return;
        if (nourriture.isCollected) return;
        if (Time.time < nourriture.lastDropTime + pickupCooldown) return;

        nourriture.Collect(beakHoldPoint);

        if (nourriture.type == CollectibleType.dansBeque)
        {
            heldnourriture = nourriture;
            Score.instance.RegisterCaughtCoconut();
        }
    }

    public void DropHeldnourriture()
    {
        if (heldnourriture == null) return;

        heldnourriture.Drop();
        heldnourriture = null;
    }
}