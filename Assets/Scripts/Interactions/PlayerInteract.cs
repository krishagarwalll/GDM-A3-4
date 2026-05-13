using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Game.Core.Pause;
using Game.Core.Audio;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private float interactRange = 3f;

    [Header("Audio")]
    [SerializeField] private string selectSfxName = "select";
    [SerializeField, Range(0f, 1f)] private float selectSfxVolume = 1f;

    private void Update()
    {
        if (PauseService.IsPaused) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            IInteractable interactable = GetInteractableObject();
            if (interactable != null)
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFXByName(selectSfxName, selectSfxVolume);

                interactable.Interact();
            }
        }
    }
    
    public IInteractable GetInteractableObject() 
    {
        Collider2D[] colliderArray = Physics2D.OverlapCircleAll(transform.position, interactRange);
        IInteractable closestInteractable = null;
        float closestDistanceSqr = float.MaxValue;

        foreach (Collider2D collider in colliderArray) 
        {
            if (collider.TryGetComponent(out IInteractable interactable)) 
            {
                float distanceSqr = (collider.transform.position - transform.position).sqrMagnitude;
                if (distanceSqr < closestDistanceSqr)
                {
                    closestDistanceSqr = distanceSqr;
                    closestInteractable = interactable;
                }
            }
        }

        return closestInteractable;
    }
}
