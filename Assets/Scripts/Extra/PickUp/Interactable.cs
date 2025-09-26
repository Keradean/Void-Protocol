using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField] private MeshRenderer mesh;
    [SerializeField] private Material highlitMaterial;
    private Material defaultMaterial;

    private void Start()
    {
        if (mesh == null) mesh = GetComponent<MeshRenderer>();
        if (mesh != null)
        defaultMaterial = mesh.material;
    }

    public void HighlitMaterial(bool active)
    {
        if (mesh == null) return;

        if (active && highlitMaterial != null)
            mesh.material = highlitMaterial;
        else if (defaultMaterial != null)
            mesh.material = defaultMaterial;
    }

    public virtual void Interaction()
    {
       
    }

    public virtual string GetInteractionText()
    {
        return "Press E to Interact";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<PlayerInteraction>(out var playerInteraction)) return;

        playerInteraction.interactables.Add(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        
        if (!other.TryGetComponent<PlayerInteraction>(out var playerInteraction)) return;

        playerInteraction.interactables.Remove(this);
        playerInteraction.UpdateClosestInteractable();
    }
}
