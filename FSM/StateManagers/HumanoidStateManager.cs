using Unity.Collections;
using UnityEngine;

namespace Sangki
{
    public abstract class HumanoidStateManager : StateManager
    {
        [Header("References")]
        public Animator anim;
        public new Rigidbody rigidbody;
        public AnimatorHook animHook;
        public WeaponHolderManager weaponHolderManager;
        public Transform mainCamera;
        
        [Header("States")] 
        [ReadOnly] public bool isGrounded;
        [ReadOnly] public bool useRootMotion;
        [ReadOnly] public bool lockOn;
        public Transform target;

        [Header("Controller Values")] 
        [ReadOnly] public float horizontal;
        [ReadOnly] public float vertical;
        [ReadOnly] public float delta;

        [Header("Equipments")] 
        public Weapon rightWeapon;
        public Weapon leftWeapon;

        private readonly string animString_isInteracting = "isInteracting";
        
        public override void Init()
        {
            if (!anim) anim = GetComponentInChildren<Animator>();
            if (!animHook) animHook = GetComponentInChildren<AnimatorHook>();
            if (!rigidbody) rigidbody = GetComponentInChildren<Rigidbody>();
            if (!weaponHolderManager) weaponHolderManager = GetComponentInChildren<WeaponHolderManager>();
            
            anim.applyRootMotion = false;
            animHook.Init(this);

            rigidbody.angularDrag = 999;
            rigidbody.drag = 4;
            rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | 
                                    RigidbodyConstraints.FreezeRotationY |
                                    RigidbodyConstraints.FreezeRotationZ;
        }

        public void PlayerTargetAnimation(string targetAnim, bool isInteracting)
        {
            anim.SetBool(animString_isInteracting, isInteracting);
            anim.CrossFade(targetAnim, 0.2f);
        }

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
    }
}