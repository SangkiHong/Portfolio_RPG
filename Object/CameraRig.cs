using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sangki
{
    public class CameraRig : MonoBehaviour
    {
        [SerializeField] 
        private Transform target;
        
        [SerializeField]
        private float smoothFactor = 0.5f;

        private Transform _thisTransform;
        private Vector3 _offset;
        private Quaternion offsetRotate;

        private void Awake()
        {
            _thisTransform = this.transform;
        }

        void Start()
        {
            if (!target) target = GameObject.FindGameObjectWithTag("Player").transform;
            _offset = _thisTransform.position - _offset;
            _thisTransform.position += _thisTransform.transform.position;
            _offset = _thisTransform.position - target.position;
            offsetRotate = _thisTransform.rotation;
        }

        void LateUpdate()
        {
            _thisTransform.SetPositionAndRotation
            (Vector3.Lerp(_thisTransform.position, target.position + _offset, smoothFactor),
                Quaternion.Lerp(_thisTransform.rotation, offsetRotate, smoothFactor));
        }
    }
}
