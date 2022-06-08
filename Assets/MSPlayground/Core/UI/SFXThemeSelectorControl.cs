
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Data;
using MSPlayground.Core.Events;
using MSPlayground.Core.Utils;
using UnityEngine;

namespace MSPlayground.Core.UI
{
    /// <summary>
    /// Load the SFX theme of the given category from player prefs, and listen for any
    /// global events to change the theme.
    /// </summary>
    public class SFXThemeSelectorControl : MonoBehaviour
    {
        static string PLAYER_PREF_SFX_THEME(string categoryID) => $"sfxTheme_{categoryID}";
        
        [SerializeField] private ThemeSelect themeSelect = null;
        [SerializeField] private string _sfxThemeCategoryID = null;

        private void Start()
        {
            GlobalEventSystem.Register<ChangeSfxThemeEvent>(ChangeSfxThemeEventHandler);
            // Set the current sfx theme to the player pref if available
            SetTheme(PlayerPrefs.GetInt(PLAYER_PREF_SFX_THEME(_sfxThemeCategoryID), 0));
        }
        
        private void OnDestroy()
        {
            GlobalEventSystem.Unregister<ChangeSfxThemeEvent>(ChangeSfxThemeEventHandler);
        }
        
        /// <summary>
        /// Cycle through the available themes
        /// </summary>
        [ContextMenu(nameof(NextTheme))]
        public void NextTheme()
        {
            int nextIndex = (themeSelect.CurrentTheme + 1) % themeSelect.ThemeProfiles.Length;
            SetTheme(nextIndex);
        }
        
        /// <summary>
        /// Handle the Change SFX theme event only if the category ID matches
        /// </summary>
        /// <param name="eventData"></param>
        void ChangeSfxThemeEventHandler(ChangeSfxThemeEvent eventData)
        {
            if (eventData.CategoryID == _sfxThemeCategoryID)
            {
                SetTheme(eventData.ThemeIndex);
            }
        }

        /// <summary>
        /// Set theme and save to player prefs
        /// </summary>
        /// <param name="index"></param>
        void SetTheme(int index)
        {
            themeSelect.SetTheme(index);
            PlayerPrefs.SetInt(PLAYER_PREF_SFX_THEME(_sfxThemeCategoryID), index);
            PlayerPrefs.Save();
            Debug.Log($"[{this.GetType().ToString()}] Change SFX theme {_sfxThemeCategoryID}: {index}");

        }
    }
}