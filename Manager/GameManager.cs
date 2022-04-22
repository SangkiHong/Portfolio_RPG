﻿using UnityEngine;
using SK.FSM;
namespace SK
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        [SerializeField] private bool cursorVisible;

        public bool CusorVisible => cursorVisible;

        public PlayerStateManager player;
        
        private void Awake()
        {
            Instance = this;

            // 씬 정보 테이블로부터 읽어들인 데이터에 맞추어 리소스를 로드하여 배치
            GameObject p = ResourceManager.Instance.LoadPlayerCharacter();
            p.name = "Player";

            Cursor.visible = cursorVisible;
            if (!cursorVisible)
                Cursor.lockState = CursorLockMode.Locked;
        }

        public bool SwitchMouseState()
        {
            cursorVisible = !cursorVisible;
            Cursor.visible = cursorVisible;

            if (cursorVisible)
                Cursor.lockState = CursorLockMode.Confined;
            else
                Cursor.lockState = CursorLockMode.Locked;

            return !cursorVisible; // Unvisible 상태 시 카메라 회전 가능하기 위해 반전 값 리턴
        }
    }
}