﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using Leap;
using Leap.Unity;
using Leap.Unity.Attributes;

public class Variables
{
    public static double[] xyz_ref;
    public static Mat target_pose;
    public static RoboDK.Item ROBOT;
}
public class MainCode : MonoBehaviour
{
    public LeapServiceProvider provider;
    double x, y, z, X = 0, Y = 0, Z = 0;
    double[] joints;
    double[] home_joints = { 0, -45, 80, 1, 45, 5 }; // home joints, in deg
    private double[] upperLimit = { 170f, 50f, 155f, 175f, 120f, 350f};
    private double[] lowerLimit = { -170f, -170f, -110f, -175f, -120f, -350f };
    public static int alpha1 = 0, alpha2 = -90, alpha3 = 90, alpha4 = 0, alpha5 = 0, alpha6 = 0;
    public bool LeapBOOL = true;
    public bool BO2 = true;
    int Factor_VR = 0;
    int Factor_LM = 135; //400
    public GameObject RobotBase;
    //public double[] jointValues = new double[6];
    public double[] gripperValues = {-35f, -35f, -35f};
    //private GameObject[] jointList = new GameObject[6];
    private GameObject[] gripperList = new GameObject[3];
    int extendedFingers = 0;
    bool Gripper_On = false;

    void Start()
    {
        //================================RoboDK Code==============================================
        initializeJoints();
        RoboDK RDK = new RoboDK();
        Variables.ROBOT = RDK.ItemUserPick("Select a robot", RoboDK.ITEM_TYPE_ROBOT);
        RDK.setRunMode(RoboDK.RUNMODE_SIMULATE);
        Variables.ROBOT.MoveJ(home_joints);
        Mat frame = Variables.ROBOT.PoseFrame();
        //Mat tool = Variables.ROBOT.PoseTool();
        Mat pose_ref = Variables.ROBOT.Pose();
        Variables.target_pose = Variables.ROBOT.Pose();
        Variables.xyz_ref = Variables.target_pose.Pos();
        Variables.ROBOT.MoveJ(pose_ref);
        Variables.ROBOT.setPoseFrame(frame);  // set the reference frame
        //Variables.ROBOT.setPoseTool(tool);    // set the tool frame: important for Online Programming
        Variables.ROBOT.setSpeed(500);        // Set Speed to 100 mm/s
        Variables.ROBOT.setZoneData(5);       // set the rounding instruction 
    }
    void Update()
    {
        Frame frame = provider.CurrentFrame;
        if (frame != null)
        {
           if (frame.Hands.Count > 0)
           {
               Hand hand = frame.Hands[0];
               X = hand.PalmPosition.z;
               Y = hand.PalmPosition.x;
               Z = hand.PalmPosition.y * 0.3;
               Debug.Log(hand.PalmPosition.z + "      " + hand.PalmPosition.x + "     " + hand.PalmPosition.y * 0.3);
               extendedFingers = 0;
               for (int f = 0; f < hand.Fingers.Count; f++)  
               {   //Check gripper State:
                   Finger digit = hand.Fingers [f];
                   if (digit.IsExtended) 
                   extendedFingers++;
               }
           }
        x = Variables.xyz_ref[0] + X * Factor_LM;
        y = Variables.xyz_ref[1] - Y * Factor_LM;
        z = Variables.xyz_ref[2] + Z * Factor_LM;
        // x = Variables.xyz_ref[0] + CPos.v.z * Factor_VR;
        // y = Variables.xyz_ref[1] - CPos.v.x * Factor_VR;
        // z = Variables.xyz_ref[2] + CPos.v.y * Factor_VR;
        }
        Variables.target_pose.setPos(x, y, z);
        Variables.ROBOT.MoveL(Variables.target_pose);
        joints = Variables.ROBOT.Joints();
        //print(joints[0]);
        alpha1 = (int)joints[0];
        alpha2 = (int)joints[1];
        alpha3 = (int)joints[2];
        alpha4 = (int)joints[3];
        alpha5 = (int)joints[4];
        alpha6 = (int)joints[5];

        if (extendedFingers <= 1)
        {
            Gripper_On = true; 
            for ( int i = 0; i < 3; i ++) {
                Vector3 currentRotation_gripper = gripperList[i].transform.localEulerAngles;
                //Debug.Log();
                currentRotation_gripper.x = (float)gripperValues[i];
                gripperList[i].transform.localEulerAngles = currentRotation_gripper;
            }
        }
        else if (extendedFingers >= 3)
        {
            Gripper_On = false; 
            for ( int i = 0; i < 3; i ++) {
                Vector3 currentRotation_gripper = gripperList[i].transform.localEulerAngles;
                //Debug.Log(Pitch);
                currentRotation_gripper.x = 0.0f;
                gripperList[i].transform.localEulerAngles = currentRotation_gripper;
            }
        }
        
    }

    void OnGUI() {
        int boundary = 20;
        #if UNITY_EDITOR
                int labelHeight = 20;
                GUI.skin.label.fontSize = GUI.skin.box.fontSize = GUI.skin.button.fontSize = 20;
        #else
                int labelHeight = 40;
                GUI.skin.label.fontSize = GUI.skin.box.fontSize = GUI.skin.button.fontSize = 40;
        #endif
        GUI.skin.label.alignment = TextAnchor.MiddleLeft;
        for (int i = 0; i < 6; i++) {
            GUI.Label(new Rect(boundary + 400, boundary + ( i * 2 + 1 ) * labelHeight, labelHeight * 4 + 60, labelHeight), "Joint " + (i+1) + ": " + (int)joints[i] );
            joints[i] = GUI.HorizontalSlider(new Rect(boundary+ 400 + labelHeight * 4 + 60, boundary + (i * 2 + 1) * labelHeight + labelHeight / 4, labelHeight * 5, labelHeight), (float)joints[i], (float)lowerLimit[i], (float)upperLimit[i]);
        }
    }


    void initializeJoints() {
        var RobotChildren = RobotBase.GetComponentsInChildren<Transform>();
        for (int i = 0; i < RobotChildren.Length; i++) {
            if (RobotChildren[i].name == "victor_right_gripper_fingerA_base") {
                gripperList[0] = RobotChildren[i].gameObject;
            }
            else if (RobotChildren[i].name == "victor_right_gripper_fingerB_base") {
                gripperList[1] = RobotChildren[i].gameObject;
            }
            else if (RobotChildren[i].name == "victor_right_gripper_fingerC_base") {
                gripperList[2] = RobotChildren[i].gameObject;
            }
        }
    }


    //void OnGUI()
    //{
    //    if (GUI.Button(new Rect(25, 25, 150, 30), "LeapMotion connect"))
    //    {
    //        LeapBOOL = true;
    //    }
    //    if (GUI.Button(new Rect(25, 60, 150, 30), "LeapMotion disconnect"))
    //    {
    //        // This code is executed when the Button is clicked
    //        LeapBOOL = false;
    //    }
    //}
}