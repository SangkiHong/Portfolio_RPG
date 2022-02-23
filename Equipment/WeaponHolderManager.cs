using UnityEngine;
using System.Collections.Generic;

namespace Sangki
{
    public class WeaponHolderManager : MonoBehaviour
    {
        public WeaponHolderHook leftHook;
        public WeaponHolderHook rightHook;
        [SerializeField] private Transform sheathRight;
        [SerializeField] private Transform sheathLeft;

        private Transform targetTransform;

        public void Init()
        {
            if (!leftHook && !rightHook)
            {
                WeaponHolderHook[] weaponHolderHooks = GetComponentsInChildren<WeaponHolderHook>();
                foreach (var hook in weaponHolderHooks)
                {
                    if (hook.isLeftHook)
                        leftHook = hook;
                    else
                        rightHook = hook;
                }
            }
        }

        public void LoadWeaponOnHook(Weapon weapon, bool isLeft)
        {
            if (isLeft)
                leftHook.LoadWeaponModel(weapon);
            else
                rightHook.LoadWeaponModel(weapon);
        }

        public void Equip(int isRight)
        {
            if (isRight == 1)
            {
                targetTransform = rightHook.currentModel.transform;
                targetTransform.parent = rightHook.transform;
            }
            else
            {
                targetTransform = leftHook.currentModel.transform;
                targetTransform.parent = leftHook.transform;
            }
            
            targetTransform.localPosition = Vector3.zero;
            targetTransform.localRotation = Quaternion.identity;
            targetTransform.localScale = Vector3.one;
        }

        public void Unequip(int isRight)
        {
            if (isRight == 1)
            {
                targetTransform = rightHook.currentModel.transform;
                targetTransform.parent = sheathRight;
            }
            else
            {
                targetTransform = leftHook.currentModel.transform;
                targetTransform.parent = sheathLeft;
            }
            
            targetTransform.localPosition = Vector3.zero;
            targetTransform.localRotation = Quaternion.identity;
            targetTransform.localScale = Vector3.one;
        }
    }
}