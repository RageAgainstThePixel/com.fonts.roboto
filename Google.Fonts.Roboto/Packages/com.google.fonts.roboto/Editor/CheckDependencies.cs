// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using TMPro.EditorUtilities;
using UnityEditor;
using UnityEngine;

namespace Google.Fonts
{
    internal static class CheckDependencies
    {
        private static readonly string FontPath = "submodules\\google.fonts\\apache\\roboto\\static";
        private static readonly string PackagePath = "Packages\\com.google.fonts.roboto";

        [InitializeOnLoadMethod]
        public static void Check()
        {
            if (!Directory.Exists($"{Application.dataPath}/TextMesh Pro"))
            {
                if (EditorUtility.DisplayDialog("Missing Package!",
                    "The project requires the Text Mesh Pro essential resources, would you like to import them now?",
                    "OK", "Later"))
                {
                    ImportTMProEssentialResources();
                }
            }

            var projectRootPath = Directory.GetParent(Directory.GetParent(Application.dataPath).FullName);
            var submoduleFontPath = $"{projectRootPath}\\{FontPath}";

            Debug.Assert(Directory.Exists(submoduleFontPath), $"Missing submodule path {submoduleFontPath}\nDid you forget to recursively checkout the repository?");

            var packagePath = Directory.GetParent(Application.dataPath);
            var packageFontPath = $"{packagePath}\\{PackagePath}\\Runtime\\Fonts";

            if (!Directory.Exists(packageFontPath))
            {
                Directory.CreateDirectory(packageFontPath);

                var fonts = Directory.GetFiles(submoduleFontPath);

                foreach (var font in fonts)
                {
                    var newPath = font.Replace(submoduleFontPath, packageFontPath);

                    if (File.Exists(newPath))
                    {
                        File.Delete(newPath);
                    }

                    File.Copy(font, newPath);
                }
            }
        }

        // Copied straight from the TMP_PackageUtilities.cs but we needed to import the assets if in batch mode.
        private static void ImportTMProEssentialResources()
        {
            // Check if the TMP Settings asset is already present in the project.
            var settings = AssetDatabase.FindAssets("t:TMP_Settings");

            string settingsFilePath;
            byte[] settingsBackup;

            if (settings.Length > 0)
            {
                // Save assets just in case the TMP Setting were modified before import.
                AssetDatabase.SaveAssets();

                // Copy existing TMP Settings asset to a byte[]
                settingsFilePath = AssetDatabase.GUIDToAssetPath(settings[0]);
                settingsBackup = File.ReadAllBytes(settingsFilePath);

                AssetDatabase.importPackageCompleted += ImportCallback;
            }

            AssetDatabase.ImportPackage($"{TMP_EditorUtility.packageFullPath}/Package Resources/TMP Essential Resources.unitypackage", Application.isBatchMode);

            void ImportCallback(string packageName)
            {
                // Restore backup of TMP Settings from byte[]
                File.WriteAllBytes(settingsFilePath, settingsBackup);
                AssetDatabase.Refresh();
                AssetDatabase.importPackageCompleted -= ImportCallback;
            }
        }

    }
}
