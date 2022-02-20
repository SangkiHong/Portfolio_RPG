using UnityEngine;
using System.Collections.Generic;

namespace Sangki
{
    public class WeaponHolderManager : MonoBehaviour
    {
        public WeaponHolderHook leftHook;
        public WeaponHolderHook rightHook;

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
    }
}