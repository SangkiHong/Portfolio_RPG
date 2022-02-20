using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sangki
{
    public class IKHandling : MonoBehaviour
    {
        private Animator anim;

        public float ikWeight = 1;

        public Transform leftIKTarget;
        public Transform rightIKTarget;

        public Transform hintLeft;
        public Transform hintRight;
        // Start is called before the first frame update
        void Start()
        {
            anim = GetComponent<Animator>();
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        private void OnAnimatorIK()
        {
            // IK FOOT POSITION
            anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, ikWeight);
            anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, ikWeight);
            
            anim.SetIKPosition(AvatarIKGoal.LeftFoot, leftIKTarget.position);
            anim.SetIKPosition(AvatarIKGoal.RightFoot, rightIKTarget.position);
            
            // IK FOOT ROTATION
            anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, ikWeight);
            anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, ikWeight);
            
            anim.SetIKRotation(AvatarIKGoal.LeftFoot, leftIKTarget.rotation);
            anim.SetIKRotation(AvatarIKGoal.RightFoot, rightIKTarget.rotation);
            
            // IK KNEE
            anim.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, ikWeight);
            anim.SetIKHintPositionWeight(AvatarIKHint.RightKnee, ikWeight);
            
            anim.SetIKHintPosition(AvatarIKHint.LeftKnee, hintLeft.position);
            anim.SetIKHintPosition(AvatarIKHint.RightKnee, hintRight.position);
        }
    }
}
