
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using MSPlayground.Core.Utils;
using UnityEngine;

namespace MSPlayground.Core
{
    /// <summary>
    /// Event fired when an anchor has been located
    /// </summary>
    public class AnchorLocatedEvent : BaseEvent
    {
        /// <summary>
        /// The tracked anchor.  Parent your content to this to anchor it.
        /// </summary>
        public GameObject AnchorObject;

        /// <summary>
        /// The ASA ID of the anchor.
        /// </summary>
        public string AnchorId;
    }
}
