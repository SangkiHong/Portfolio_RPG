using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SK
{
    public class TEMP_AttackInitialize : MonoBehaviour
    {
        [SerializeField] private Behavior.Attack[] attacks;

        private void Awake()
        {
            foreach (var attack in attacks)
            {
                attack.animName = attack.name;
            }
        }
    }
}
