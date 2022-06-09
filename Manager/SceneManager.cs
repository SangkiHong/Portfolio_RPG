using System.Collections.Generic;
using UnityEngine;

namespace SK
{
    public class SceneManager : MonoBehaviour
    {
        // �̱���
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

        // ���� ��ġ�� ������ ������ ��ųʸ�(Ű: �ν��Ͻ�ID, ��: ����)
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

        // ���� ������ ������ ��ųʸ��� �߰��ϴ� �Լ�
        public void AddUnit(FSM.Unit unit)
            => Units.Add(unit.gameObject.GetInstanceID(), unit);

        // ���� ���� ������ ��ųʸ����� �����ϴ� �Լ�
        public void RemoveUnit(int instanceID)
            => Units.Remove(instanceID);

        // ���� ��ųʸ����� Ű��(�ν��Ͻ�ID)�� ������ ��ȯ
        public FSM.Unit GetUnit(int instanceID) 
        {
            if (Units.ContainsKey(instanceID))
                return Units[instanceID];
            else
                return null;
        }
    }
}
