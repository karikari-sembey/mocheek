using System;
using com.guraril.mocheek.runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace com.guraril.mocheek.editor
{
    [CustomEditor(typeof(MoCheek))]
    class MoCheekInspector : Editor
    {
        [SerializeField] private VisualTreeAsset visualTreeAsset;
        private bool drawGizmo = false;

        public override VisualElement CreateInspectorGUI()
        {
            MoCheek self = target as MoCheek;

            VisualElement root = new();
            root.Add(visualTreeAsset.Instantiate());

            Button import = root.Q<Button>("Import");
            Button export = root.Q<Button>("Export");
            Toggle drawGizmoToggle = root.Q<Toggle>("DrawGizmoToggle");

            import.clicked += () =>
            {
                SkinnedMeshRenderer targetMesh = self.gameObject.GetComponent<SkinnedMeshRenderer>();
                self.profile = ProfileManager.LoadProfile(EditorUtility.OpenFilePanel("MoCheekプロファイルを選択", "./Assets", "mocheek"));
            };

            export.clicked += () =>
            {
                ProfileManager.SaveProfile(self.profile, EditorUtility.SaveFilePanel("MoCheekプロファイルを保存", "./Asset", "profile", "mocheek"));
            };

            drawGizmoToggle.RegisterValueChangedCallback((e) => { drawGizmo = e.newValue; SceneView.RepaintAll(); });

            return root;
        }

        public void OnSceneGUI()
        {
            MoCheek self = target as MoCheek;

            if (!drawGizmo) { return; }

            for (int i = 0; i < self.profile.bones.Length; i++)
            {
                EditorGUI.BeginChangeCheck();

                Vector3 pos = Handles.PositionHandle(
                    Vector3.Scale(self.profile.bones[i].leafBonePosition, self.transform.lossyScale),
                    Quaternion.identity
                );

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(self, "Move Target Position");

                    self.profile.bones[i].leafBonePosition =
                        new(
                            pos.x / self.transform.lossyScale.x,
                            pos.y / self.transform.lossyScale.y,
                            pos.z / self.transform.lossyScale.z
                        );

                    EditorUtility.SetDirty(self);
                }
            }

        }
    }
}
