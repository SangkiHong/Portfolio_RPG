using System.Collections;
using UnityEngine;

namespace SK.Assets.Scripts.Practice
{
    public class TestPlayer : MonoBehaviour
    {
        [SerializeField] private float speed;
        [SerializeField] private float rotateSpeed;

        private Transform _transform;
        private Vector3 _destPos, _dir;
        private RaycastHit _hit;

        void Start()
        {
            _transform = transform;
            _destPos = _transform.position;
        }
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out _hit, 100))
                {
                    // 플레이어 클릭시 콘솔창에 이름 출력
                    if (_hit.collider.CompareTag("Player"))
                    { 
                        Debug.Log(_hit.collider.name);
                        return; // 플레이어 클릭 시 이동 하지 않도록 return
                    }
                    _destPos = _hit.point;
                    _destPos.y = _transform.position.y;                
                }
            }

            if (_transform.position != _destPos)
            {
                // MoveToward로 목표지점 이동
                var pos = _transform.position;
                _transform.position = Vector3.MoveTowards(pos, _destPos, Time.deltaTime * speed);

                // 목적지를 향해 Rotate
                _dir = _destPos - pos;
                _transform.rotation = Quaternion.Lerp(_transform.rotation, Quaternion.LookRotation(_dir.normalized), Time.deltaTime * rotateSpeed);
            }
        }
    }
}