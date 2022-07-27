using System.Collections.Generic;
using UnityEngine;

namespace SK
{
    public class SceneManager : MonoBehaviour
    {
        // 싱글톤
        public static SceneManager Instance { get; private set; }

        public delegate void UpdateHandler();
        public delegate void FixedUpdateHandler();

        public Location.LocationManager locationManager;
        public Quests.QuestManager questManager;

        // 씬에 배치된 유닛을 저장할 딕셔너리(키: 인스턴스ID, 값: 유닛)
        public Dictionary<int, FSM.Unit> units = new Dictionary<int, FSM.Unit>();

        // 씬에 배치된 NPC의 정보를 저장할 딕셔너리(키: npcCodeName, 값: NPC)
        public Dictionary<string, NPC> NPCs = new Dictionary<string, NPC>();

        // 리스폰 대기 중인 몬스터의 인스턴스 ID를 저장할 리스트
        private List<Enemy> _respawnEnemy = new List<Enemy>();
        // 리스폰 대기 중인 몬스터의 경과 시간을 저장할 리스트
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
            // 유닛 일괄 업데이트
            if (units.Count > 0)
                foreach (KeyValuePair<int, FSM.Unit> unit in units)
                    unit.Value.FixedTick();

            // NPC 일괄 업데이트
            if (NPCs.Count > 0)
                foreach (KeyValuePair<string, NPC> npc in NPCs)
                    npc.Value.FixedTick();

            // 그 외 업데이트
            OnFixedUpdate?.Invoke();

            // 리스폰 대기 몬스터 시간 체크
            if (_respawnEnemy.Count > 0)
            {
                for (int i = 0; i < _respawnEnemy.Count; i++)
                {
                    // 리스폰 시간 전인 경우
                    if (_respawnEnemy[i].enemyData.respawnInterval > _respawnElapsed[i])
                        _respawnElapsed[i] += Time.fixedDeltaTime;
                    // 리스폰 시간 경과한 경우
                    else
                    {
                        // 몬스터 리스폰
                        RespawnEnemy(_respawnEnemy[i]);
                        // 리스폰 리스트에서 삭제
                        _respawnEnemy.RemoveAt(i);
                        _respawnElapsed.RemoveAt(i);
                    }
                }
            }
        }

        private void Update()
        {
            // 유닛 일괄 업데이트
            if(units.Count > 0)
                foreach (KeyValuePair<int, FSM.Unit> unit in units)
                    unit.Value.Tick();

            // 그외 업데이트
            OnUpdate?.Invoke();
        }

        #region Unit 관련
        // 씬에 관리할 유닛을 딕셔너리에 추가
        public void AddUnit(FSM.Unit unit)
            => units.Add(unit.gameObject.GetInstanceID(), unit);

        // 관리 중인 유닛을 딕셔너리에서 제거
        public void RemoveUnit(int instanceID)
            => units.Remove(instanceID);

        // 유닛 딕셔너리에서 키값(인스턴스ID)의 유닛을 반환
        public FSM.Unit GetUnit(int instanceID) 
        {
            if (units.ContainsKey(instanceID))
                return units[instanceID];
            else
                return null;
        }
        #endregion

        #region Enemy 관련
        // 씬에 관리할 몬스터를 자료구조에 추가
        public void AddUnit(Enemy _enemy)
        {
            // 딕셔너리에 추가
            var instanceID = _enemy.gameObject.GetInstanceID();
            // 유닛 관리 딕셔너리에 추가
            units.Add(instanceID, _enemy);
        }
        // 몬스터가 죽으면 리스폰 리스트에 추가
        public void AddDeadEnemy(Enemy _enemy)
        {
            var respawnInterval = _enemy.enemyData.respawnInterval;

            // 리스폰 시간이 0 보다 많은 경우에만 리스폰
            if (respawnInterval > 0)
            {
                _respawnEnemy.Add(_enemy);
                _respawnElapsed.Add(0);
                units.Remove(_enemy.gameObject.GetInstanceID());
            }
        }
        // 몬스터를 다시 리스폰
        private void RespawnEnemy(Enemy _enemy)
        {
            GameObject enemyObject = _enemy.gameObject;

            // 몬스터 오브젝트를 켬
            enemyObject.SetActive(true);
        }
        #endregion

        #region NPC 관련
        // NPC를 딕셔너리에 저장하는 함수
        public void AddNPC(string codeName, NPC npc)
            => NPCs.Add(codeName, npc);

        // NPC의 정보를 딕셔너리에서 삭제하는 함수
        public void RemoveNPC(string codeName)
            => NPCs.Remove(codeName);

        // NPC 딕셔너리에 저장된 NPC 정보를 가져오는 함수
        public NPC GetNPC(string codeName) { return NPCs[codeName]; }
        #endregion

        private void OnDisable()
        {
            OnUpdate = null;
            OnFixedUpdate = null;
        }
    }
}