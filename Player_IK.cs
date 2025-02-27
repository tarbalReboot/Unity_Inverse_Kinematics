﻿// Often in a 3D game, characters walk or run on slopes. Dynamic colliders on feet are not practical or useful,
// so inverse kinematics is often utilized to place footing appropriately. Otherwise, one foot would be in the
// air, and the other would be below the surface when on a slope.

// Set a boolean variable in your player controller with an animation trigger, such that this script runs only
// when the boolean is true. That way, IK can be turned on and off as the character is running or walking,
// such that it is only activated when the characters foot in on or near the surface.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_IK : MonoBehaviour
{
    private Player_Control player_script;

    [SerializeField] private Animator foot_anim;
    [SerializeField] private Transform left_foot;
    [SerializeField] private Transform right_foot;
    [SerializeField] private Transform left_knee;
    [SerializeField] private CapsuleCollider capsule_collider;
    private Vector3 ref_point;
    private RaycastHit hit;

    private float foot_ray()
    {
	    // raycast must ignore the layer of the player character and any colliders associated with that layer
        if (Physics.Raycast(capsule_collider.bounds.center, -Vector3.up, out hit, 20f))
        {
            ref_point = hit.point;
	    // Add the following game objects in mechanim skeletal hierarchy to script in inspector (left_foot, right_foot, & left_knee).
	    // The three points are required for the mathematical operations below.
	    // Normal vector of the surface plane is calculated, so that feet can be rotated and positioned appropriately.
            Plane plane = new Plane(left_foot.transform.position, right_foot.transform.position, left_knee.transform.position);
            Vector3 projection = Vector3.ProjectOnPlane(hit.normal, plane.normal);
            Vector3 projection2 = Vector3.ProjectOnPlane(gameObject.transform.up, plane.normal);
            return Vector3.SignedAngle(projection, projection2, plane.normal);
        }
        else
        {
            return 0f;
        }
    }

    private void OnAnimatorIK(int layerIndex)

    {
        if (player_script.IsGrounded())
        {
            float final_angle = foot_ray() * (3.1416f / 180f);

            foot_anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0.9f);
            foot_anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0.9f);
            foot_anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0.9f);
            foot_anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0.9f);

            float half_dist_between_feet = (left_foot.transform.position - right_foot.transform.position).magnitude / 2f;
            float left_pos = ref_point.y + 0.075f + (Mathf.Sin(final_angle) * half_dist_between_feet);
            float right_pos = ref_point.y + 0.075f - (Mathf.Sin(final_angle) * half_dist_between_feet);

	    // Set foot position (auto).
            foot_anim.SetIKPosition(AvatarIKGoal.LeftFoot, new Vector3(left_foot.transform.position.x, left_pos, left_foot.transform.position.z));
            foot_anim.SetIKPosition(AvatarIKGoal.RightFoot, new Vector3(right_foot.transform.position.x, right_pos, right_foot.transform.position.z));

            Vector3 slopeCorrected = -Vector3.Cross(hit.normal, gameObject.transform.right);

            if (slopeCorrected != hit.normal)
            {
                Quaternion leftFootRot = Quaternion.LookRotation(slopeCorrected, hit.normal);
                Quaternion rightFootRot = Quaternion.LookRotation(slopeCorrected, hit.normal);

		// Set foot rotation (auto).
                foot_anim.SetIKRotation(AvatarIKGoal.LeftFoot, leftFootRot);
                foot_anim.SetIKRotation(AvatarIKGoal.RightFoot, rightFootRot);
            }
        }
    }

    private void Start()
    {
        player_script = gameObject.GetComponent<Player_Control>();
    }
}
