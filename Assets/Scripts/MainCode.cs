using System.Collections;
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
    double x, y, z, X = 0, Y = 0, Z = 0, Yaw =0, Pitch= 0, Roll = 0;
    double[] joints;
    double[] home_joints = { 0, -45, 80, 1, 45, 5 }; // home joints, in deg
    private double[] upperLimit = { 170f, 50f, 155f, 175f, 120f, 350f};
    private double[] lowerLimit = { -170f, -170f, -110f, -175f, -120f, -350f };
    public static int alpha1 = 0, alpha2 = -90, alpha3 = 90, alpha4 = 0, alpha5 = 0, alpha6 = 0;
    public bool LeapBOOL = true;
    public bool BO2 = true;
    int Factor_VR = 50;
    int Factor_LM = 220; //400
    public GameObject RobotBase , Camera_Rig;
    private GameObject Right_Controller;

    //public double[] jointValues = new double[6];
    public double[] gripperValues = {-35f, -35f, -35f};
    //private GameObject[] jointList = new GameObject[6];
    private GameObject[] gripperList = new GameObject[3];
    int extendedFingers = 0;
    bool Gripper_On = false;
    bool Leap_On = false;
    bool VR_controllers_On = true;
    double[] VR_Init_Pos = {0.0f, 0.0f, 0.0f};

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
        VR_Init_Pos[0] = Right_Controller.transform.position.z;
        VR_Init_Pos[1] = Right_Controller.transform.position.x;
        VR_Init_Pos[2] = Right_Controller.transform.position.y;  
    }
    void Update()
    {
        if(Leap_On)
        {
            Frame frame = provider.CurrentFrame;
            if (frame != null)
            {
                if (frame.Hands.Count > 0)
                {
                    Hand hand = frame.Hands[0];
                    X = hand.PalmPosition.z * 0.6;
                    Y = hand.PalmPosition.x;
                    Z = hand.PalmPosition.y;
                    Roll = hand.Rotation.x * 180 * 7 / 22;
                    Pitch = hand.Rotation.y * 180 * 7 / 22;
                    Yaw = hand.Rotation.z * 180 * 7 / 22; 
                    Debug.Log(hand.PalmPosition.z + "      " + hand.PalmPosition.x + "     " + hand.PalmPosition.y * 0.3);
                    extendedFingers = 0;
                    for (int f = 0; f < hand.Fingers.Count; f++)  
                    {   //Check gripper State:
                        Finger digit = hand.Fingers [f];
                        if (digit.IsExtended) 
                        extendedFingers++;
                    }
                }
            
            }
            x = Variables.xyz_ref[0] + X * Factor_LM;
            y = Variables.xyz_ref[1] - Y * Factor_LM;
            z = Variables.xyz_ref[2] + Z * Factor_LM;
        }
        else if (VR_controllers_On)
        {
            X = Right_Controller.transform.position.z - VR_Init_Pos[0];
            Y = Right_Controller.transform.position.x - VR_Init_Pos[1];
            Z = Right_Controller.transform.position.y - VR_Init_Pos[2];
            Roll  = Right_Controller.transform.rotation.x * 180 * 7 / 22;
            Pitch = Right_Controller.transform.rotation.y * 180 * 7 / 22;
            Yaw   = Right_Controller.transform.rotation.z * 180 * 7 / 22;
            x = Variables.xyz_ref[0] + X * Factor_VR;
            y = Variables.xyz_ref[1] - Y * Factor_VR;
            z = Variables.xyz_ref[2] + Z * Factor_VR;

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
        alpha6 = (int)joints[5] + (int)Pitch;

        if (extendedFingers <= 1)
        {
            Gripper_On = true; 
            for ( int i = 0; i < 3; i ++) {
                Vector3 currentRotation_gripper = gripperList[i].transform.localEulerAngles;
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
        var Controller_Obj = Camera_Rig.GetComponentsInChildren<Transform>();
        for (int i = 0; i < Controller_Obj.Length; i++) {
            if (Controller_Obj[i].name == "RightHand") {
                Right_Controller = Controller_Obj[i].gameObject;
                break;
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