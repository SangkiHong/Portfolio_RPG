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
            GameObject model = Instantiate(assignedEquipment.modelPrefab);
            
            if (assignedEquipment.GetType() == typeof(Weapon))
                ((Weapon)assignedEquipment).weaponHook = model.GetComponent<WeaponHook>();
            
            if (model != null)
            {
                if (parentOverride != null)
                    model.transform.parent = parentOverride;
                else
                    model.transform.parent = transform;
                
                model.transform.localPosition = assignedEquipment.modelPrefab.transform.localPosition;
                model.transform.localRotation = assignedEquipment.modelPrefab.transform.localRotation;
                model.transform.localScale = assignedEquipment.modelPrefab.transform.localScale;
            }

            currentModel = model;
        }
    }
}