using UnityEngine;
using VRC.SDKBase;

namespace com.guraril.mocheek.runtime
{
    [AddComponentMenu("GuraRil/MoCheek")]
    [RequireComponent(typeof(SkinnedMeshRenderer))]
    public class MoCheek : MonoBehaviour, IEditorOnly
    {
        public Profile profile;
        private void OnDrawGizmos()
        {
            if (profile.bones == null) { return; }
            for (int i = 0; i < profile.bones.Length; i++)
            {
                Gizmos.DrawWireSphere(transform.TransformPoint(profile.bones[i].leafBonePosition), profile.cheekRadius);
            }
        }
    }
}
