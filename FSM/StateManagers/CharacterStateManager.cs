using UnityEngine;
using System.Linq;
using Cinemachine;
using SK.Behavior;

namespace SK.FSM
{
    public abstract class CharacterStateManager : StateManager
    {
        [Header("References")]
        public Transform mainCamera;
        public CinemachineFreeLook normalCamera;
        public CinemachineFreeLook lockOnCamera;
        public CinemachineImpulseSource impulseSource;
        public Animator anim;
        //public Rigidbody thisRigidbody;
        public CapsuleCollider thisCollider;
        public AnimatorHook animHook;
        public Combat combat;
        public Health health;

        [Header("States")] 
        public bool isGrounded;
        public bool useRootMotion;
        public bool lockOn;
        public bool isDead;

        [Header("Controller Values")] 
        public float horizontal;
        public float vertical;
        public float fixedDelta;

        [Header("Targeting")]
        [SerializeField] private float targetSearchRange = 20;
        [SerializeField] private LayerMask targetLayer;
        internal Transform targetEnemy;
        private Collider[] _targetColliders;
        
        public override void Init()
        {
            if (!anim) anim = GetComponentInChildren<Animator>();
            if (!animHook) animHook = GetComponentInChildren<AnimatorHook>();
            //if (!thisRigidbody) thisRigidbody = GetComponentInChildren<Rigidbody>(); //deprecated::Don't use Rigidbody
            if (!impulseSource) impulseSource = GetComponent<CinemachineImpulseSource>();
            if (!combat) combat = GetComponent<Combat>();
            if (!health) health = GetComponent<Health>();
            
            anim.applyRootMotion = false;
            animHook.Init(this);

            //deprecated::Don't use Rigidbody
            /*thisRigidbody.angularDrag = 999;
            thisRigidbody.drag = 4;
            thisRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | 
                                        RigidbodyConstraints.FreezeRotationY |
                                        RigidbodyConstraints.FreezeRotationZ;*/

            _targetColliders = new Collider[5];
        }

        public void PlayerTargetAnimation(string targetAnim, bool isInteracting)
        {
            anim.SetBool(Strings.animPara_isInteracting, isInteracting);
            anim.CrossFade(targetAnim, 0.15f);
        }

        #region Lock System
        public void OnAssignLookOverride(Transform lockTarget)
        {
            targetEnemy = lockTarget;
            if (lockTarget == null) return;

            lockOn = true;
            normalCamera.gameObject.SetActive(false);
            lockOnCamera.gameObject.SetActive(true);
            lockOnCamera.m_LookAt = lockTarget;
        }

        public void OnClearLookOverride()
        {
            lockOn = false;
            targetEnemy = null;
            normalCamera.gameObject.SetActive(true);
            lockOnCamera.gameObject.SetActive(false);
        }

        public Transform FindLockableTarget()
        {
            if (Physics.OverlapSphereNonAlloc(mTransform.position, targetSearchRange, _targetColliders, targetLayer) > 0)
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
    }
}