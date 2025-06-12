using UnityEngine;

namespace Player
{
    public class PlayerInteract : MonoBehaviour
    {
        private ICanBeInteracted interactable;

        public bool RaycastInfo(ICanBeInteracted interactable)
        {
            if (interactable == null)
            {
                this.interactable = null;
                return false;
            }

            if (!ReferenceEquals(this.interactable, interactable))
            {
                this.interactable = interactable;
                return interactable.IsConditionMet();
            }
            return false;
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
    bool IsConditionMet();
}

