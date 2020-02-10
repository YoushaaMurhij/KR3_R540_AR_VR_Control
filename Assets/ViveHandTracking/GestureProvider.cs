using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
#if UNITY_2018_3_OR_NEWER
using UnityEngine.Android;
#endif

namespace ViveHandTracking {

// This is a the main class used for gesture detection. It handles start/stop of the detection
// automatically once the script is enabled. GestureProvider is a singleton class, make sure there
// is one and only one instance in the whole scene. It provides several static readonly properties
// about detection status and result.
public class GestureProvider : MonoBehaviour {
#if VIVEHANDTRACKING_WITH_WAVEVR || VIVEHANDTRACKING_WITH_GOOGLEVR
  private static string[] permissionNames = { "android.permission.CAMERA" };
#endif

  // Get detection result of left hand. Returns null if left hand is not detected.
  public static GestureResult LeftHand {
    get;
    private set;
  }
  // Get detection result of right hand. Returns null if right hand is not detected.
  public static GestureResult RightHand {
    get;
    private set;
  }
  // Returns running status of gesture detection.
  public static GestureStatus Status {
    get;
    private set;
  }
  // Returns detailed error if Status is Error
  public static GestureFailure Error {
    get;
    private set;
  }
  // Returns true if LeftHand and RightHand are updated in this frame. This can be useful for
  // calculating states that only depends on hand result. Since detection normally have slower FPS
  // than VR rendering, it saves computation power if these states are only updated when hand
  // results change. Always return false if detection is not running.
  public static bool UpdatedInThisFrame {
    get;
    private set;
  }
  // Returns the current singleton (or null if no instance exists).
  public static GestureProvider Current {
    get;
    private set;
  }
  // Current running mode for detection, value is only valid if Status is Starting or Running.
  public static GestureMode Mode {
    get;
    private set;
  }
  // A shortcut for checking skeleton mode. Equivalent to Status == Skeleton.
  public static bool HaveSkeleton {
    get;
    private set;
  }
  private bool initialized = false;

  [SerializeField]
  private GestureOption option;
  internal int lastIndex = -1;

  void Awake() {
    if (Current != null) {
      Debug.LogWarning("Only one GestureProvider is allowed in the scene.");
      GameObject.Destroy(this);
      return;
    }
    Current = this;
    ClearState();
    Mode = option.mode;
    HaveSkeleton = Mode == GestureMode.Skeleton;
  }

  void OnEnable() {
    if (initialized)
      StartGestureDetection();
  }

  IEnumerator Start () {
    Screen.sleepTimeout = SleepTimeout.NeverSleep;

#if UNITY_ANDROID
#if VIVEHANDTRACKING_WITH_WAVEVR
    // setup wavevr rendering
    if (transform.GetComponent<WaveVR_Render>() == null) {
      Destroy(transform.GetComponent<AudioListener>());
      Destroy(transform.GetComponent<FlareLayer>());
      gameObject.AddComponent<WaveVR_Render>();
      var tracker = gameObject.AddComponent<WaveVR_DevicePoseTracker>();
#if VIVEHANDTRACKING_WITH_WAVEVR3
      tracker.type = WaveVR_Controller.EDeviceType.Head;
#else
      tracker.type = wvr.WVR_DeviceType.WVR_DeviceType_HMD;
#endif
    }
    yield return WaitForReady();

    // get camera permission
    var pmInstance = WaveVR_PermissionManager.instance;
    while (!pmInstance.isInitialized())
      yield return null;

    bool granting = true;
    bool hasPermission = pmInstance.isPermissionGranted(permissionNames[0]);
    WaveVR_PermissionManager.requestCompleteCallback callback = (results) => {
      granting = false;
      if (results.Count > 0 && results[0].Granted)
        hasPermission = true;
    };
    while (!hasPermission) {
      granting = true;
      pmInstance.requestPermissions(permissionNames, callback);
      while (granting)
        yield return null;
    }
#elif VIVEHANDTRACKING_WITH_GOOGLEVR
    // get camera permission using daydream API
    if (UnityEngine.VR.VRSettings.loadedDeviceName == "daydream") {
      var permissionRequester = GvrPermissionsRequester.Instance;
      bool granting = true;
      Action<GvrPermissionsRequester.PermissionStatus[]> callback = (results) => granting = false;
      while (!permissionRequester.IsPermissionGranted(permissionNames[0])) {
        granting = true;
        permissionRequester.RequestPermissions(permissionNames, callback);
        while (granting)
          yield return null;
      }
    }
#elif UNITY_2018_3_OR_NEWER
    // Unity 2018.3 or newer adds support for android runtime permission
    while (!Permission.HasUserAuthorizedPermission(Permission.Camera)) {
      Permission.RequestUserPermission(Permission.Camera);
      yield return null;
    }
#else
    while (!Application.HasUserAuthorization(UserAuthorization.WebCam))
      yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
#endif
#endif
    GestureInterface.UseExternalTransform(true);

    // start detection
    StartGestureDetection();
    initialized = true;
    yield break;
  }

  void Update () {
    if (Status == GestureStatus.NotStarted || Status == GestureStatus.Error)
      return;
    GestureInterface.SetCameraTransform(transform.position, transform.rotation);

    UpdatedInThisFrame = false;
    IntPtr ptr;
    int index;
    var size = GestureInterface.GetGestureResult(out ptr, out index);

    if (index < 0) {
#if VIVEHANDTRACKING_WITH_WAVEVR
      WaveVR_Log.Log.e("Aristo", "Gesture detection stopped");
#else
      Debug.LogError("Gesture detection stopped");
#endif
      Status = GestureStatus.Error;
      Error = GestureFailure.Internal;
      return;
    } else if (Status == GestureStatus.Starting && index > 0)
      Status = GestureStatus.Running;
    if (index <= lastIndex)
      return;
    lastIndex = index;
    UpdatedInThisFrame = true;

    LeftHand = RightHand = null;
    if (size <= 0)
      return;

    var structSize = Marshal.SizeOf(typeof(GestureResultRaw));
    for (var i = 0; i < size; i++) {
      var gesture = (GestureResultRaw)Marshal.PtrToStructure(ptr, typeof(GestureResultRaw));
      ptr = new IntPtr(ptr.ToInt64() + structSize);
      if (gesture.isLeft)
        LeftHand = new GestureResult(gesture);
      else
        RightHand = new GestureResult(gesture);
    }
  }

  void ClearState() {
    Status = GestureStatus.NotStarted;
    UpdatedInThisFrame = false;
    Error = GestureFailure.None;
    LeftHand = RightHand = null;
    lastIndex = -1;
  }

  void StartGestureDetection() {
    if (Status == GestureStatus.Starting || Status == GestureStatus.Running)
      return;
    Error = GestureInterface.StartGestureDetection(option);
    if (Error != GestureFailure.None) {
#if VIVEHANDTRACKING_WITH_WAVEVR
      WaveVR_Log.Log.e("Aristo", "Start gesture detection failed: " + Error);
#else
      Debug.LogError("Start gesture detection failed: " + Error);
#endif
      Status = GestureStatus.Error;
    } else {
      Mode = option.mode;
      HaveSkeleton = Mode == GestureMode.Skeleton;
      Status = GestureStatus.Starting;
    }
  }

  void StopGestureDetection() {
    GestureInterface.StopGestureDetection();
    ClearState();
  }

  void OnDisable() {
    StopGestureDetection();
  }

  void OnDestroy() {
    StopGestureDetection();
    Current = null;
  }

  IEnumerator WaitForReady() {
#if VIVEHANDTRACKING_WITH_WAVEVR
    // Wait a few frames until WaveVR API is fully ready after startup/sleep
    for (int i = 0; i < 10; i++)
      yield return null;
#else
    yield return null;
#endif
  }

  IEnumerator RestartDetection() {
    yield return WaitForReady();
    StartGestureDetection();
  }

  void OnApplicationPause(bool isPaused) {
    if (isPaused)
      StopGestureDetection();
    else if (initialized)
      StartCoroutine(RestartDetection());
  }

  void OnApplicationQuit() {
    StopGestureDetection();
  }
}

}
