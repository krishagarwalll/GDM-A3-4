using TMPro;
using UnityEngine;

public class PlayerInteractUI : MonoBehaviour
{
    [SerializeField] private GameObject containerGameObject;
    [SerializeField] private PlayerInteract playerInteract;
    [SerializeField] private TextMeshProUGUI interactTextMeshProUGUI;

    private void Awake()
    {
        if (playerInteract == null)
            playerInteract = FindFirstObjectByType<PlayerInteract>();
    }

    private void Update()
    {
        if (playerInteract == null)
        {
            hide();
            return;
        }

        var interactable = playerInteract.GetInteractableObject();
        if (interactable != null) Show(interactable);
        else hide();
    }

    private void Show(IInteractable interactable)
    {
        containerGameObject.SetActive(true);
        interactTextMeshProUGUI.text = interactable.GetInteractiveText();
    }

    private void hide()
    {
        containerGameObject.SetActive(false);
    }
}
