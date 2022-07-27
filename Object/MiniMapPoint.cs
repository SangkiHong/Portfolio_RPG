using UnityEngine;

namespace SK
{
    public class MiniMapPoint : MonoBehaviour
    {
        private Transform _thisTransform;
        private Vector3 _defaultRotation;

        private void Awake()
        {
            _thisTransform = transform;
            _defaultRotation = _thisTransform.rotation.eulerAngles;

            SceneManager.Instance.OnFixedUpdate += UpdateRotation;
        }

        private void UpdateRotation()
        {
            _defaultRotation.y = _thisTransform.rotation.eulerAngles.y;
            _thisTransform.rotation = Quaternion.Euler(_defaultRotation);
        }
    }
}
