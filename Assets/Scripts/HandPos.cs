using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.Utility;
public class HandPos : MonoBehaviour
{
    public static Vector3 v;
    Vector3 v0,pose;
    void start()
    {
        //v0 = VivePose.GetPoseEx(HandRole.RightHand).pos; // last known position of left controller
        v0 = transform.position;
    }
    void Update()
    {
        //RigidPose pose = VivePose.GetPoseEx(HandRole.RightHand);
         pose = transform.position;
        //if (ViveInput.GetPressEx(HandRole.RightHand, ControllerButton.Trigger))
        //{
        //    v = pose.pos - v0;
        //}
        v = pose - v0;
        //print(v);
    }
}
