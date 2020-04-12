using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Axis_2 : MonoBehaviour
{
    private double theta_2;
    private double d_theta2;
    void Start()
    {
        theta_2 = -45;
    }
    void Update()
    {
        theta_2 = MainCode.alpha2;
        theta_2 = -1 * theta_2;
        if (theta_2 >= -50 && theta_2 <= 170)
        {
            d_theta2 = theta_2 - transform.localEulerAngles.x;
            Quaternion originalRot = transform.rotation;
            Quaternion target = Quaternion.Euler(-1 * (float)d_theta2, 0, 0);
            transform.localEulerAngles = new Vector3(-1 * (float)theta_2 - 180, 180, 0);
        }
    }
}
