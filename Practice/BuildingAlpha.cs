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

        // 1. ������ ���� ī�޶�� ĳ���� ������ �ǹ� ������Ʈ ����
        Vector3 dir = Player.transform.position - Camera.main.transform.position;

        RaycastHit[] hits = Physics.RaycastAll(Camera.main.transform.position, dir.normalized);

        // �÷��̾ ������ �ִ� ���ӿ�����Ʈ�� ���� ���� �ʴ´ٸ�
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

        // 2. ������ �浹�� ���ӿ�����Ʈ ��ü�� ����Ʈ�� ����
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

        // 3. ���ĸ���Ʈ���� ����������� ��������
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

        // 4. ��������Ʈ���ִ� ������Ʈ�� ���� ����Ʈ���� ����
        int size = recoverList.Count;

        for(int i= 0; i < size; i++)
        {
            // ���ĸ���Ʈ���� ��Ŀ�� ����Ʈ���� ���ӿ�����Ʈ�� �ִ��� �˻��Ѵ�.
            GameObject findObj = alphaList.Find(o => (o.Equals(recoverList[i])));
            
            if(findObj != null)
            {
                alphaList.Remove(findObj);
            }
        }

        recoverList.Clear();
    }
}
