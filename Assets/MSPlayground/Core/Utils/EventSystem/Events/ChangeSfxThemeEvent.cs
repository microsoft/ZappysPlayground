
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using MSPlayground.Core.Utils;
using UnityEngine;

namespace MSPlayground.Core.Events
{
    /// <summary>
    /// Global event used to change the active data source sfx theme
    /// for the given theme type
    /// </summary>
    public class ChangeSfxThemeEvent : BaseEvent
    {
        /// <summary>
        /// This event could be used for any arbitrary sound effects that we want to theme.
        /// Right now, we only support themed Robot audio sound effects (e.g. CategoryID: "robot")
        /// but you may want to potentially extend behaviour to theme other objects.
        /// </summary>
        public string CategoryID;
        /// <summary>
        /// Theme index from the array sfx themes defined in the relevant data source
        /// </summary>
        public int ThemeIndex;
    }
}
