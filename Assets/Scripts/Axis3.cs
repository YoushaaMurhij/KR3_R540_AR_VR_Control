using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Axis3 : MonoBehaviour
{
    private double theta_3;
    private double d_theta3;
    internal void Start()
    {
        theta_3 = 90;
    }
    internal void Update()
    {
        theta_3 = MainCode.alpha3;
        if (theta_3 >= -110 && theta_3 <= 155)
        {
            d_theta3 = theta_3 - transform.localEulerAngles.x;
            Quaternion originalRot = transform.rotation;
            Quaternion target = Quaternion.Euler((float)d_theta3 - 47, 0, 0);
            transform.localEulerAngles = new Vector3((float)theta_3, 0, 0);
        }
    }
}
