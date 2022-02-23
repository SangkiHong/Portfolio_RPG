using System;
using UnityEngine;

namespace Sangki
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private bool cursorVisible;
        
        private void Awake()
        {
            Cursor.visible = cursorVisible;
            Cursor.lockState = CursorLockMode.Locked;
        }
        Vector3 vStart;
        Vector3 vEnd = Vector3.zero;
        Vector3 vDir = Vector3.zero;
        
        void Start()
        {
            vStart = new Vector3(-3, 2, 4);
            vEnd = new Vector3(10, 10, 10);
            vDir = vEnd - vStart;

            vStart = this.gameObject.transform.position;
            vDir = vEnd - vStart;
        }
    }
}