using UnityEngine;
using UnityEngine.EventSystems;

namespace SK.UI
{
    public class RotateRenderTextureCamera : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private float rotateSpeed;

        [System.Serializable]
        private enum RotateDirection { Left, Center, Right }

        [SerializeField] private RotateDirection direction;

        private Quaternion _rotatingValue;
        private bool inPressed;

        private void LateUpdate()
        {
            if (inPressed)
            {
                // 누른 방향에 따른 변수 값 설정
                var rotatingVariable = direction == RotateDirection.Left ? 1 : -1;

                // RenderTexture 카메라의 부모 트랜스폼의 로컬 회전
                _rotatingValue = GameManager.Instance.Player.renderCameraTransform.localRotation;
                GameManager.Instance.Player.renderCameraTransform.localRotation
                    = Quaternion.Lerp(_rotatingValue, 
                                      _rotatingValue * Quaternion.Euler(Vector3.up * rotatingVariable), 
                                      Time.deltaTime * rotateSpeed);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        { 
            // 중앙 버튼을 누를 경우 원위치(0, 0, 0)으로 변경_220513
            if (direction == RotateDirection.Center)
            {
                GameManager.Instance.Player.renderCameraTransform.localRotation = Quaternion.identity;
                return;
            }

            inPressed = true; 
        }

        public void OnPointerUp(PointerEventData eventData)        
            => inPressed = false;
    }
}