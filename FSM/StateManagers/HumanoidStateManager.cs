using  UnityEngine;

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
        public bool isGrounded;
        public bool useRootMotion;
        public bool lockOn;
        public Transform target;

        [Header("Controller Values")] 
        public float horizontal;
        public float vertical;
        public float delta;
        public Vector3 rootMovement;

        [Header("Equipments")] 
        public Weapon rightWeapon;
        public Weapon leftWeapon;
        
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
            anim.SetBool("isInteracting", isInteracting);
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