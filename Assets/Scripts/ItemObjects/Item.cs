using UnityEngine;

public class Item : MonoBehaviour, ICanBeGrabbed
{
    public Transform anchor;

    [SerializeField] GameObject indicatorGrab;
    [SerializeField] GameObject indicatorDist;

    private Rigidbody rb;
    private Collider coll;
    private bool isInHand;
  
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        coll = GetComponent<Collider>();
    }

    #region item interaction
    public void PickItem(bool isPicked)
    {
        rb.useGravity = !isPicked;
        rb.isKinematic = isPicked;
        coll.isTrigger = isPicked;
        isInHand = isPicked;

        DistIndicatorSwitch(!isPicked);
      
    }

    public void ThrowItem(Vector3 direction, float force)
    {
        PickItem(false);

        rb.AddForce(direction * force, ForceMode.Impulse);   
    }
    #endregion

    #region indicator
    public void OnRaycastAim(RaycastAimState aimState)
    {
        if (isInHand)
        { 
            return;
        }

        switch (aimState)
        {
            case RaycastAimState.clicked:
                indicatorGrab.SetActive(false);
                break;

            case RaycastAimState.aimedAt:
                indicatorGrab.SetActive(true);
                break;

            case RaycastAimState.leftAlone:
                indicatorGrab.SetActive(false);
                break;

            default:
                Debug.LogWarning("wrong enum state: " + aimState);
                break;
        }
    }

    public void DistIndicatorSwitch(bool isActive)
    {
        if (isInHand)
        {
            indicatorDist.SetActive(false);
            return;
        }

        indicatorDist.SetActive(isActive);
    }
    #endregion
}