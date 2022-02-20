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
    }
}