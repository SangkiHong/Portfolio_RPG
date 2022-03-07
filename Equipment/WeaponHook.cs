using System;
using UnityEngine;

namespace SK
{
    public class WeaponHook : MonoBehaviour
    {
        public GameObject damageCollider;

        private void Start()
        {
            if (damageCollider.activeInHierarchy) damageCollider.SetActive(false);
        }

        public void DamageColliderStatus(bool status) => damageCollider.SetActive(status);
    }
}