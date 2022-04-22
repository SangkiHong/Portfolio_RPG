using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SK
{
    public class AlphaController : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private float fadeSpeed;
        [SerializeField] private LayerMask hitLayerMask;
        [SerializeField] private List<MeshRenderer> alphaList = new List<MeshRenderer>();
        [SerializeField] private List<MeshRenderer> recoverList = new List<MeshRenderer>();

        private Transform cameraTranform;
        private Color _tmpColor;
        private void Awake()
        {
            cameraTranform = Camera.main.transform;
        }

        private void LateUpdate()
        {
            var dir = player.position - cameraTranform.position;
            var dist = Vector3.Distance(player.position, cameraTranform.position);

            // ����� ����
            Debug.DrawRay(cameraTranform.position, dir.normalized * dist, Color.red);

            RaycastHit[] hits = Physics.RaycastAll(cameraTranform.position, dir.normalized, dist, hitLayerMask);
            if (hits.Length > 0)
            {
                // alphaList�� ���Ӱ� �߰�
                for (int i = 0; i < hits.Length; i++)
                {
                    // alphaList�� ���� ��� Pass
                    if (alphaList.Find(x => x.transform == hits[i].transform))
                        continue;

                    bool match = false;
                    // recoverList�� ���� ��� alphaList�� �̵�
                    if (recoverList.Count > 0)
                    {
                        for (int j = 0; j < recoverList.Count; j++)
                        {
                            if (recoverList[j].transform == hits[i].transform)
                            {
                                alphaList.Add(recoverList[j]);
                                recoverList.Remove(recoverList[j]);
                                --j;
                                match = true;
                            }
                        }
                    }

                    // recoverList���� ������ ��� ���� �߰� ���� ����
                    if (match) continue;

                    MeshRenderer tmpRenderer = hits[i].transform.GetComponent<MeshRenderer>();
                    if (tmpRenderer != null)
                        alphaList.Add(tmpRenderer); 
                }

                // alphaList�� hits�� ���Ե��� ���� ���� recoverList�� ����
                for (int i = 0; i < alphaList.Count; i++)
                {
                    bool match = false;
                    for (int j = 0; j < hits.Length; j++)
                    {
                        if (hits[j].transform == alphaList[i].transform)
                        {
                            match = true;
                            break;
                        }
                    }

                    // ������ �ȵ� ��� recoverList�� �̵�
                    if (!match)
                    { 
                        recoverList.Add(alphaList[i]);
                        alphaList.Remove(alphaList[i]);
                        --i;
                    }
                }
            }
            else
            {
                // �߰��� ������Ʈ�� ���� ��� alphaList�� ������Ʈ�� ��� recoverList�� ����
                if (alphaList.Count > 0)
                {
                    for (int i = 0; i < alphaList.Count; i++)
                    {
                        recoverList.Add(alphaList[i]);
                    }

                    // alphaList ����
                    alphaList.Clear();
                }
            }

            // ���� �� ����
            if (alphaList.Count > 0)
            {
                for (int i = 0; i < alphaList.Count; i++)
                {
                    _tmpColor = alphaList[i].material.color;
                    
                    // 0.2���� ���ĸ� ����
                    if (_tmpColor.a <= 0.2f) continue;
                    
                    _tmpColor.a -= Time.deltaTime * fadeSpeed * 1.5f;
                    alphaList[i].material.color = _tmpColor;
                }
            }

            // ���� �� ���
            if (recoverList.Count > 0)
            {
                for (int i = 0; i < recoverList.Count; i++)
                {
                    _tmpColor = recoverList[i].material.color;
                    _tmpColor.a += Time.deltaTime * fadeSpeed;
                    recoverList[i].material.color = _tmpColor;

                    // ���� �� 1�� �� �� recoverList���� ����
                    if (_tmpColor.a >= 1)
                        recoverList.Remove(recoverList[i]);
                }
            }
        }
    }
}
