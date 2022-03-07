using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace SK
{
    public class EnemyStatsUI : PoolObject
    {
        [SerializeField]
        private Slider slider;
        [SerializeField]
        private TextMesh text;

        private Transform thisTransform, targetTransform;
        private readonly string exclamationMark = "( 0_0 )";
        private float offsetY;
        private bool isAssign;

        private void Awake()
        {
            thisTransform = this.transform;
        }

        private void FixedUpdate()
        {
            if (isAssign)
            {
                thisTransform.position = Vector3.Lerp(thisTransform.position, targetTransform.position + Vector3.up * offsetY, 0.3f);
            }
        }

        public void Assign(Transform target, int maxValue)
        {
            isAssign = true;
            targetTransform = target;

            if (offsetY < 1) offsetY = targetTransform.GetComponent<Collider>().bounds.max.y - targetTransform.position.y;
            slider.gameObject.SetActive(true);
            slider.maxValue = maxValue;
        }

        public void Assign(Transform target, bool Astonished)
        {
            isAssign = true;
            targetTransform = target;

            if (offsetY < 1) offsetY = targetTransform.GetComponent<Collider>().bounds.max.y - targetTransform.position.y;
            text.gameObject.SetActive(true);
            text.text = exclamationMark;
            DOVirtual.DelayedCall(2, () => { text.gameObject.SetActive(false); });
        }

        public void UpdateState(int currentPoint) => slider.value = currentPoint;
        
        public void UpdateState(string str) => text.text = str;
        
        public void Unassign()
        {
            isAssign = false;
            targetTransform = null;
            offsetY = 0;
            text.gameObject.SetActive(false);
            slider.gameObject.SetActive(false);
            Done(true);
        }

        public bool isAssignBar()
        {
            if (slider.IsActive()) return true;
            else return false;
        }
    }
}
