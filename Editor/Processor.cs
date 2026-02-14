using System.Collections.Generic;
using nadena.dev.ndmf;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using VRC.SDK3.Dynamics.PhysBone.Components;
using com.guraril.mocheek.runtime;

[assembly: ExportsPlugin(typeof(com.guraril.mocheek.editor.Processor))]

namespace com.guraril.mocheek.editor
{
    public class Processor : Plugin<Processor>
    {
        public override string DisplayName => "MoCheek: もちもちほっぺジェネレーター";

        private bool foundTooManyBones;

        protected override void Configure()
        {
            InPhase(BuildPhase.Transforming).Run("Run MoCheek", ctx =>
            {
                MoCheek[] moCheekList = ctx.AvatarRootObject.GetComponentsInChildren<MoCheek>();
                if (moCheekList.Length < 1)
                {
                    Debug.Log("MoCheek: MoCheek設定が見つかりませんでした。");
                    return;
                }

                if (!ctx.AvatarRootObject.TryGetComponent<Animator>(out var animator))
                {
                    Debug.LogWarning("MoCheek: アバタールートにAnimatorコンポーネントが見つかりませんでした。");
                    return;
                }

                Transform headTransform = animator.GetBoneTransform(HumanBodyBones.Head);
                if (headTransform == null)
                {
                    Debug.LogWarning("MoCheek: アバターにHeadボーンが存在していないようです。");
                    return;
                }

                foreach (MoCheek moCheek in moCheekList)
                {
                    Process(moCheek, headTransform);
                    Object.DestroyImmediate(moCheek);
                }
            });
        }

        private void Process(MoCheek moCheek, Transform headTransform)
        {
            // Nullであるべきではない(MoCheekのRequiredComponentなので)
            SkinnedMeshRenderer faceMesh = moCheek.gameObject.GetComponent<SkinnedMeshRenderer>();

            // 元のメッシュを破壊しないように複製する
            Mesh mesh = Object.Instantiate(faceMesh.sharedMesh);

            var bones = new List<Transform>(faceMesh.bones);
            var bindPoses = new List<Matrix4x4>(mesh.bindposes);
            var boneWeights = mesh.boneWeights;

            foreach (var cheekBoneData in moCheek.profile.bones)
            {
                var rootBone = new GameObject(cheekBoneData.baseName);
                rootBone.transform.SetParent(headTransform, false);
                rootBone.transform.position = cheekBoneData.rootBonePosition;

                var leafBone = new GameObject(cheekBoneData.baseName + ".001");
                leafBone.transform.SetParent(rootBone.transform, false);
                leafBone.transform.position = cheekBoneData.leafBonePosition;

                bones.Add(rootBone.transform);
                bones.Add(leafBone.transform);

                bindPoses.Add(rootBone.transform.worldToLocalMatrix * faceMesh.transform.localToWorldMatrix);
                bindPoses.Add(leafBone.transform.worldToLocalMatrix * faceMesh.transform.localToWorldMatrix);

                int leafBoneIndex = bones.Count - 1;

                ApplyBoneWeights(moCheek, mesh, boneWeights, cheekBoneData, leafBoneIndex, faceMesh.transform);

                var physBone = rootBone.AddComponent<VRCPhysBone>();
                physBone.version = VRC.Dynamics.VRCPhysBoneBase.Version.Version_1_1;
                physBone.pull = 0.7f;
                physBone.spring = 0;
                physBone.immobile = 1;
                physBone.limitType = VRC.Dynamics.VRCPhysBoneBase.LimitType.Angle;
                physBone.CalcMaxAngle(5);
                physBone.allowCollision = VRC.Dynamics.VRCPhysBoneBase.AdvancedBool.True;
                physBone.radius = moCheek.profile.cheekRadius;
                physBone.maxStretch = 3;
                physBone.grabMovement = 1;
            }

            faceMesh.bones = bones.ToArray();
            mesh.bindposes = bindPoses.ToArray();
            mesh.boneWeights = boneWeights;
            faceMesh.sharedMesh = mesh;

            if (foundTooManyBones)
            {
                Debug.LogWarning("MoCheek: 1つ以上の頂点に既に4つのボーンウェイトが割り当てられていたため、一部の頂点で処理をスキップしました。");
            }
        }

        private void ApplyBoneWeights(MoCheek moCheek, Mesh mesh, BoneWeight[] boneWeights, MoCheekBones cheekBoneData, int leafBoneIndex, Transform meshTransform)
        {
            var vertices = mesh.vertices;
            var cheekWorldPos = meshTransform.TransformPoint(cheekBoneData.leafBonePosition);

            for (int i = 0; i < vertices.Length; i++)
            {
                var vertexWorldPos = meshTransform.TransformPoint(vertices[i]);
                float distance = Vector3.Distance(vertexWorldPos, cheekWorldPos);

                if (distance >= moCheek.profile.cheekRadius) continue;

                float distanceRatio = 1.0f - (distance / moCheek.profile.cheekRadius);
                float newWeight = moCheek.profile.moveWeightCurve.Evaluate(distanceRatio);

                var weights = CalculateBoneWeights(moCheek.profile.weightBlendMode, boneWeights[i], leafBoneIndex, newWeight);
                ApplyBoneWeights(weights, ref boneWeights[i]);
            }
        }

        private List<(int index, float weight)> CalculateBoneWeights(Mode weightBlendMode, BoneWeight weight, int leafBoneIndex, float newWeight)
        {
            var weights = new List<(int index, float weight)>(4);
            if (weight.weight0 > 0) weights.Add((weight.boneIndex0, weight.weight0));
            if (weight.weight1 > 0) weights.Add((weight.boneIndex1, weight.weight1));
            if (weight.weight2 > 0) weights.Add((weight.boneIndex2, weight.weight2));
            if (weight.weight3 > 0) weights.Add((weight.boneIndex3, weight.weight3));
            if (weightBlendMode == Mode.Substract)
            {
                if (weights.Count == 0)
                {
                    weights[0] = (leafBoneIndex, newWeight);
                }
                else if (weights.Count <= 3)
                {
                    float sub = newWeight / weights.Count;
                    for (int j = 0; j < weights.Count; j++) { weights[j] = (weights[j].index, weights[j].weight - sub); }
                    weights.Add((leafBoneIndex, newWeight));
                }
                else
                {
                    foundTooManyBones = true;
                }
            }
            else // Mode.SoftMix
            {
                weights.Add((leafBoneIndex, newWeight));
                float totalWeight = weights.Sum((weight) => { return weight.weight; });
                for (int j = 0; j < weights.Count; j++) { weights[j] = (weights[j].index, weights[j].weight / totalWeight); }
            }
            weights.Sort((a, b) => b.weight.CompareTo(a.weight));

            return weights;
        }

        private void ApplyBoneWeights(List<(int index, float weight)> weights, ref BoneWeight weight)
        {
            if (weights.Count >= 1)
            {
                weight.weight0 = weights[0].weight;
                weight.boneIndex0 = weights[0].index;
            }
            if (weights.Count >= 2)
            {
                weight.weight1 = weights[1].weight;
                weight.boneIndex1 = weights[1].index;
            }
            if (weights.Count >= 3)
            {
                weight.weight2 = weights[2].weight;
                weight.boneIndex2 = weights[2].index;
            }
            if (weights.Count >= 4)
            {
                weight.weight3 = weights[3].weight;
                weight.boneIndex3 = weights[3].index;
            }
        }
    }
}
