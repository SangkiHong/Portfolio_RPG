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

            // 디버깅 레이
            Debug.DrawRay(cameraTranform.position, dir.normalized * dist, Color.red);

            RaycastHit[] hits = Physics.RaycastAll(cameraTranform.position, dir.normalized, dist, hitLayerMask);
            if (hits.Length > 0)
            {
                // alphaList에 새롭게 추가
                for (int i = 0; i < hits.Length; i++)
                {
                    // alphaList에 있을 경우 Pass
                    if (alphaList.Find(x => x.transform == hits[i].transform))
                        continue;

                    bool match = false;
                    // recoverList에 있을 경우 alphaList로 이동
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

                    // recoverList에서 가져온 경우 새로 추가 하지 않음
                    if (match) continue;

                    MeshRenderer tmpRenderer = hits[i].transform.GetComponent<MeshRenderer>();
                    if (tmpRenderer != null)
                        alphaList.Add(tmpRenderer); 
                }

                // alphaList에 hits에 포함되지 않은 것은 recoverList로 전달
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

                    // 포함이 안된 경우 recoverList로 이동
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
                // 중간에 오브젝트가 없을 경우 alphaList의 오브젝트를 모두 recoverList로 보냄
                if (alphaList.Count > 0)
                {
                    for (int i = 0; i < alphaList.Count; i++)
                    {
                        recoverList.Add(alphaList[i]);
                    }

                    // alphaList 비우기
                    alphaList.Clear();
                }
            }

            // 알파 값 감소
            if (alphaList.Count > 0)
            {
                for (int i = 0; i < alphaList.Count; i++)
                {
                    _tmpColor = alphaList[i].material.color;
                    
                    // 0.2까지 알파를 낮춤
                    if (_tmpColor.a <= 0.2f) continue;
                    
                    _tmpColor.a -= Time.deltaTime * fadeSpeed * 1.5f;
                    alphaList[i].material.color = _tmpColor;
                }
            }

            // 알파 값 상승
            if (recoverList.Count > 0)
            {
                for (int i = 0; i < recoverList.Count; i++)
                {
                    _tmpColor = recoverList[i].material.color;
                    _tmpColor.a += Time.deltaTime * fadeSpeed;
                    recoverList[i].material.color = _tmpColor;

                    // 알파 값 1이 될 시 recoverList에서 제외
                    if (_tmpColor.a >= 1)
                        recoverList.Remove(recoverList[i]);
                }
            }
        }
    }
}
