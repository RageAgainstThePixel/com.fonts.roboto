// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using TMPro.EditorUtilities;
using UnityEditor;
using UnityEngine;

namespace Google.Fonts
{
    internal static class CheckDependencies
    {
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
