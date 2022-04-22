using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingAlpha : MonoBehaviour
{
    List<GameObject> alphaList;
    List<GameObject> recoverList;
    public GameObject Player;

    public void Awake()
    {
        alphaList = new List<GameObject>();
        recoverList = new List<GameObject>();
    }

    // Update is called once per frame
    void LateUpdate()
    {

        // 1. 광선에 의한 카메라와 캐릭터 사이의 건물 오브젝트 검출
        Vector3 dir = Player.transform.position - Camera.main.transform.position;

        RaycastHit[] hits = Physics.RaycastAll(Camera.main.transform.position, dir.normalized);

        // 플레이어를 가리고 있는 게임오브젝트가 존재 하지 않는다면
        if(hits.Length == 0)
        {
            foreach( GameObject one in alphaList )
            {
                Color col = one.GetComponent<MeshRenderer>().material.color;
                col.a = 1f;
                one.GetComponent<MeshRenderer>().material.color = col;
            }
            alphaList.Clear();
            return;
        }

        // 2. 광선과 충돌한 게임오브젝트 전체를 리스트에 저장
        for (int i = 0; i < hits.Length; i++)
        {
            GameObject findObj = alphaList.Find(o => o == hits[i].collider.gameObject);
            if (findObj == null)
            {
                Color col = hits[i].collider.gameObject.GetComponent<MeshRenderer>().material.color;
                col.a = 0.2f;
                hits[i].collider.gameObject.GetComponent<MeshRenderer>().material.color = col;
                alphaList.Add(hits[i].collider.gameObject);

            }
        }

        // 3. 알파리스트에서 빠져나간경우 복원구현
        foreach (GameObject one in alphaList)
        {
            GameObject tmp = null;

            foreach(RaycastHit hitone in hits)
            { 
                if(one.Equals(hitone.collider.gameObject))
                {
                    tmp = hitone.collider.gameObject;
                }
            }

            if(tmp == null)
            {
               GameObject recoverObj = recoverList.Find(o => (o == one));
                if(recoverObj != null)
                {
                    continue;
                }

                Color col = one.GetComponent<MeshRenderer>().material.color;
                col.a = 1f;
                one.GetComponent<MeshRenderer>().material.color = col;
                recoverList.Add(one);
            }

        }

        // 4. 복원리스트에있는 오브젝트를 알파 리스트에서 삭제
        int size = recoverList.Count;

        for(int i= 0; i < size; i++)
        {
            // 알파리스트에서 리커버 리스트스의 게임오브젝트가 있는지 검사한다.
            GameObject findObj = alphaList.Find(o => (o.Equals(recoverList[i])));
            
            if(findObj != null)
            {
                alphaList.Remove(findObj);
            }
        }

        recoverList.Clear();
    }
}
