using UnityEngine;

namespace SK.Behavior
{
    [RequireComponent(typeof(Combat))]
    public class Alert : MonoBehaviour
    {
        [Header("Debug")]
        public bool debugAlertRange;

        [Header("Alert System")]
        [SerializeField] private bool canAlert;
        [SerializeField] private float alertRange = 10;

        private Transform _transform;
        [SerializeField] private Collider[] _colliderBuff;
        [SerializeField] private int _enemyLayerMask;
        private Enemy temp;

        private void Awake() => GetComponent<Combat>().alert = this;
        
        private void Start()
        {
            _transform = transform;
            _enemyLayerMask = 1 << gameObject.layer;
            _colliderBuff = new Collider[10];
        }

        public void SendAlert(GameObject target)
        {
            if (!canAlert) return;

            if (Physics.OverlapSphereNonAlloc(_transform.position, alertRange, _colliderBuff,
                _enemyLayerMask, QueryTriggerInteraction.Collide) > 0)
            {
                foreach (var col in _colliderBuff)
                {
                    if (col != null && col.gameObject != gameObject)
                    {
                        temp = col.GetComponent<Enemy>();
                        if (!temp.isDead && !temp.combat.TargetObject) temp.GetAlert(target);
                    }
                }
            }
        }

        #region Debug
        private void DrawAlertRange()
        {
#if UNITY_EDITOR
            var oldColor = UnityEditor.Handles.color;
            var color = Color.magenta;
            color.a = 0.1f;
            UnityEditor.Handles.color = color;

            UnityEditor.Handles.DrawSolidDisc(transform.position, transform.up, alertRange);

            UnityEditor.Handles.color = oldColor;
#endif
        }

        private void OnDrawGizmosSelected()
        {
            if (debugAlertRange) DrawAlertRange();
        }
        #endregion
    }
}
