using System.Collections;
using UnityEngine;

namespace SK.Assets.Scripts.Practice
{
    public class TestCamera : MonoBehaviour
    {
        [SerializeField] private float speed;

        private Transform _transform, _player;
        private Vector3 offset;

        private void Awake()
        {
            _transform = transform;
        }

        private void Start()
        {
            _player = GameObject.FindGameObjectWithTag("Player").transform;

            offset = _transform.position - _player.position;
        }

        private void Update()
        {
            // 부드러운 카메라 이동
            _transform.position = Vector3.Lerp(_transform.position, _player.position + offset, Time.deltaTime * speed);
        }
    }
}