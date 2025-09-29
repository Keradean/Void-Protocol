using TMPro;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private MeshRenderer mesh; 
    private Material defaultMaterial;
    
    [Header("Text")]
    [SerializeField] private GameObject interactTMP;
    

    private void Start()
    {
        if(mesh == null) mesh = GetComponent<MeshRenderer>();

        defaultMaterial = mesh.material;

        Debug.Log("Interactable" + gameObject.name);
    }

    public virtual void Interaction()
    {

    }
    public void HighlightActive(bool active)
    {
        if (mesh == null) return;

        if (active && highlightMaterial != null)
            mesh.material = highlightMaterial;
        else if (!active && defaultMaterial != null)
            mesh.material = defaultMaterial;
    }

    public void InteractionText(bool active)
    {
        if (interactTMP == null) return;

        if (active && interactTMP != null)
        {
            interactTMP.SetActive(true);
        }
        else if(!active && interactTMP != null)
        {
            interactTMP.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerInteraction playerInteraction = other.GetComponent<PlayerInteraction>();
        if (playerInteraction == null) return;

        playerInteraction.interactables.Add(this);
        playerInteraction.UpdateClosestInteractable();
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerInteraction playerInteraction = other.GetComponent<PlayerInteraction>();
        if (playerInteraction == null) return;

        playerInteraction.interactables.Remove(this);
        playerInteraction.UpdateClosestInteractable();
    }

    private void OnDestroy()
    {
        
        PlayerInteraction playerInteraction = FindFirstObjectByType<PlayerInteraction>();
        if (playerInteraction != null && playerInteraction.interactables.Contains(this))
        {
            playerInteraction.interactables.Remove(this);
            playerInteraction.UpdateClosestInteractable();
        }
    }
}
