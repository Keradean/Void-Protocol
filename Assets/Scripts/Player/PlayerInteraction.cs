using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public List<Interactable> interactables;
    private Interactable closestInteractable;

    private UIManager uiManager;


    private void Start()
    {
        uiManager = FindFirstObjectByType<UIManager>();
    }
    public void InteractWithClosest()
    {
        if (closestInteractable != null)
        {
            closestInteractable.Interaction();
        }
    }

    public void UpdateClosestInteractable()
    {
        closestInteractable?.HighlitMaterial(false);
        uiManager?.HideInteractionText();

        closestInteractable = null;
        float closestDistance = float.MaxValue;

        foreach (Interactable interactable in interactables)
        {
            if (interactable == null) continue;

            float distance = Vector3.Distance(transform.position, interactable.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestInteractable = interactable;
            }
        }

            closestInteractable.HighlitMaterial(true);

            uiManager?.ShowInteractionText(closestInteractable.GetInteractionText());

    }
}
