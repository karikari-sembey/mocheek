using System.IO;
using com.guraril.mocheek.runtime;
using UnityEngine;

namespace com.guraril.mocheek.editor
{
    class ProfileManager
    {
        public static Profile LoadProfile(string profilePath)
        {
            string profileText = File.ReadAllText(profilePath);
            Profile profile = JsonUtility.FromJson<Profile>(profileText);
            if (profile == null) { Debug.LogError("壊れたプロファイルが読み込まれました。デフォルト値のMoCheekが追加されます。"); }
            return profile;
        }

        public static Profile LoadProfileFromPreset(MoCheekPreset preset)
        {
            const string RESOURCE_FOLDER = "./Packages/com.guraril.mocheek/Resources";
            return preset switch
            {
                MoCheekPreset.Chiffon => LoadProfile(RESOURCE_FOLDER + "/chiffon.mocheek"),
                MoCheekPreset.Hakka => LoadProfile(RESOURCE_FOLDER + "/hakka.mocheek"),
                MoCheekPreset.Mishe => LoadProfile(RESOURCE_FOLDER + "/mishe.mocheek"),
                _ => null,
            };
        }

        public static void SaveProfile(Profile profile, string path)
        {
            if (path == null || path == "") { return; }
            File.WriteAllText(path, JsonUtility.ToJson(profile));
        }
    }
}
