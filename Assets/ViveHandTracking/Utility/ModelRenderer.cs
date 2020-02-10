using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ViveHandTracking {

class ModelRenderer : MonoBehaviour {
  public bool IsLeft = false;
  public GameObject Hand = null;
  public Transform[] Nodes = null;

  private Vector3 wristYDir, wristXDir, wristZDir;
  private Vector3 MwristYDir, MwristXDir, MwristZDir;
  private List<Quaternion> initialRootJointQuatertion = new List<Quaternion>();
  private List<Quaternion> initialSecondJoinQuatertion = new List<Quaternion>();
  private List<Quaternion> initialThirdJointQuatertion = new List<Quaternion>();
  private List<float> idealFingerLength = new List<float>();

  private const float epsilon = 1e-5f;

  void Awake() {
    for (int i = 0; i < 5; i++) {
      int startIndex = i * 4 + 1;
      initialRootJointQuatertion.Add(Nodes[startIndex].localRotation);
      initialSecondJoinQuatertion.Add(Nodes[startIndex + 1].localRotation);
      initialThirdJointQuatertion.Add(Nodes[startIndex + 2].localRotation);
      idealFingerLength.Add((Nodes[startIndex].position - Nodes[0].position).magnitude);
      idealFingerLength.Add((Nodes[startIndex + 1].position - Nodes[startIndex].position).magnitude);
      idealFingerLength.Add((Nodes[startIndex + 2].position - Nodes[startIndex + 1].position).magnitude);
    }

    Hand.SetActive(false);
  }

  IEnumerator Start() {
    while (GestureProvider.Status == GestureStatus.NotStarted)
      yield return null;
    if (!GestureProvider.HaveSkeleton)
      this.enabled = false;
  }

  void Update() {
    if (!GestureProvider.UpdatedInThisFrame)
      return;

    GestureResult result = IsLeft ? GestureProvider.LeftHand : GestureProvider.RightHand;
    if (result == null) {
      Hand.SetActive(false);
      return;
    }
    Hand.SetActive(true);
    for (int i = 0; i < 5; i++) {
      int startIndex = i * 4 + 1;
      float squareLengthRoot = (result.points[startIndex] - result.points[0]).sqrMagnitude;
      float squareLengthFirst = (result.points[startIndex + 1] - result.points[startIndex]).sqrMagnitude;
      float squareLengthSecond = (result.points[startIndex + 2] - result.points[startIndex + 1]).sqrMagnitude;
      float squareLengthThird = (result.points[startIndex + 3] - result.points[startIndex + 2]).sqrMagnitude;
      if (squareLengthRoot < epsilon || squareLengthFirst < epsilon || squareLengthSecond < epsilon ||
          squareLengthThird < epsilon)
        return;
      for (int j = i + 1; j < 5; j++) {
        int nextStartIndex = j * 4 + 1;
        if ((result.points[nextStartIndex] - result.points[startIndex]).sqrMagnitude == 0)
          return;
      }
    }
    Vector3 _Wrist = result.points[0];
    Vector3 _Index = result.points[5];
    Vector3 _Middle = result.points[9];
    Vector3 _ring = result.points[13];

    transform.position = _Wrist;
    Vector3 _MidDir = _Middle - _Wrist;
    Vector3 _ringDir = _ring - _Wrist;
    Vector3 _IndexDir = _Index - _Wrist;

    Vector3 _PalmDir = Vector3.Cross(_IndexDir, _ringDir);

    Vector3 _UpDir = Vector3.Cross(_ringDir, _PalmDir);
    _MidDir = Quaternion.AngleAxis(4.7f, _PalmDir) * _MidDir;
    transform.rotation = Quaternion.LookRotation(_MidDir, _UpDir);

    setFingerPos(result);
    setAxis(result);
    adjustFingerRootPos(result);
    for (int i = 0; i < 5; ++i)
      SetFingerAngle(result, i);
    for (int i = 0; i < 5; i++) {
      int startIndex = i * 4 + 1;
      Nodes[startIndex].position = Nodes[0].position + (Nodes[startIndex].position - Nodes[0].position).normalized *
                                   idealFingerLength[i * 3];
      Nodes[startIndex + 1].position = Nodes[startIndex].position + (Nodes[startIndex + 1].position -
                                       Nodes[startIndex].position).normalized * idealFingerLength[i * 3 + 1];
      Nodes[startIndex + 2].position = Nodes[startIndex + 1].position + (Nodes[startIndex + 2].position - Nodes[startIndex +
                                       1].position).normalized * idealFingerLength[i * 3 + 2];
    }
  }

  private void ConfirmIsCoplanar(int fingerIndex) {
    int startIndex = fingerIndex * 4 + 1;
    Vector3 tempVec0 = Nodes[startIndex + 2].position - Nodes[startIndex].position;
    Vector3 tempVec1 = Nodes[startIndex + 3].position - Nodes[startIndex + 2].position;
    if (tempVec0.normalized == tempVec1.normalized)
      return;
    Vector3 tempVec2 = Nodes[startIndex + 1].position - Nodes[startIndex].position;
    Vector3 ploneNormal = Vector3.Cross(tempVec0, tempVec1);
    float angle = SignedAngle(tempVec2, ploneNormal);
    Nodes[startIndex].rotation = Quaternion.AngleAxis(angle - 90, Vector3.Cross(tempVec2,
                                 ploneNormal)) * Nodes[startIndex].rotation;
    Nodes[startIndex + 1].rotation = Quaternion.AngleAxis(90 - angle, Vector3.Cross(tempVec2,
                                     ploneNormal)) * Nodes[startIndex + 1].rotation;
    Nodes[startIndex + 2].rotation = Quaternion.AngleAxis(90 - angle, Vector3.Cross(tempVec2,
                                     ploneNormal)) * Nodes[startIndex + 2].rotation;
  }

  private void adjustFingerRootPos(GestureResult hand) {
    for (int i = 0; i < 5; i++) {
      if (i == 1 || i == 2)
        continue;
      int startIndex = i * 4 + 1;
      Vector3 vec1 = hand.points[startIndex] - hand.points[0];
      Vector3 vec2 = Nodes[startIndex].position - Nodes[0].position;
      if (Vector3.Cross(wristXDir, vec1).magnitude > epsilon) {
        Vector3 vec3;
        float angle;
        calculateModelPos(wristXDir, vec1, MwristXDir, vec2, out angle, out vec3);
        Nodes[startIndex].position = Nodes[0].position + vec3.normalized * (Nodes[startIndex].position -
                                     Nodes[0].position).magnitude;
      }
    }
  }

  private void setFingerPos(GestureResult hand) {
    for (int i = 0; i < 5; i++) {
      int startIndex = i * 4 + 1;
      float IdealLength = (hand.points[startIndex] - hand.points[0]).magnitude;
      Nodes[startIndex].position = Nodes[0].position + (Nodes[startIndex].position - Nodes[0].position).normalized *
                                   IdealLength;
      IdealLength = (hand.points[startIndex + 1] - hand.points[startIndex]).magnitude;
      Nodes[startIndex + 1].position = Nodes[startIndex].position + (Nodes[startIndex + 1].position -
                                       Nodes[startIndex].position).normalized * IdealLength;
      IdealLength = (hand.points[startIndex + 2] - hand.points[startIndex + 1]).magnitude;
      Nodes[startIndex + 2].position = Nodes[startIndex + 1].position + (Nodes[startIndex + 2].position - Nodes[startIndex +
                                       1].position).normalized * IdealLength;
      IdealLength = (hand.points[startIndex + 3] - hand.points[startIndex + 2]).magnitude;
      Nodes[startIndex + 3].position = Nodes[startIndex + 2].position + (Nodes[startIndex + 3].position - Nodes[startIndex +
                                       2].position).normalized * IdealLength;
    }
  }

  /// fingerIndex is ranged from 0 to 4, i.e. from thumb to pinky
  private void SetFingerAngle(GestureResult hand, int fingerIndex) {
    int startIndex = fingerIndex * 4 + 1;
    if (fingerIndex == 0) {
      float toPinkyLength = (hand.points[startIndex + 2] - hand.points[17]).magnitude;
      float toRingLength = (hand.points[startIndex + 2] - hand.points[13]).magnitude;
      float toMiddleLength = (hand.points[startIndex + 2] - hand.points[9]).magnitude;
      float toIndexLength = (hand.points[startIndex + 2] - hand.points[5]).magnitude;
      if (toPinkyLength < 0.028f || toRingLength < 0.028f || toMiddleLength < 0.028f || toIndexLength < 0.028f)
        return;
    }

    Vector3 root = hand.points[startIndex];
    Vector3 joint1 = hand.points[startIndex + 1];
    Vector3 joint2 = hand.points[startIndex + 2];
    Vector3 top = hand.points[startIndex + 3];

    Vector3 vec0 = root - hand.points[0];
    Vector3 vec1 = joint1 - root;
    Vector3 vec2 = joint2 - joint1;
    Vector3 vec3 = top - joint2;

    Nodes[startIndex].localRotation = initialRootJointQuatertion[fingerIndex];
    Nodes[startIndex + 1].localRotation = initialSecondJoinQuatertion[fingerIndex];
    Nodes[startIndex + 2].localRotation = initialThirdJointQuatertion[fingerIndex];

    setRotation(vec0, vec1, 0.0f, Vector3.zero, startIndex, hand);
    setRotation(vec1, vec2, 0.0f, Vector3.zero, startIndex + 1, hand);

    float angle = Mathf.Clamp(Vector3.Angle(vec2, vec3), 0, 180);
    Nodes[startIndex + 2].localEulerAngles = new Vector3(initialThirdJointQuatertion[fingerIndex].eulerAngles.x,
        initialThirdJointQuatertion[fingerIndex].eulerAngles.y, -angle);

    ConfirmIsCoplanar(fingerIndex);
  }

  float[] GaussFunction(float[,] a, int n) {
    int i, j, k;
    int rank;
    float temp, l, s;
    float[] x = new float[n];
    for (i = 0; i <= n - 2; i++) {
      rank = i;
      for (j = i + 1; j <= n - 1; j++)
        if (a[j, i] > a[i, i])
          rank = j;
      for (k = 0; k <= n; k++) {
        temp = a[i, k];
        a[i, k] = a[rank, k];
        a[rank, k] = temp;
      }
      for (j = i + 1; j <= n - 1; j++) {
        l = a[j, i] / a[i, i];
        for (k = i; k <= n; k++)
          a[j, k] = a[j, k] - l * a[i, k];
      }
    }
    x[n - 1] = a[n - 1, n] / a[n - 1, n - 1];
    for (i = n - 2; i >= 0; i--) {
      s = 0;
      for (j = i + 1; j <= n - 1; j++)
        s = s + a[i, j] * x[j];
      x[i] = (a[i, n] - s) / a[i, i];
    }
    return x;
  }

  void calculateModelPos(Vector3 v0, Vector3 v1, Vector3 Mv0, Vector3 Mv1, out float angle, out Vector3 vec) {
    angle = SignedAngle(v0, v1);
    vec = Vector3.Cross(v0, v1).normalized;
    float a1 = Vector3.Dot(wristXDir, vec);
    float a2 = Vector3.Dot(wristYDir, vec);
    float a3 = Vector3.Dot(wristZDir, vec);
    float[,] arr = { {  MwristXDir.x, MwristXDir.y, MwristXDir.z, a1  },
      { MwristYDir.x, MwristYDir.y, MwristYDir.z, a2  },
      { MwristZDir.x, MwristZDir.y, MwristZDir.z, a3},
    };
    float[] result = new float[3];
    result = GaussFunction(arr, 3);

    vec = new Vector3(result[0], result[1], result[2]);
    vec = Quaternion.AngleAxis(angle, vec) * Mv0;
    angle = SignedAngle(Mv1, vec);
  }

  void setAxis(GestureResult hand) {
    wristXDir = (-Vector3.Cross(hand.points[5] - hand.points[0], hand.points[9] - hand.points[0])).normalized;
    wristYDir = (Vector3.Cross(wristXDir, (hand.points[9] + hand.points[5]) / 2 - hand.points[0])).normalized;
    wristZDir = ((hand.points[9] + hand.points[5]) / 2 - hand.points[0]).normalized;
    MwristXDir = (-Vector3.Cross(Nodes[5].position - Nodes[0].position,
                                 Nodes[9].position - Nodes[0].position)).normalized;
    MwristYDir = (Vector3.Cross(MwristXDir,
                                (Nodes[9].position + Nodes[5].position) / 2 - Nodes[0].position)).normalized;
    MwristZDir = ((Nodes[9].position + Nodes[5].position) / 2 - Nodes[0].position).normalized;
  }

  void setRotation(Vector3 vec0, Vector3 vec1, float angle, Vector3 vec2, int joint, GestureResult hand) {
    Vector3 vecM0 = Vector3.zero;
    Vector3 vecM1 = Vector3.zero;
    if (joint % 4 == 1)
      vecM0 = Nodes[joint].position - Nodes[0].position;
    else
      vecM0 = Nodes[joint].position - Nodes[joint - 1].position;

    vecM1 = Nodes[joint + 1].position - Nodes[joint].position;

    calculateModelPos(vec0, vec1, vecM0, vecM1, out angle, out vec2);

    Nodes[joint].rotation = Quaternion.AngleAxis(angle, Vector3.Cross(vecM1, vec2)) * Nodes[joint].rotation;
  }

  private static float SignedAngle(Vector3 v1, Vector3 v2) {
#if UNITY_2017_1_OR_NEWER
    return Vector3.SignedAngle(v1, v2, Vector3.Cross(v1, v2));
#else
    return Vector3.Angle(v1, v2);
#endif
  }
}

}
