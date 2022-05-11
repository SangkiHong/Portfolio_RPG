using UnityEngine;
using System.Collections.Generic;

namespace SK
{
    public class EquipmentHolderManager : MonoBehaviour
    {
        public EquipmentHolderHook primaryHook;
        public EquipmentHolderHook secondaryHook;
        [SerializeField] private Transform sheathPrimary;
        [SerializeField] private Transform sheathSecondary;

        private bool isEquipPrimary, isEquipSecondary;
        private Transform targetTransform;

        public void Init()
        {
            if (!secondaryHook && !primaryHook)
            {
                EquipmentHolderHook[] equipmentHolderHooks = GetComponentsInChildren<EquipmentHolderHook>();
                foreach (var hook in equipmentHolderHooks)
                {
                    if (hook.isPrimaryHook)
                        secondaryHook = hook;
                    else
                        primaryHook = hook;
                }
            }

            // Subscribe Event
            GetComponent<Health>().onDead += UnloadEquipment;
        }

        public void LoadEquipmentOnHook(Equipments equipment, bool isPrimary)
        {
            if (isPrimary)
                primaryHook.LoadModel(equipment);
            else
                secondaryHook.LoadModel(equipment);
        }

        public void UnloadEquipment()
        {
            primaryHook?.UnloadWeapon();
            secondaryHook?.UnloadWeapon();
        }

        public void Equip(int isPrimary = 0)
        {
            if (isPrimary == 0) // 주무기
            {
                if (!isEquipPrimary)
                {
                    isEquipPrimary = true;
                    targetTransform = primaryHook.currentModel.transform;
                    targetTransform.parent = primaryHook.transform;
                }
            }
            else // 보조 무기
            {
                if (!isEquipSecondary)
                {
                    isEquipSecondary = true;
                    targetTransform = secondaryHook.currentModel.transform;
                    targetTransform.parent = secondaryHook.transform;
                }
            }
            
            targetTransform.localPosition = Vector3.zero;
            targetTransform.localRotation = Quaternion.identity;
            targetTransform.localScale = Vector3.one;
        }

        public void Unequip(int isPrimary = 0)
        {
            if (isPrimary == 0)
            {
                if (isEquipPrimary)
                {
                    isEquipPrimary = false;
                    targetTransform = primaryHook.currentModel.transform;
                    targetTransform.parent = sheathPrimary;
                }
            }
            else
            {
                if (isEquipSecondary)
                {
                    isEquipSecondary = false;
                    targetTransform = secondaryHook.currentModel.transform;
                    targetTransform.parent = sheathSecondary;
                }
            }
            
            targetTransform.localPosition = Vector3.zero;
            targetTransform.localRotation = Quaternion.identity;
            targetTransform.localScale = Vector3.one;
        }
    }
}