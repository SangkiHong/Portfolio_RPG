using System.Collections.Generic;
using UnityEngine;

namespace SK
{
    public class SceneManager : MonoBehaviour
    {
        // 싱글톤
        private static SceneManager _instance;
        public static SceneManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<SceneManager>();

                return _instance;
            }
        }

        // 씬에 배치된 유닛을 저장할 딕셔너리(키: 인스턴스ID, 값: 유닛)
        public Dictionary<int, FSM.Unit> Units = new Dictionary<int, FSM.Unit>();
        
        // 씬에 배치된 NPC의 정보를 저장할 딕셔너리(키: npcCodeName, 값: NPC)
        public Dictionary<string, NPC> NPCs = new Dictionary<string, NPC>();

        private void FixedUpdate()
        {
            // 유닛 일괄 업데이트
            if (Units.Count > 0)
                foreach (KeyValuePair<int, FSM.Unit> unit in Units)
                    unit.Value.FixedTick();

            // NPC 일괄 업데이트
            if (NPCs.Count > 0)
                foreach (KeyValuePair<string, NPC> npc in NPCs)
                    npc.Value.FixedTick();
        }

        // Update is called once per frame
        void Update()
        {
            if(Units.Count > 0)
                foreach (KeyValuePair<int, FSM.Unit> unit in Units)
                    unit.Value.Tick();
        }

        #region Unit 관련
        // 씬에 관리할 유닛을 딕셔너리에 추가하는 함수
        public void AddUnit(FSM.Unit unit)
            => Units.Add(unit.gameObject.GetInstanceID(), unit);

        // 관리 중인 유닛을 딕셔너리에서 제거하는 함수
        public void RemoveUnit(int instanceID)
            => Units.Remove(instanceID);

        // 유닛 딕셔너리에서 키값(인스턴스ID)의 유닛을 반환
        public FSM.Unit GetUnit(int instanceID) 
        {
            if (Units.ContainsKey(instanceID))
                return Units[instanceID];
            else
                return null;
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
    }
}