// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Data;
using MSPlayground.Common.Helper;
using MSPlayground.Core.Utils;
using UnityEngine;

namespace MSPlayground.Core.UI
{
    /// <summary>
    /// A data source that notifies child data consumers when themeable components
    /// should be updated with the current UIThemeProfile.
    /// Unlike the DataSourceUITheme, this script gets the source UIThemeProfile
    /// from the ThemeSystem singleton and listens for change events.
    /// </summary>
    /// <remarks>
    /// All themeable components must be a child of the GameObject where this data source is attached in
    /// order to be properly notified. Parent/ancestor level does not matter.
    /// </remarks>
    public class DataSourceUIThemeListener : DataSourceGOBase
    {
        /// The Reset() function is called whenever this script is first attached to a new GameObject
        /// or when manually clicked in the component settings menu.
        private void Reset()
        {
            dataSourceType = "theme";      // Make default "theme" to differentiate from "data" data source types
        }

        private void Start()
        {
            // Wait until theme system exists before initializing
            StartCoroutine(Coroutines.WaitUntil(() => ThemeSystem.Exists, Initialize));
        }

        private void Initialize()
        {
            GlobalEventSystem.Register<ChangeUIThemeEvent>(ChangeUIThemeEventHandler);
            SetTheme(ThemeSystem.GetCurrentUITheme());
        }
        
        private void OnDestroy()
        {
            GlobalEventSystem.Unregister<ChangeUIThemeEvent>(ChangeUIThemeEventHandler);
        }
        
        /// <summary>
        /// Handle the Change UI theme event
        /// </summary>
        /// <param name="eventData"></param>
        private void ChangeUIThemeEventHandler(ChangeUIThemeEvent eventData)
        {
            SetTheme(eventData.Theme);
        }

        /// <summary>
        /// Set the UI theme and notified data source
        /// </summary>
        /// <param name="themeIndex"></param>
        public void SetTheme(UIThemeProfile themeProfile)
        {
            if (themeProfile != null)
            {
                DataSourceReflection dataSource = DataSource as DataSourceReflection;
                dataSource.SetDataSourceObject(themeProfile);
                DataSource.NotifyAllChanged();
            }
        }
        
        /// <inheritdoc/>
        public override IDataSource AllocateDataSource()
        {
            if (ThemeSystem.Exists)
            {
                return new DataSourceReflection(ThemeSystem.GetCurrentUITheme());
            }

            return null;
        }
    }
}