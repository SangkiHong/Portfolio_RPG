using Unity.Mathematics;
using UnityEngine;

namespace Sangki
{
    public class WeaponHolderHook : MonoBehaviour
    {
        public Transform parentOverride;
        public bool isLeftHook;

        private GameObject _currentModel;

        public void UnloadWeapon()
        {
            if (_currentModel != null)
            {
                _currentModel.SetActive(false);
            }
        }

        public void UnloadWeaponAndDestroy()
        {
            if (_currentModel != null)
            {
                Destroy(_currentModel);
            }
        }

        public void LoadWeaponModel(Weapon weaponModel)
        {
            if (weaponModel == null)
            {
                UnloadWeapon();
                return;
            }
            
            GameObject model = Instantiate(weaponModel.modelPrefab) as GameObject;
            if (model != null)
            {
                if (parentOverride != null)
                {
                    model.transform.parent = parentOverride;
                }
                else
                {
                    model.transform.parent = this.transform;
                }

                model.transform.localPosition = weaponModel.modelPrefab.transform.localPosition;
                model.transform.localRotation = weaponModel.modelPrefab.transform.localRotation;
                model.transform.localScale = weaponModel.modelPrefab.transform.localScale;
            }

            _currentModel = model;
        }
    }
}