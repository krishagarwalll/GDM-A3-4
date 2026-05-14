using UnityEngine;
using Game.Core.Events;

public class Treasure : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactText = "Collection Treasure";
    [SerializeField] private VoidEventChannelSO onCollected;
    [SerializeField] private GameObject visualObject;

    private bool isCollected;

    public void Interact()
    {
        if (isCollected) return;

        isCollected = true;
        onCollected?.Raise();

        if (visualObject != null)
            visualObject.SetActive(false);
        else
            gameObject.SetActive(false);
    }

    public string GetInteractiveText()
    {
        return isCollected ? "" : interactText;
    }
}
