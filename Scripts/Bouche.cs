using UnityEngine;
using UnityEngine.InputSystem;

public class Bouche : MonoBehaviour
{
    public Nourritureprise nourritureprise;
    public InputActionReference dropAction;

    private void OnEnable()
    {
        dropAction.action.Enable();
        dropAction.action.performed += OnDropPerformed;
    }

    private void OnDisable()
    {
        dropAction.action.performed -= OnDropPerformed;
        dropAction.action.Disable();
    }

    private void OnDropPerformed(InputAction.CallbackContext context)
    {
        nourritureprise.DropHeldnourriture(); 
    }
}