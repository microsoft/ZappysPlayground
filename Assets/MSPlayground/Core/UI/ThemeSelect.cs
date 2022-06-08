// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.Toolkit.Data;
using UnityEngine;

namespace MSPlayground.Core.UI
{
    /// <summary>
    /// Based off of the helper class {Microsoft.MixedReality.Toolkit.Data.ThemeSelector}, this helper
    /// allows you to control generic themes through scriptable objects.
    /// Unlike the MRTK Theme Selector, this class exposes the array of theme profiles so that we may
    /// cycle through them externally.
    /// </summary>
    /// <remarks>
    /// TODO Remove if {Microsoft.MixedReality.Toolkit.Data.ThemeSelector} exposes length of available theme profiles.
    /// </remarks>
    public class ThemeSelect : MonoBehaviour
    {
        [Tooltip("A scriptable object that provides the theme to use for MRTK UX Controls")]
        [SerializeField] private ScriptableObject[] _themeProfiles;

        [Tooltip("The ThemeProvider instance to modify.")]
        [SerializeField] private DataSourceThemeProvider _themeProvider;

        [Tooltip("The current theme.")]
        [SerializeField] private int _currentTheme = 0;

        public int CurrentTheme => _currentTheme;
        public ScriptableObject[] ThemeProfiles => _themeProfiles;

        /// <summary>
        /// Set the theme to specified profile in the list of theme profiles.
        /// </summary>
        /// <param name="whichTheme">Index for theme to select and make currently active theme.</param>
        public void SetTheme(int whichTheme)
        {
            if (_themeProvider != null && whichTheme < _themeProfiles.Length)
            {
                _themeProvider.SetTheme(_themeProfiles[whichTheme]);
                _themeProvider.ThemeProfile = _themeProfiles[whichTheme];
                _currentTheme = whichTheme;
            }
        }

        private void OnStart()
        {
            SetTheme(_currentTheme);
        }

        private void OnValidate()
        {
            if (CurrentTheme < 0)
            {
                SetTheme(0);
            }
            if (CurrentTheme >= _themeProfiles.Length)
            {
                SetTheme(_themeProfiles.Length - 1);
            }
            SetTheme(_currentTheme);
        }
    }
}