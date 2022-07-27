using System.Collections.Generic;
using UnityEngine;

namespace SK
{
    public class SceneManager : MonoBehaviour
    {
        // �̱���
        public static SceneManager Instance { get; private set; }

        public delegate void UpdateHandler();
        public delegate void FixedUpdateHandler();

        public Location.LocationManager locationManager;
        public Quests.QuestManager questManager;

        // ���� ��ġ�� ������ ������ ��ųʸ�(Ű: �ν��Ͻ�ID, ��: ����)
        public Dictionary<int, FSM.Unit> units = new Dictionary<int, FSM.Unit>();

        // ���� ��ġ�� NPC�� ������ ������ ��ųʸ�(Ű: npcCodeName, ��: NPC)
        public Dictionary<string, NPC> NPCs = new Dictionary<string, NPC>();

        // ������ ��� ���� ������ �ν��Ͻ� ID�� ������ ����Ʈ
        private List<Enemy> _respawnEnemy = new List<Enemy>();
        // ������ ��� ���� ������ ��� �ð��� ������ ����Ʈ
        private List<float> _respawnElapsed = new List<float>();

        public event UpdateHandler OnUpdate;
        public event FixedUpdateHandler OnFixedUpdate;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        private void FixedUpdate()
        {
            // ���� �ϰ� ������Ʈ
            if (units.Count > 0)
                foreach (KeyValuePair<int, FSM.Unit> unit in units)
                    unit.Value.FixedTick();

            // NPC �ϰ� ������Ʈ
            if (NPCs.Count > 0)
                foreach (KeyValuePair<string, NPC> npc in NPCs)
                    npc.Value.FixedTick();

            // �� �� ������Ʈ
            OnFixedUpdate?.Invoke();

            // ������ ��� ���� �ð� üũ
            if (_respawnEnemy.Count > 0)
            {
                for (int i = 0; i < _respawnEnemy.Count; i++)
                {
                    // ������ �ð� ���� ���
                    if (_respawnEnemy[i].enemyData.respawnInterval > _respawnElapsed[i])
                        _respawnElapsed[i] += Time.fixedDeltaTime;
                    // ������ �ð� ����� ���
                    else
                    {
                        // ���� ������
                        RespawnEnemy(_respawnEnemy[i]);
                        // ������ ����Ʈ���� ����
                        _respawnEnemy.RemoveAt(i);
                        _respawnElapsed.RemoveAt(i);
                    }
                }
            }
        }

        private void Update()
        {
            // ���� �ϰ� ������Ʈ
            if(units.Count > 0)
                foreach (KeyValuePair<int, FSM.Unit> unit in units)
                    unit.Value.Tick();

            // �׿� ������Ʈ
            OnUpdate?.Invoke();
        }

        #region Unit ����
        // ���� ������ ������ ��ųʸ��� �߰�
        public void AddUnit(FSM.Unit unit)
            => units.Add(unit.gameObject.GetInstanceID(), unit);

        // ���� ���� ������ ��ųʸ����� ����
        public void RemoveUnit(int instanceID)
            => units.Remove(instanceID);

        // ���� ��ųʸ����� Ű��(�ν��Ͻ�ID)�� ������ ��ȯ
        public FSM.Unit GetUnit(int instanceID) 
        {
            if (units.ContainsKey(instanceID))
                return units[instanceID];
            else
                return null;
        }
        #endregion

        #region Enemy ����
        // ���� ������ ���͸� �ڷᱸ���� �߰�
        public void AddUnit(Enemy _enemy)
        {
            // ��ųʸ��� �߰�
            var instanceID = _enemy.gameObject.GetInstanceID();
            // ���� ���� ��ųʸ��� �߰�
            units.Add(instanceID, _enemy);
        }
        // ���Ͱ� ������ ������ ����Ʈ�� �߰�
        public void AddDeadEnemy(Enemy _enemy)
        {
            var respawnInterval = _enemy.enemyData.respawnInterval;

            // ������ �ð��� 0 ���� ���� ��쿡�� ������
            if (respawnInterval > 0)
            {
                _respawnEnemy.Add(_enemy);
                _respawnElapsed.Add(0);
                units.Remove(_enemy.gameObject.GetInstanceID());
            }
        }
        // ���͸� �ٽ� ������
        private void RespawnEnemy(Enemy _enemy)
        {
            GameObject enemyObject = _enemy.gameObject;

            // ���� ������Ʈ�� ��
            enemyObject.SetActive(true);
        }
        #endregion

        #region NPC ����
        // NPC�� ��ųʸ��� �����ϴ� �Լ�
        public void AddNPC(string codeName, NPC npc)
            => NPCs.Add(codeName, npc);

        // NPC�� ������ ��ųʸ����� �����ϴ� �Լ�
        public void RemoveNPC(string codeName)
            => NPCs.Remove(codeName);

        // NPC ��ųʸ��� ����� NPC ������ �������� �Լ�
        public NPC GetNPC(string codeName) { return NPCs[codeName]; }
        #endregion

        private void OnDisable()
        {
            OnUpdate = null;
            OnFixedUpdate = null;
        }
    }
}