
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.MixedReality.Toolkit.Data;
using Microsoft.MixedReality.Toolkit.UX;
using MSPlayground.Core;
using MSPlayground.Core.Events;
using MSPlayground.Core.UI;
using MSPlayground.Core.Utils;
using MSPlayground.Turbines;
using MSPlayground.Turbines.Events;
using UnityEngine;

namespace MSPlayground.Scenarios.Turbines
{
    /// <summary>
    /// Manages the settings panel objects and actions
    /// </summary>
    public class SettingsPanel : Panel
    {
        private const string PLAYER_PREF_ROBOT_SFX_THEME = "sfxTheme_robot";


        [SerializeField] private GameObject _themeGroupGameObject;        
        [SerializeField] private Core.UI.ToggleGroup _themeGroup;
        [SerializeField] private GameObject _robotLanguageGroupGameObject;
        [SerializeField] private Core.UI.ToggleGroup _robotLanguageGroup;
        [SerializeField] private string[] _themes = new[] { "blue", "red", "green"};
        [SerializeField] private string[] _robotVoices = new[] {"voice_1", "voice_2"};

        /// <summary>
        /// Root gameobject for UI theming
        /// </summary>
        public GameObject ThemeGroup => _themeGroupGameObject;
        
        /// <summary>
        /// Root gameobject for language theming
        /// </summary>
        public GameObject LanguageThemeGroup => _robotLanguageGroupGameObject;

        private void Start()
        {
            _themeGroup.InstantiateButtons(_themes.Length, OnThemeToggleButtonInstantiated);
            _robotLanguageGroup.InstantiateButtons(_robotVoices.Length, OnRobotToggleButtonInstantiated);
            SetButtonToggledFromPlayerPrefs();
        }

        protected override void OnEnable()
        {
            PlayIntro();

            // Refeed the theme and robot language theme data back into the button instances
            for (int i = 0; i < _themeGroup.ButtonInstances.Count; ++i)
            {
                OnThemeToggleButtonInstantiated(i, _themeGroup.ButtonInstances[i]);
            }
            
            for (int i = 0; i < _robotLanguageGroup.ButtonInstances.Count; ++i)
            {
                OnRobotToggleButtonInstantiated(i, _robotLanguageGroup.ButtonInstances[i]);
            }
        }

        /// <summary>
        /// Visually toggle the active themes from player prefs.
        /// </summary>
        void SetButtonToggledFromPlayerPrefs()
        {
            int uiThemeIndex = Array.IndexOf(_themes, ThemeSystem.GetCurrentUIThemeName());
            _themeGroup.SetButtonToggled(uiThemeIndex >= 0 ? uiThemeIndex : 0);
            _robotLanguageGroup.SetButtonToggled(PlayerPrefs.GetInt(PLAYER_PREF_ROBOT_SFX_THEME, 0));
        }

        private void OnRobotToggleButtonInstantiated(int index, PressableButton button)
        {
            string robotTheme = _robotVoices[index];
            
            var dataSource = button.transform.GetComponentInParent<DataSourceGODictionary>();
            dataSource.DataChangeSetBegin();
            dataSource.SetValue("text", $"UI/{robotTheme}");
            dataSource.SetValue("theme", robotTheme);
            dataSource.DataChangeSetEnd();
            
            button.OnClicked.AddListener(() =>
            {
                OnToggleRobotLanguageButtonPressed(robotTheme);
            });
        }

        private void OnThemeToggleButtonInstantiated(int index, PressableButton button)
        {
            string theme = _themes[index];
            
            var dataSource = button.transform.GetComponentInParent<DataSourceGODictionary>();
            dataSource.DataChangeSetBegin();
            dataSource.SetValue("text", $"UI/{theme}");
            dataSource.SetValue("color", theme);
            dataSource.SetValue("theme", theme);
            dataSource.DataChangeSetEnd();
            
            button.OnClicked.AddListener(() =>
            {
                OnToggleThemeButtonPressed(theme);
            });
        }

        public void ToggleActive()
        {
            if (gameObject.activeSelf)
            {
                ClosePanel();
            }
            else
            {
                ShowPanel();
            }
        }

        private void OnToggleThemeButtonPressed(string themeID)
        {
            Debug.Log($"<b>[{themeID}]</b> selected");
            ThemeSystem.ChangeUITheme(themeID);
        }

        public void OnToggleRobotLanguageButtonPressed(string robotLanguageID)
        {
            int index = 0;
            for (int i = 0; i < _robotVoices.Length; ++i)
            {
                if (_robotVoices[i].Equals(robotLanguageID, StringComparison.Ordinal))
                {
                    index = i;
                    break;
                }
            }
         
            GlobalEventSystem.Fire(new ChangeSfxThemeEvent()
            {
                CategoryID = "robot",
                ThemeIndex = index
            });

            // Trigger general sfx to give example of new robot voice
            GlobalEventSystem.Fire(new RobotSFXEvent() {SfxType = SFXType.General});
        }

        public void OnEditNameButtonPressed()
        {
            UISystem.SpawnComplexPanel("settings_change_name");
            ClosePanel();
        }
    }
}