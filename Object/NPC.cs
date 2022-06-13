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

        [SerializeField] private string codeName;
        [SerializeField] private NPCType npctype;
        [SerializeField] private Collider _thisCollider;
        [SerializeField] private Quests.Quest[] npcQuests;

        public NPCType NpcType => npctype;
        public IReadOnlyList<Quests.Quest> NpcQuests => npcQuests;

        private Transform _thisTransform;
        private Transform _playerTransform;
        private CapsuleCollider _playerCollider;

        private float _dialgueAngle;
        private bool _onBoundsPlayer, _isActive;

        private void Awake()        
            => _thisTransform = transform;        

        // ���� �� �� �Ŵ����� ��ųʸ��� �߰�(Ű: �ڵ����, ��: NPC)
        private void Start()       
            => SceneManager.Instance.AddNPC(codeName, this);
        
        public void FixedTick()
        {
            if (!GameManager.Instance.Player) return;

            // �÷��̾� �ٿ�� �Ҵ�
            if (_playerCollider == null)
            {
                _playerCollider = GameManager.Instance.Player.thisCollider;
                _playerTransform = GameManager.Instance.Player.transform;
                _dialgueAngle = GameManager.Instance.UIManager.dialogManager.dialogueAngle;
            }

            // Bounds�� �̿��� �ݶ��̴��� AABB �浹üũ
            if (_thisCollider.bounds.Intersects(_playerCollider.bounds))
                _onBoundsPlayer = true;
            else
                _onBoundsPlayer = false;

            // �÷��̾��� �ݶ��̴��� �浹�� ���
            if (_onBoundsPlayer)
            {
                // NPC�� ��ġ���� �÷��̾ ���� ����
                var dir = _playerTransform.position - _thisTransform.position;

                // �÷��̾���� ������ ��ȭ ������ ������ �Ǿ��� ��� Ȱ��ȭ
                if (Vector3.Angle(_thisTransform.forward, dir) <= _dialgueAngle)
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
        }

        private void NpcActivate(bool isActive)
        {
            // Ȱ��ȭ�� ���
            if (isActive)
            {
                // ��Ƽ���� �ƿ����� Ȱ��ȭ


                // Dialog Manager���� NPC ��ȭ ��� �Ҵ�
                GameManager.Instance.UIManager.dialogManager.AssignNPC(codeName);
            }
            // ��Ȱ��ȭ�� ���
            else
            {
                // ��Ƽ���� �ƿ����� ��Ȱ��ȭ


                // Dialog Manager���� NPC ��ȭ ��� �Ҵ� ����
                GameManager.Instance.UIManager.dialogManager.UnassignNPC();
            }
        }
    }
}
