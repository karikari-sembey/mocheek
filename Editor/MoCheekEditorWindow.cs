using System.Collections.Generic;
using System.Linq;
using com.guraril.mocheek.runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;

namespace com.guraril.mocheek.editor
{
    class MoCheekEditorWindow : EditorWindow
    {
        [SerializeField] private VisualTreeAsset visualTreeAsset;
        [SerializeField] private VisualTreeAsset moCheekInspector;
        private bool selectFromPreset = true;
        private MoCheekPreset moCheekPreset = MoCheekPreset.Hakka;
        private string profilePath = null;
        private SkinnedMeshRenderer faceMesh = null;

        DropdownField avatarSelection = null;
        ObjectField selectedAvatar = null;
        ObjectField selectedFaceMesh = null;

        Dictionary<string, GameObject> avatarDictionary = new();

        [MenuItem("Tools/MoCheek")]
        public static void ShowWindow() { var window = GetWindow<MoCheekEditorWindow>(); window.minSize = new Vector2(400, 450); }

        private void OnEnable() { EditorSceneManager.sceneOpened += SceneChanged; }

        private void OnDisable() { EditorSceneManager.sceneOpened -= SceneChanged; }

        private void SceneChanged(Scene _scene, OpenSceneMode _mode) { UpdateState(); }

        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.Add(visualTreeAsset.Instantiate());

            avatarSelection = root.Q<DropdownField>("AvatarSelection");
            selectedAvatar = root.Q<ObjectField>("SelectedAvatar");
            RadioButtonGroup profileLoadWay = root.Q<RadioButtonGroup>("ProfileLoadWay");
            EnumField presetSelection = root.Q<EnumField>("PresetSelection");
            VisualElement profileFile = root.Q<VisualElement>("ProfileFile");
            Label profilePathLabel = root.Q<Label>("ProfileFilePath");
            Button profileFileSelectButton = root.Q<Button>("ProfileFileSelectButton");
            selectedFaceMesh = root.Q<ObjectField>("FaceMesh");
            Button configButton = root.Q<Button>("ConfigButton");

            VisualElement warnNotInAvatar = root.Q<VisualElement>("W-NotInAvatar");

            avatarSelection.RegisterValueChangedCallback((e) =>
            {
                if (avatarDictionary.TryGetValue(e.newValue, out var value)) { selectedAvatar.value = value; }
            });

            profileLoadWay.RegisterValueChangedCallback((e) =>
                {
                    if (e.newValue == 0)
                    {
                        presetSelection.style.display = DisplayStyle.Flex;
                        profileFile.style.display = DisplayStyle.None;
                        selectFromPreset = true;
                    }
                    if (e.newValue == 1)
                    {
                        presetSelection.style.display = DisplayStyle.None;
                        profileFile.style.display = DisplayStyle.Flex;
                        selectFromPreset = false;
                    }
                });

            presetSelection.RegisterValueChangedCallback((e) => { moCheekPreset = (MoCheekPreset)e.newValue; });

            profileFileSelectButton.clicked += () =>
            {
                var filePath = EditorUtility.OpenFilePanelWithFilters("MoCheek プロファイルを選択", "./Assets", new string[] { "MoCheek Profile", "mocheek" });
                if (filePath != null)
                {
                    profilePath = filePath;
                    profilePathLabel.text = filePath;
                }
                else
                {
                    profilePathLabel.text = "ファイルが選択されていません";
                }
            };

            selectedFaceMesh.RegisterValueChangedCallback((e) =>
                {
                    if (((SkinnedMeshRenderer)e.newValue).transform.IsChildOf(((VRCAvatarDescriptor)selectedAvatar.value).transform))
                    {
                        faceMesh = (SkinnedMeshRenderer)e.newValue;
                        warnNotInAvatar.style.display = DisplayStyle.None;
                    }
                    else
                    {
                        warnNotInAvatar.style.display = DisplayStyle.Flex;
                    }
                });

            configButton.clicked += () =>
            {
                MoCheek moCheek = faceMesh.gameObject.AddComponent<MoCheek>();
                if (selectFromPreset)
                {
                    moCheek.profile = ProfileManager.LoadProfileFromPreset(moCheekPreset);
                }
                else
                {
                    moCheek.profile = ProfileManager.LoadProfile(profilePath);
                }
            };

            UpdateState();
        }

        private void UpdateState()
        {
            var avatars = SceneManager.GetActiveScene().GetRootGameObjects().ToList().FindAll(
                (a) => { if (a.TryGetComponent<VRC_AvatarDescriptor>(out var _)) { return true; } else { return false; } });

            avatarDictionary.Clear();
            foreach (var avatar in avatars)
            {
                avatarDictionary[avatar.name] = avatar;
                avatarSelection.choices.Add(avatar.name);
            }

            if (avatars.Count > 0)
            {
                var avatar = avatars.First();
                var avatarDescriptor = avatar.GetComponent<VRCAvatarDescriptor>();

                faceMesh = avatar.GetComponent<VRCAvatarDescriptor>().VisemeSkinnedMesh;

                avatarSelection.value = avatar.name;
                selectedAvatar.value = avatar;
                if (avatarDescriptor.VisemeSkinnedMesh != null)
                {
                    selectedFaceMesh.value = avatarDescriptor.VisemeSkinnedMesh;
                }
            }

        }
    }
}
