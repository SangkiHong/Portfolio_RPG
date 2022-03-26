using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SK
{
    public class TestManager : MonoBehaviour
    {
        [SerializeField] private int monsterAmount;
        [SerializeField] private float spawnRange;

        private void Awake()
        {
            // 플레이어 소환
            GameObject player = Resources.Load("Player") as GameObject;
            Instantiate(player, new Vector3(5, 0, 5), Quaternion.identity);

            // 몬스터 소환
            GameObject monster = Resources.Load("Monster") as GameObject;
            for (int i = 0; i < monsterAmount; i++)
            {
                // SpawnRange에 따른 랜덤 위치 소환
                var x = Random.Range(-spawnRange, spawnRange);
                var z = Random.Range(-spawnRange, spawnRange);
                Instantiate(monster, new Vector3(x, 0, z), Quaternion.identity);
            }
        }
    }
}
