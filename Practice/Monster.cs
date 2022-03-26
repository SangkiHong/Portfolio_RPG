using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityStandardAssets.Cameras;

namespace SK
{
    public class Monster : MonoBehaviour
    {
        [SerializeField] private float rotateSpeed = 5;
        [SerializeField] private LayerMask groundLayer;
        public Collider thisCollider;
        
        private Transform _transform;
        private Transform _player;
        private Vector3 _dir;
        
        void Awake()
        {
            _transform = transform;
            _player = GameObject.FindGameObjectWithTag("Player").transform;
            thisCollider = GetComponent<Collider>();
        }

        private void Update()
        {
            if (_player)
            {
                var temp = _player.position;
                var thisPos = _transform.position;
                temp.y = thisPos.y;
                _dir = temp - thisPos;

                var newDir = Vector3.RotateTowards(_transform.forward, _dir.normalized,
                    Time.deltaTime * rotateSpeed, 0.2f);
                _transform.forward = newDir.normalized;
                
                //_transform.rotation = Quaternion.Lerp(_transform.rotation, Quaternion.LookRotation(_dir),
                //    Time.deltaTime * rotateSpeed);

                //_transform.forward = _dir.normalized;
            }
        }

        private Vector3 OnGround(Vector3 pos)
        {
            if (Physics.Raycast(pos, Vector3.down, out  RaycastHit hit, 10, groundLayer))
            {
                return hit.point;
            }

            return Vector3.zero;
        }
    }
}
