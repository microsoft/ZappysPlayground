
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using MSPlayground.Core.Utils;
using UnityEngine;

namespace MSPlayground.Core
{
    /// <summary>
    /// Event fired when anchor creation has finished
    /// </summary>
    public class AnchorCreatedEvent : BaseEvent
    {
        /// <summary>
        /// The anchor that has been created.  
        /// Null if creation failed.
        /// The content has been parented to this object.
        /// </summary>
        public GameObject AnchorObject;

        /// <summary>
        /// The ASA ID or guid of the anchor.
        /// </summary>
        public string AnchorId;
    }
}
