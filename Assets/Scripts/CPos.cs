using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.Utility;
public class CPos : MonoBehaviour
{
    public static Vector3 v;
    Vector3 v0;
    void awake()
    {
        v0 = VivePose.GetPoseEx(HandRole.RightHand).pos; // last known position of left controller
    }
    void Update()
    {
        RigidPose pose = VivePose.GetPoseEx(HandRole.RightHand);
        if (ViveInput.GetPressEx(HandRole.RightHand, ControllerButton.Trigger))
        {
            v = pose.pos - v0;
        }
    }
}
