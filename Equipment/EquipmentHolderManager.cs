﻿using UnityEngine;
using System.Collections.Generic;

namespace SK
{
    public class EquipmentHolderManager : MonoBehaviour
    {
        public EquipmentHolderHook primaryHook;
        public EquipmentHolderHook secondaryHook;
        [SerializeField] private Transform sheathPrimary;
        [SerializeField] private Transform sheathSecondary;

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
        }

        public void LoadEquipmentOnHook(Equipments equipment, bool isPrimary)
        {
            if (isPrimary)
                primaryHook.LoadModel(equipment);
            else
                secondaryHook.LoadModel(equipment);
        }

        public void Equip(int isPrimary)
        {
            if (isPrimary == 1)
            {
                targetTransform = primaryHook.currentModel.transform;
                targetTransform.parent = primaryHook.transform;
            }
            else
            {
                targetTransform = secondaryHook.currentModel.transform;
                targetTransform.parent = secondaryHook.transform;
            }
            
            targetTransform.localPosition = Vector3.zero;
            targetTransform.localRotation = Quaternion.identity;
            targetTransform.localScale = Vector3.one;
        }

        public void Unequip(int isPrimary)
        {
            if (isPrimary == 1)
            {
                targetTransform = primaryHook.currentModel.transform;
                targetTransform.parent = sheathPrimary;
            }
            else
            {
                targetTransform = secondaryHook.currentModel.transform;
                targetTransform.parent = sheathSecondary;
            }
            
            targetTransform.localPosition = Vector3.zero;
            targetTransform.localRotation = Quaternion.identity;
            targetTransform.localScale = Vector3.one;
        }
    }
}