
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.OpenXR.ARFoundation;
using MSPlayground.Core;
using MSPlayground.Core.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Microsoft.MixedReality.OpenXR;

namespace MSPlayground.Core
{
    public class LocalAnchorManager : MonoBehaviour
    {
        public static LocalAnchorManager Instance { get; private set; }

        [SerializeField] ARAnchorManager _arAnchorManager;
        XRAnchorStore _anchorStore = null;
        private Dictionary<TrackableId, string> _anchorTrackableNamesMap = new Dictionary<TrackableId, string>();

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public void Initialize(UnityAction onInitialized)
        {
            _ = InitializeAsync(onInitialized);
        }

        async Task InitializeAsync(UnityAction onInitialized)
        {
            // delay results for one frame
            await Task.Delay(34);
            
            if (_anchorStore==null)
            { 
                _anchorStore = await _arAnchorManager.LoadAnchorStoreAsync();
            }

            onInitialized();
        }

        /// <summary>
        /// Create and persist an anchor to the anchor store and parent objectToAnchor to it
        /// </summary>
        /// <param name="objectToAnchor"></param>
        /// <returns></returns>
        public AnchorCreatedEvent CreateAnchorForGameObject(GameObject objectToAnchor)
        {
            Transform objectToAnchorTransform = objectToAnchor.transform;

            GameObject anchorObject = new GameObject();
            anchorObject.transform.SetPositionAndRotation(objectToAnchor.transform.position, objectToAnchor.transform.rotation);
            ARAnchor arAnchor = anchorObject.AddComponent<ARAnchor>();
            LocalAnchor localAnchor = anchorObject.AddComponent<LocalAnchor>();

            localAnchor.Initialize(arAnchor, System.Guid.NewGuid().ToString());

            // persist the anchor
            _anchorStore.TryPersistAnchor(arAnchor.trackableId, localAnchor.Guid);

            // parent our content to the anchor
            objectToAnchor.transform.SetParent(anchorObject.transform);

            // success event
            return new AnchorCreatedEvent()
            {
                AnchorId = localAnchor.Guid,
                AnchorObject = anchorObject
            };
        }

        /// <summary>
        /// Search for a set of anchors from the anchor store
        /// Calls AnchorLocatedEvent for the first anchor found
        /// </summary>
        /// <param name="anchorIds">List of ASA Ids to search for</param>
        public void LocateAnchors(string[] anchorIds)
        {
            StartCoroutine(LocateAnchorsCR());

            IEnumerator LocateAnchorsCR()
            {
                // delay results for one frame
                yield return null;

                _anchorTrackableNamesMap = new Dictionary<TrackableId, string>();

                foreach (string anchorName in anchorIds)
                {
                    TrackableId trackableId = _anchorStore.LoadAnchor(anchorName);
                    _anchorTrackableNamesMap.Add(trackableId, anchorName);
                }

                _arAnchorManager.anchorsChanged += OnAnchorsChanged;
            }
        }

        /// <summary>
        /// Stop anchor search
        /// </summary>
        public void StopLocatingAnchors()
        {
            _anchorTrackableNamesMap = null;
            _arAnchorManager.anchorsChanged -= OnAnchorsChanged;
        }

        /// <summary>
        /// Called by the system when anchors have changed
        /// </summary>
        /// <param name="eventArgs"></param>
        void OnAnchorsChanged(ARAnchorsChangedEventArgs eventArgs)
        {
            foreach (ARAnchor arAnchor in eventArgs.added)
            {
                if (TestForAnchorFound(arAnchor))
                {
                    return;
                }
            }

            foreach (ARAnchor arAnchor in eventArgs.updated)
            {
                if (TestForAnchorFound(arAnchor))
                {
                    return;
                }
            }

            bool TestForAnchorFound(ARAnchor arAnchor)
            {
                if (arAnchor.trackableId != null && arAnchor.trackingState ==TrackingState.Tracking)
                {
                    string anchorName = null;
                    if (_anchorTrackableNamesMap.TryGetValue(arAnchor.trackableId,out anchorName))
                    {
                        Debug.Log($"TestForAnchorFound: {anchorName}");

                        // send anchor located event
                        GlobalEventSystem.Fire<AnchorLocatedEvent>(new AnchorLocatedEvent()
                        {
                            AnchorId = anchorName,
                            AnchorObject = arAnchor.gameObject
                        });
                    }

                    return true;
                }
                return false;
            }
        }

    }
}
