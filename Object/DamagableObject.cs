using UnityEngine;

namespace SK
{
    [RequireComponent(typeof(Collider))]
    public class DamagableObject : MonoBehaviour
    {
        [SerializeField] private uint damage;
        [SerializeField] private LayerMask targetLayerMask;

        private void OnTriggerEnter(Collider other)
        {
            int objectLayer = other.gameObject.layer;
            if ((targetLayerMask.value & 1 << objectLayer) == 1 << objectLayer)
            {
                SceneManager.Instance.GetUnit(other.gameObject.GetInstanceID()).OnDamage(transform, damage, false);
            }
        }
    }
}
