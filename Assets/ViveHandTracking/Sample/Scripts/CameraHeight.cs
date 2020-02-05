using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ViveHandTracking.Sample {

class CameraHeight : MonoBehaviour {
  void Awake() {
#if UNITY_ANDROID
#if VIVEHANDTRACKING_WITH_WAVEVR && !UNITY_EDITOR
    if (wvr.Interop.WVR_GetWaveSDKVersion() <= 3) // version before WaveVR SDK 3.1
#endif
      // increase camera height by 1.5m on android, since they assume camera height starts at 0m
      transform.position = new Vector3(0, 1.5f, 0);
#endif
    GameObject.Destroy(this);
  }
}

}
