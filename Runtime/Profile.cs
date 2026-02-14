using UnityEngine;

namespace com.guraril.mocheek.runtime
{
    [System.Serializable]
    public struct MoCheekBones
    {
        public string baseName;
        public Vector3 rootBonePosition;
        public Vector3 leafBonePosition;
    }

    [System.Serializable]
    public enum Mode
    {
        Substract,  // ほっぺのボーンを除く影響する全てのボーンのウェイトを均等に引き算します。
        Normalize // ほっぺのボーンを含む影響する全てのボーンの影響度を良い感じにノーマライズします。
    }

    [System.Serializable]
    public class Profile
    {
        public MoCheekBones[] bones;
        [Range(0f, 180f)]
        public float angleLimit = 30f;
        [Range(0f, 0.2f)]
        public float cheekRadius = 0.025f;
        public AnimationCurve moveWeightCurve = new(new Keyframe[] { new(0, 0.2f), new(1, 0.3f) });
        public Mode weightBlendMode = Mode.Normalize;
    }
}
