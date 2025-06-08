using UnityEngine;

namespace Player
{
    public class PlayerInteract : MonoBehaviour
    {
        private ICanBeInteracted interactable;

        public void RaycastInfo(ICanBeInteracted interactable)
        {
            if (interactable == null)
            {
                this.interactable = null;
                return;
            }

            if (!ReferenceEquals(this.interactable, interactable))
            {
                this.interactable = interactable;
            }
        }

        public void Interact()
        {
            if (this.interactable != null)
            {
                interactable.OnInteracted();
            }
        }
    }
}

public interface ICanBeInteracted
{
    void OnInteracted();
}

