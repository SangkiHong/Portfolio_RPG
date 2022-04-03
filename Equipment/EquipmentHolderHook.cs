using Unity.Mathematics;
using UnityEngine;

namespace SK
{
    public class EquipmentHolderHook : MonoBehaviour
    {
        [HideInInspector]
        public GameObject currentModel;
        public Transform parentOverride;
        public bool isPrimaryHook;

        [HideInInspector] public Equipments assignedEquipment;

        public void UnloadWeapon()
        {
            if (currentModel != null)
            {
                currentModel.SetActive(false);
            }
        }

        public void UnloadWeaponAndDestroy()
        {
            if (currentModel != null)
            {
                Destroy(currentModel);
            }
        }

        public void LoadModel(Equipments loadWeapon)
        {
            if (loadWeapon == null)
            {
                UnloadWeapon();
                return;
            }

            assignedEquipment = loadWeapon;
            currentModel = Instantiate(assignedEquipment.modelPrefab);
            
            if (currentModel != null)
            {
                if (parentOverride != null)
                    currentModel.transform.parent = parentOverride;
                else
                    currentModel.transform.parent = transform;

                currentModel.transform.localPosition = assignedEquipment.modelPrefab.transform.localPosition;
                currentModel.transform.localRotation = assignedEquipment.modelPrefab.transform.localRotation;
                currentModel.transform.localScale = assignedEquipment.modelPrefab.transform.localScale;
            }
        }
    }
}