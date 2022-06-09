using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SK
{
    public class NPC : MonoBehaviour
    {
        [SerializeField] private string codeName;
        [SerializeField] private Collider _thisCollider;

        private Transform _thisTransform;
        private Transform _playerTransform;
        private CapsuleCollider _playerCollider;

        private float _dialgueAngle;
        private bool _onBoundsPlayer, _isActive;

        private void Awake()
        {
            _thisTransform = transform;
        }

        private void FixedUpdate()
        {
            if (!GameManager.Instance.Player) return;

            // 플레이어 바운드 할당
            if (_playerCollider == null)
            {
                _playerCollider = GameManager.Instance.Player.thisCollider;
                _playerTransform = GameManager.Instance.Player.transform;
                _dialgueAngle = GameManager.Instance.UIManager.dialogManager.dialogueAngle;
            }

            // Bounds를 이용한 콜라이더의 AABB 충돌체크
            if (_thisCollider.bounds.Intersects(_playerCollider.bounds))
                _onBoundsPlayer = true;
            else
                _onBoundsPlayer = false;

            // 플레이어의 콜라이더와 충돌된 경우
            if (_onBoundsPlayer)
            {
                // NPC의 위치에서 플레이어를 향한 방향
                var dir = _playerTransform.position - _thisTransform.position;

                // 플레이어와의 각도가 대화 가능한 각도가 되었을 경우 활성화
                if (Vector3.Angle(_thisTransform.forward, dir) <= _dialgueAngle)
                {
                    // 활성화가 안되어 있는 경우
                    if (!_isActive)
                    {
                        _isActive = true;

                        NpcActivate(true);
                    }
                }
                // 대화 가능한 각도에서 벗어난 경우 비활성화
                else
                {
                    if (_isActive)
                    {
                        _isActive = false;

                        NpcActivate(false);
                    }
                }
            }
            // 바운드에서 벗어난 경우
            else
            {
                // 비활성화가 안되어 있었다면 비활성화
                if (_isActive)
                {
                    _isActive = false;

                    NpcActivate(false);
                }
            }
        }

        private void NpcActivate(bool isActive)
        {
            // 활성화인 경우
            if (isActive)
            {
                // 머티리얼 아웃라인 활성화


                // Dialog Manager에게 NPC 대화 상대 할당
                GameManager.Instance.UIManager.dialogManager.AssignNPC(codeName);
            }
            // 비활성화인 경우
            else
            {
                // 머티리얼 아웃라인 비활성화


                // Dialog Manager에게 NPC 대화 상대 할당 해제
                GameManager.Instance.UIManager.dialogManager.UnassignNPC();
            }
        }
    }
}
