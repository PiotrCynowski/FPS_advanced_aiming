using UnityEngine;

namespace Player
{
    public class PlayerInteract : MonoBehaviour
    {
        private ICanBeInteracted interactable;

        public string RaycastInfo(ICanBeInteracted interactable)
        {
            if (interactable == null)
            {
                this.interactable = null;
                return string.Empty;
            }

            if (!ReferenceEquals(this.interactable, interactable))
            {
                this.interactable = interactable;
                return interactable.IsConditionMet();
            }
            return string.Empty;
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
    string IsConditionMet();
}

