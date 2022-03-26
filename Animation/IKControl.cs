using UnityEngine;

namespace SK.Animation
{
    [RequireComponent(typeof(Animator))]
    public class IKControl : MonoBehaviour
    {
        [SerializeField] private Animator anim;
        [SerializeField] private bool ikActive;
        [SerializeField] private LayerMask groundLayerMask;
        [SerializeField] [Range(0f, 1f)] 
        private float distanceToGround = 0.05f;
        [SerializeField] private Transform leftFoot;
        [SerializeField] private Transform rightFoot;
        void Start()
        {
            if (!anim) anim = GetComponent<Animator>();
        }
        private void OnAnimatorIK(int layerIndex)
        {
            if (ikActive)
            {
                anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
                anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
                anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
                anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);

                // Left Foot
                RaycastHit hit;
                Ray ray = new Ray(anim.GetIKPosition(AvatarIKGoal.LeftFoot) + Vector3.up, Vector3.down);
                if (Physics.Raycast(ray, out hit, distanceToGround + 1f, groundLayerMask))
                {
                    Vector3 footPos = hit.point;
                    footPos.y += distanceToGround;
                    anim.SetIKPosition(AvatarIKGoal.LeftFoot, footPos);
                    anim.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.LookRotation(transform.forward, hit.normal));
                }

                // Right Foot
                ray = new Ray(anim.GetIKPosition(AvatarIKGoal.RightFoot) + Vector3.up, Vector3.down);
                if (Physics.Raycast(ray, out hit, distanceToGround + 1f, groundLayerMask))
                {
                    Vector3 footPos = hit.point;
                    footPos.y += distanceToGround;
                    anim.SetIKPosition(AvatarIKGoal.RightFoot, footPos);
                    anim.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.LookRotation(transform.forward, hit.normal));
                }
            }
            else
            {
                anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0);
                anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0);
                anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);
                anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0);
                anim.SetLookAtWeight(0);
            }
        }
    }
}
