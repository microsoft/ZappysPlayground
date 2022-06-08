// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace MSPlayground.Core.Utils.Editor
{
    /// <summary>
    /// Processors to build addressables before app builds
    /// </summary>
    static class BuildAddressablesProcessor
    {
        /// <summary>
        /// Run a clean build before export.
        /// </summary>
        public static void PreExport()
        {
            Debug.Log("BuildAddressablesProcessor.PreExport start");
            AddressableAssetSettings.CleanPlayerContent();
            AddressableAssetSettings.BuildPlayerContent();
            Debug.Log("BuildAddressablesProcessor.PreExport done");
        }

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            BuildPlayerWindow.RegisterBuildPlayerHandler(BuildPlayerHandler);
        }

        /// <summary>
        /// Show dialog in Unity build menu to determine whether the build needs to rebuild Addressables content.
        /// </summary>
        /// <param name="options"></param>
        private static void BuildPlayerHandler(BuildPlayerOptions options)
        {
            if (EditorUtility.DisplayDialog("Build with Addressables",
                    "Do you want to build clean addressables before export?\n" +
                    "Addressables need to be rebuilt if there is new content for localization.",
                    "Build with Addressables", "Skip"))
            {
                PreExport();
            }

            BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(options);
        }
    }
}