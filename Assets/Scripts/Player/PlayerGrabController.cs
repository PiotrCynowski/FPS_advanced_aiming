using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player 
{
    public class PlayerGrabController : MonoBehaviour 
    {
        [Header("References")]
        [SerializeField] private Transform itemHoldAnchor;
        [SerializeField] private Transform cameraRoot;
        [Header("Settings")]
        [SerializeField] private float itemFlyToHandSpeed;
        [SerializeField] private float itemThrowForce;
        [SerializeField] private float interactionDistance;
        [SerializeField] private float pickableItemsIndicateDistance;
        [SerializeField] private LayerMask toRayInteract;

        private GrabbableItem itemPicked;
        private List<GrabbableItem> itemCollCache;

        private Coroutine moveItemCoroutine;
        private ICanBeGrabbed currentAim;

        private Ray ray;
        private const int framesPerRaycast = 10;
        private int frameCount = 0;

        private float itemDistance;
        private Vector3 playerPos;

        private RaycastAimState aimedObj;
        public RaycastAimState AimedObj {
            get 
            { 
                return aimedObj; 
            }
            set 
            {
                if (aimedObj != value) 
                {
                    aimedObj = value;
                    CallCurrentAim(aimedObj);
                }
            }
        }

        private void Awake() 
        {
            aimedObj = RaycastAimState.leftAlone;
        }

        private void Start() 
        {
            IndicateItemsToPick(true);
        }

        private void Update() 
        {
            frameCount++;
            if (frameCount >= framesPerRaycast) 
            {
                CheckItemDistByPlayerPos();
                frameCount = 0;
            }
        }

        #region player position check
        private void CheckItemDistByPlayerPos() 
        {
            if (Vector3.Distance(transform.position, playerPos) > pickableItemsIndicateDistance) 
            {
                playerPos = transform.position;
                IndicateItemsToPick(true);
            }
            else {
                IndicateItemsToPick(false);
            }
        }
        #endregion

        #region input system receive message
        public void OnMouseLMB(bool isPressed)
        {
            if (isPressed && itemPicked != null)
                ItemThrow();
        }

        public void OnMouseRMB(bool isPressed)
        {
            if (isPressed)
                ItemGrab();
            else if (itemPicked != null)
                ItemRelease();
        }
        #endregion

        #region item interaction
        private void ItemGrab() 
        {
            if (currentAim != null)
            {
                currentAim.PickItem(true);
                itemPicked = currentAim.GetInteractable();
                moveItemCoroutine = StartCoroutine(MoveItemToHoldPos());
                aimedObj = RaycastAimState.grabbed;
            }
        }

        private void ItemRelease() 
        {
            if (moveItemCoroutine != null) 
                StopCoroutine(moveItemCoroutine);

            itemPicked.transform.parent = null;
            itemPicked.PickItem(false);
            itemPicked = null;
        }

        private void ItemThrow() 
        {
            if (moveItemCoroutine != null) 
                StopCoroutine(moveItemCoroutine);

            itemPicked.transform.parent = null;
            itemPicked.ThrowItem(cameraRoot.forward, itemThrowForce);
            itemPicked = null;
        }

        private IEnumerator MoveItemToHoldPos() 
        {
            float distance = Vector3.Distance(itemPicked.transform.position, itemPicked.anchor.transform.position);
            float time = 0;
            itemPicked.transform.parent = itemHoldAnchor;
            while (time < 1) {
                time += 0.01f * itemFlyToHandSpeed;

                itemPicked.transform.rotation = Quaternion.Lerp(itemPicked.transform.rotation,
                    Quaternion.LookRotation(itemHoldAnchor.transform.TransformDirection(Vector3.forward)) * Quaternion.Inverse(itemPicked.anchor.transform.localRotation),
                    time);

                itemPicked.transform.position = Vector3.Lerp(itemPicked.transform.position,
                    itemHoldAnchor.position + ((itemPicked.transform.position - itemPicked.anchor.transform.position).normalized * distance),
                    time);

                yield return null;
            }
        }
        #endregion

        #region raycast interaction
        public void RaycastInfo(ICanBeGrabbed grabbable)
        {
            if (itemPicked != null) return;

            if (grabbable != null)
            {
                if (currentAim == null)
                {
                    currentAim = grabbable;
                    AimedObj = RaycastAimState.aimedAt;
                    return;
                }

                if (!ReferenceEquals(currentAim, grabbable))
                {
                    AimedObj = RaycastAimState.leftAlone;

                    currentAim = grabbable;
                    AimedObj = RaycastAimState.aimedAt;
                }
            }
            else
            {
                AimedObj = RaycastAimState.leftAlone;
                currentAim = null;
                return;
            }
        }

        public bool IsAimedAt() { return AimedObj != RaycastAimState.leftAlone; }

        private void CallCurrentAim(RaycastAimState aimState) 
        {
            currentAim?.OnRaycastAim(aimState);
        }

        private void IndicateItemsToPick(bool isItemsCacheRefresh) 
        {
            if (isItemsCacheRefresh || itemCollCache.Count < 0) 
            {
                Collider[] colliders = Physics.OverlapSphere(transform.position, 5 * pickableItemsIndicateDistance, toRayInteract);

                itemCollCache = new();

                for (int i = 0; i < colliders.Length; i++) 
                {
                    if (colliders[i].TryGetComponent<GrabbableItem>(out var obj)) 
                    {
                        itemCollCache.Add(obj);
                    }
                }
            }

            for (int i = 0; i < itemCollCache.Count; i++) 
            {
                if (itemCollCache[i] == null) 
                {
                    break;
                }

                itemDistance = Vector3.Distance(transform.position, itemCollCache[i].transform.position);

                itemCollCache[i].DistIndicatorSwitch(itemDistance > pickableItemsIndicateDistance ? false : true);
            }
        }
        #endregion
    }
}

public interface ICanBeGrabbed 
{
    void OnRaycastAim(RaycastAimState aimState);

    void PickItem(bool isPicked);

    GrabbableItem GetInteractable();
}

public enum RaycastAimState 
{
    aimedAt,
    grabbed,
    leftAlone
}