using System.Collections.Generic;
using UnityEngine;

namespace SK
{
    public class NPC : MonoBehaviour
    {
        public enum NPCType
        {
            Normal, // 일반 NPC
            PropShop,
            EquipmentShop
        }

        [SerializeField] private string codeName; // 코드 내부 사용할 NPC 이름
        [SerializeField] private string displayName; // UI에 표시될 NPC 이름
        [SerializeField] private NPCType npctype; // NPC 타입
        [SerializeField] private Collider _thisCollider; // NPC의 콜라이더
        [SerializeField] private Quests.Quest[] npcQuests; // NPC의 퀘스트

        [Header("Looking")]
        [SerializeField] private bool canRotating; // 대화 시 플레이어를 바라볼 수 있는 지의 여부
        [SerializeField] private float lookTime = 1; // 플레이어 바라보는 시간
        public Transform lookTarget;

        // 프로퍼티
        public string DisplayName => displayName;
        public NPCType NpcType => npctype;
        public IReadOnlyList<Quests.Quest> NpcQuests => npcQuests;

        private Transform _thisTransform; // 트랜스폼 캐싱
        private Transform _playerTransform; // 플레이어 트랜스폼
        private CapsuleCollider _playerCollider; // 플레이어의 콜라이더

        private Vector3 _rotateDirection;
        private float _dialgueAngle; // 대화 가능 각도
        private float _elapsed; // 플레이어를 향해 회전하는 경과 시간
        private bool _isActive; // 대화 가능 상태 활성화 여부
        private bool _isRotating; // 플레이어를 향해 회전중인지의 여부

        private void Awake()        
            => _thisTransform = transform;

        private void Start()
        {
            // 생성 시 씬 매니저의 딕셔너리에 추가(키: 코드네임, 값: NPC)
            SceneManager.Instance.AddNPC(codeName, this);
            // 콜라이더의 트리거 체크
            if (!_thisCollider.isTrigger) _thisCollider.isTrigger = true;
        }
        
        public void FixedTick()
        {
            if (!GameManager.Instance.Player) return;

            // 플레이어 바운드 할당
            if (_playerCollider == null)
            {
                _playerCollider = GameManager.Instance.Player.thisCollider;
                _playerTransform = GameManager.Instance.Player.transform;
                _dialgueAngle = UI.UIManager.Instance.dialogManager.dialogueAngle;
            }

            #region Bounds를 이용한 콜라이더의 AABB 충돌체크
            if (_thisCollider.bounds.Intersects(_playerCollider.bounds))
            {
                // NPC의 위치에서 플레이어를 향한 방향
                var dir = _thisTransform.position - _playerTransform.position;

                // 플레이어와의 각도가 대화 가능한 각도가 되었을 경우 활성화
                if (Vector3.Angle(_playerTransform.forward, dir) <= _dialgueAngle)
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
            #endregion

            // 플레이어를 향해 회전
            if (_isRotating)
            {
                _elapsed += Time.fixedDeltaTime;
                _thisTransform.rotation = Quaternion.Lerp(_thisTransform.rotation, Quaternion.LookRotation(_rotateDirection), _elapsed / lookTime);
            }
        }

        private void NpcActivate(bool isActive)
        {
            // 활성화인 경우
            if (isActive)
            {
                // 머티리얼 아웃라인 활성화


                // Dialog Manager에게 NPC 대화 상대 할당
                UI.UIManager.Instance.dialogManager.AssignNPC(codeName);
            }
            // 비활성화인 경우
            else
            {
                // 머티리얼 아웃라인 비활성화


                // Dialog Manager에게 NPC 대화 상대 할당 해제
                UI.UIManager.Instance.dialogManager.UnassignNPC();
            }
        }

        public void StartConversation()
        {
            if (canRotating)
            {
                _elapsed = 0;
                _rotateDirection = (_playerTransform.position - _thisTransform.position).normalized;
                _rotateDirection.y = 0;
                _isRotating = true;
            }
        }
    }
}
