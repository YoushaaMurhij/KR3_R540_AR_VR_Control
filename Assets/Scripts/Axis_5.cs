using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Axis_5 : MonoBehaviour
{
    private double theta_5;
    private double d_theta5;
    internal void Start()
    {
        theta_5 = 0;
    }
    internal void Update()
    {
        theta_5 = MainCode.alpha5;
        if (theta_5 >= -120 && theta_5 <= 120)
        {
            d_theta5 = theta_5 - transform.localEulerAngles.x;
            Quaternion originalRot = transform.rotation;
            transform.localEulerAngles = new Vector3((float)theta_5, 180, 0);
        }
    }
}
