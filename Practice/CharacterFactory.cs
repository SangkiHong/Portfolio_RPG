using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SK.Practice
{

    public class CharacterFactory : MonoBehaviour
    {
        //List<Character> list;
        public void CreateObject(string name)
        {
            // 로드한 리소스를 검색하고(자료구조에 저장되어 있는 상황)
            GameObject tmp = Resources.Load<GameObject>(name);

            // 인스턴스 생성
            GameObject instance = Instantiate<GameObject>(tmp);

            //Character instanceComponent = null;

            switch (name)
            {
                case "Npc":
                    //instanceComponent = instance.AddComponent<Npc>();
                    break;
                case "Character":
                    //instanceComponent = instance.AddComponent<Character>();
                    break;
                default:
                    Debug.LogError("생성할 수 없는 게임오브젝트 입니다.");
                    break;
            }

            //if (instanceComponent == null)
            //    list.Add(instance);
        }
    }
}