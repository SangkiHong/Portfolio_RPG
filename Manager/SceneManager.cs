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

        private void FixedUpdate()
        {
            if (Units.Count > 0)
                foreach (KeyValuePair<int, FSM.Unit> unit in Units)
                    unit.Value.FixedTick();
        }

        // Update is called once per frame
        void Update()
        {
            if(Units.Count > 0)
                foreach (KeyValuePair<int, FSM.Unit> unit in Units)
                    unit.Value.Tick();
        }

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
    }
}
