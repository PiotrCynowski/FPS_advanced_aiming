using System.Collections;
using UnityEngine;

namespace DestrObj 
{
    public class ObjMovingOnHit : DestructibleObj
    {
        [SerializeField] private ParticleSystem onHitParticles;
        [SerializeField] Vector3[] movePos;
        private int len;

        [SerializeField] float movSpeed = 1f;
        private int indexNextPos = 0;

        private Coroutine movingCoroutine;
        private Vector3 targetPosition;


        private void Start()
        {
            len = movePos.Length;
        }

        public override void TakeDamage(int damage, Vector3 hitPos, Quaternion? hitRot)
        {
            base.TakeDamage(damage, hitPos, hitRot);

            if (damage != 0)
            {
                onHitParticles.transform.position = hitPos;
                onHitParticles.Play();
            }

            if (movingCoroutine == null)
            {
                movingCoroutine = StartCoroutine(MoveObject());
            }

            if(currentHealth <= 0)
            {
                StopCoroutine(movingCoroutine);
            }
        }


        #region moving
        private IEnumerator MoveObject()
        {
            indexNextPos++;
            if(indexNextPos >= len)
            {
                indexNextPos = 0;
            }

            targetPosition = movePos[indexNextPos];

            while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, movSpeed * Time.deltaTime);
                yield return null;
            }

            StopCoroutineMoveObj();
            yield return null;
        }

        private void StopCoroutineMoveObj()
        {
            StopCoroutine(MoveObject());
            movingCoroutine = null;
        }
        #endregion


#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            for (int i = 0; i < len; i++)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(movePos[i], 0.5f);
            }
        }
#endif
    }
}