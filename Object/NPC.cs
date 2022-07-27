using System.Collections.Generic;
using UnityEngine;

namespace SK
{
    public class NPC : MonoBehaviour
    {
        public enum NPCType
        {
            Normal, // �Ϲ� NPC
            PropShop,
            EquipmentShop
        }

        [SerializeField] private string codeName; // �ڵ� ���� ����� NPC �̸�
        [SerializeField] private string displayName; // UI�� ǥ�õ� NPC �̸�
        [SerializeField] private NPCType npctype; // NPC Ÿ��
        [SerializeField] private Collider _thisCollider; // NPC�� �ݶ��̴�
        [SerializeField] private Quests.Quest[] npcQuests; // NPC�� ����Ʈ

        [Header("Looking")]
        [SerializeField] private bool canRotating; // ��ȭ �� �÷��̾ �ٶ� �� �ִ� ���� ����
        [SerializeField] private float lookTime = 1; // �÷��̾� �ٶ󺸴� �ð�
        public Transform lookTarget;

        // ������Ƽ
        public string DisplayName => displayName;
        public NPCType NpcType => npctype;
        public IReadOnlyList<Quests.Quest> NpcQuests => npcQuests;

        private Transform _thisTransform; // Ʈ������ ĳ��
        private Transform _playerTransform; // �÷��̾� Ʈ������
        private CapsuleCollider _playerCollider; // �÷��̾��� �ݶ��̴�

        private Vector3 _rotateDirection;
        private float _dialgueAngle; // ��ȭ ���� ����
        private float _elapsed; // �÷��̾ ���� ȸ���ϴ� ��� �ð�
        private bool _isActive; // ��ȭ ���� ���� Ȱ��ȭ ����
        private bool _isRotating; // �÷��̾ ���� ȸ���������� ����

        private void Awake()        
            => _thisTransform = transform;

        private void Start()
        {
            // ���� �� �� �Ŵ����� ��ųʸ��� �߰�(Ű: �ڵ����, ��: NPC)
            SceneManager.Instance.AddNPC(codeName, this);
            // �ݶ��̴��� Ʈ���� üũ
            if (!_thisCollider.isTrigger) _thisCollider.isTrigger = true;
        }
        
        public void FixedTick()
        {
            if (!GameManager.Instance.Player) return;

            // �÷��̾� �ٿ�� �Ҵ�
            if (_playerCollider == null)
            {
                _playerCollider = GameManager.Instance.Player.thisCollider;
                _playerTransform = GameManager.Instance.Player.transform;
                _dialgueAngle = UI.UIManager.Instance.dialogManager.dialogueAngle;
            }

            #region Bounds�� �̿��� �ݶ��̴��� AABB �浹üũ
            if (_thisCollider.bounds.Intersects(_playerCollider.bounds))
            {
                // NPC�� ��ġ���� �÷��̾ ���� ����
                var dir = _thisTransform.position - _playerTransform.position;

                // �÷��̾���� ������ ��ȭ ������ ������ �Ǿ��� ��� Ȱ��ȭ
                if (Vector3.Angle(_playerTransform.forward, dir) <= _dialgueAngle)
                {
                    // Ȱ��ȭ�� �ȵǾ� �ִ� ���
                    if (!_isActive)
                    {
                        _isActive = true;

                        NpcActivate(true);
                    }
                }
                // ��ȭ ������ �������� ��� ��� ��Ȱ��ȭ
                else
                {
                    if (_isActive)
                    {
                        _isActive = false;

                        NpcActivate(false);
                    }
                }
            }
            // �ٿ�忡�� ��� ���
            else
            {
                // ��Ȱ��ȭ�� �ȵǾ� �־��ٸ� ��Ȱ��ȭ
                if (_isActive)
                {
                    _isActive = false;

                    NpcActivate(false);
                }
            }
            #endregion

            // �÷��̾ ���� ȸ��
            if (_isRotating)
            {
                _elapsed += Time.fixedDeltaTime;
                _thisTransform.rotation = Quaternion.Lerp(_thisTransform.rotation, Quaternion.LookRotation(_rotateDirection), _elapsed / lookTime);
            }
        }

        private void NpcActivate(bool isActive)
        {
            // Ȱ��ȭ�� ���
            if (isActive)
            {
                // ��Ƽ���� �ƿ����� Ȱ��ȭ


                // Dialog Manager���� NPC ��ȭ ��� �Ҵ�
                UI.UIManager.Instance.dialogManager.AssignNPC(codeName);
            }
            // ��Ȱ��ȭ�� ���
            else
            {
                // ��Ƽ���� �ƿ����� ��Ȱ��ȭ


                // Dialog Manager���� NPC ��ȭ ��� �Ҵ� ����
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
