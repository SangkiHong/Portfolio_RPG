using UnityEngine;
using System.Linq;
using SK.FSM;

namespace SK
{
    public abstract class CharacterStateManager : StateManager
    {
        [Header("References")]
        public Animator anim;
        public new Rigidbody rigidbody;
        public AnimatorHook animHook;
        public EquipmentHolderManager equipmentHolderManager;
        public Transform mainCamera;

        [Header("States")] 
        public bool isGrounded;
        public bool useRootMotion;
        public bool lockOn;
        public bool isDead;

        [Header("Controller Values")] 
        public float horizontal;
        public float vertical;
        public float delta;

        [Header("Equipments")] 
        public Equipments primaryEquipment;
        public Equipments secondaryEquipment;
        [System.NonSerialized]
        public Equipments currentWeaponInUse;
        
        [Header("Targeting")] 
        public Transform target;
        [SerializeField] 
        private LayerMask targetLayer;
        private Collider[] _targetColliders;
        
        
        private readonly string animString_isInteracting = "isInteracting";
        
        public override void Init()
        {
            if (!anim) anim = GetComponentInChildren<Animator>();
            if (!animHook) animHook = GetComponentInChildren<AnimatorHook>();
            if (!rigidbody) rigidbody = GetComponentInChildren<Rigidbody>();
            if (!equipmentHolderManager) equipmentHolderManager = GetComponentInChildren<EquipmentHolderManager>();
            
            anim.applyRootMotion = false;
            animHook.Init(this);

            rigidbody.angularDrag = 999;
            rigidbody.drag = 4;
            rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | 
                                    RigidbodyConstraints.FreezeRotationY |
                                    RigidbodyConstraints.FreezeRotationZ;
            
            _targetColliders = new Collider[5];
        }

        public void PlayerTargetAnimation(string targetAnim, bool isInteracting)
        {
            anim.SetBool(animString_isInteracting, isInteracting);
            anim.CrossFade(targetAnim, 0.2f);
        }

        #region Lock System
        public virtual void OnAssignLookOverride(Transform target)
        {
            this.target = target;
            if (target != null)
                lockOn = true;
        }

        public virtual void OnClearLookOverride()
        {
            lockOn = false;
        }

        public Transform FindLockableTarget()
        {
            if (Physics.OverlapSphereNonAlloc(mTransform.position, 15, _targetColliders, targetLayer) > 0)
            {
                _targetColliders
                    .Where(x => Vector3.Distance(mTransform.position, x.transform.position) < 30)
                    .OrderBy(x => Vector3.Distance(mTransform.position, x.transform.position));

                if (_targetColliders.Length > 0)
                    return _targetColliders[0].transform;
            }
            
            return null;
        }
        #endregion

        public void AssignCurrentWeapon(Equipments weapon) => currentWeaponInUse = weapon;

        public void HandleDamageCollider(bool status)
        {
            if (currentWeaponInUse == null) return;
            
            if(currentWeaponInUse.GetType() == typeof(Weapon))
                ((Weapon)currentWeaponInUse).weaponHook.DamageColliderStatus(status);
        }
    }
}