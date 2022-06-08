// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.MixedReality.Toolkit;
using MSPlayground.Core.Utils;
using MSPlayground.Turbines;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MSPlayground.Core.UI
{
    #region Global Events
    /// <summary>
    /// Invoked to propogate UI theme updates to the themable UI.
    /// </summary>
    /// <remarks>
    /// This is used to pass the referenced theme profile to event listeners.
    /// If you want to change the actual UI Theme, you can call the public method
    /// ThemeSystem.ChangeUITheme()
    /// </remarks>
    internal class ChangeUIThemeEvent : BaseEvent
    {
        public UIThemeProfile Theme;
    }
    #endregion

    /// <summary>
    /// A centralized theme system to keep track of and manage the user's
    /// desired UI theme. Pair with DataSourceUIThemeListener to listen for
    /// theme updates.
    /// </summary>
    /// <remarks>
    /// This system allows us to control the theme of UI panels with a singular
    /// reference of the actual UIThemeProfiles, rather than restrict all themeable
    /// UI to be parented to a DataSourceUITheme (embedded theme profiles).
    /// </remarks>
    public class ThemeSystem : MonoBehaviour
    {
        static ThemeSystem _instance;
        
        const string PLAYER_PREF_UI_THEME = "uiTheme";

        [SerializeField] private ThemeReference[] _availableThemes;
        
        /// <summary>
        /// Indexes of the theme reference in _availableThemes
        /// </summary>
        private Dictionary<string, int> _themeIndexes = new Dictionary<string, int>();
        /// <summary>
        /// ID of the current theme
        /// </summary>
        private string _currentTheme = null;

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;

            LoadIndexes();

            // Get the current theme from PlayerPrefs if available (else default)
            string defaultTheme = _availableThemes.FirstOrDefault().ID;
            _currentTheme = PlayerPrefs.GetString(PLAYER_PREF_UI_THEME, defaultTheme);
        }
        
        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
        
        /// <summary>
        /// Get array indexes for all of the available theme profiles for
        /// quick referencing
        /// </summary>
        private void LoadIndexes()
        {
            if (_availableThemes != null)
            {
                for (int i = 0; i < _availableThemes.Length; i++)
                {
                    if (!_themeIndexes.ContainsKey(_availableThemes[i].ID))
                    {
                        _themeIndexes[_availableThemes[i].ID] = i;
                    }
                    else
                    {
                        Debug.LogWarning($"[{this.GetType().ToString()}] There are multiple theme references" +
                                         $"with the ID: {_availableThemes[i].ID}");
                    }
                }
            }
        }

        /// <summary>
        /// Change the UI theme given theme ID
        /// </summary>
        /// <param name="themeID"></param>
        private void DoChangeUITheme(string themeID)
        {
            if (_themeIndexes.TryGetValue(themeID, out int themeIndex))
            {
                GlobalEventSystem.Fire(new ChangeUIThemeEvent()
                {
                    Theme = _availableThemes[themeIndex].Theme
                });
                _currentTheme = themeID;
                PlayerPrefs.SetString(PLAYER_PREF_UI_THEME, _currentTheme);
            }
        }
        
        
        /// <summary>
        /// Change the UI theme given index in array
        /// </summary>
        /// <param name="index"></param>
        private void DoChangeUITheme(int index)
        {
            if (0 <= index && index < _availableThemes.Length)
            {
                ThemeReference themeReference = _availableThemes[index];
                GlobalEventSystem.Fire(new ChangeUIThemeEvent()
                {
                    Theme = themeReference.Theme
                });
                _currentTheme = themeReference.ID;
                PlayerPrefs.SetString(PLAYER_PREF_UI_THEME, _currentTheme);
            }
        }

        private UIThemeProfile DoGetCurrentUITheme()
        {
            if (!string.IsNullOrEmpty(_currentTheme))
            {
                return _availableThemes[_themeIndexes[_currentTheme]].Theme;
            }

            return null;
        }

        /// <summary>
        /// Cycle through the available themes
        /// </summary>
        [ContextMenu(nameof(NextTheme))]
        public void NextTheme()
        {
            int currentIndex = _themeIndexes[_currentTheme];
            DoChangeUITheme((currentIndex + 1) % _availableThemes.Length);
        }
        #region Static Interface
        /// <summary>
        /// Does the theme system exist (non-null instance)
        /// </summary>
        public static bool Exists => _instance != null;
        /// <summary>
        /// Get the current UI theme or null
        /// </summary>
        public static UIThemeProfile GetCurrentUITheme() => _instance.DoGetCurrentUITheme();
        /// <summary>
        /// Get the current UI theme name
        /// </summary>
        public static string GetCurrentUIThemeName() => _instance?._currentTheme;
        /// <summary>
        /// Set the current theme to the given profile
        /// </summary>
        public static void ChangeUITheme(string themeID) => _instance.DoChangeUITheme(themeID);
        #endregion
    }

    #region Embedded Types
    /// <summary>
    /// Theme profile reference with the given ID
    /// </summary>
    [Serializable]
    public struct ThemeReference
    {
        public string ID;
        public UIThemeProfile Theme;
    }
    #endregion
}