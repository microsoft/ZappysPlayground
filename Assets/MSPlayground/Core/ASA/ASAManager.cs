
using Microsoft.Azure.SpatialAnchors;
using Microsoft.Azure.SpatialAnchors.Unity;
using MSPlayground.Core.Utils;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace MSPlayground.Core.ASA
{
    public class ASAManager : MonoBehaviour
    {
        static public ASAManager Instance { get; private set; }

        [Tooltip("Simulate anchor save failures for testing purposes")]
        [SerializeField] bool _simulateSaveFailures = false;

        SpatialAnchorManager _spatialAnchorManager;
        CloudSpatialAnchorWatcher _anchorWatcher;

        const int EXPIRATION_DAYS = 60;

        private void Awake()
        {
            Debug.Assert(Instance == null, "Instance already exists");

            Instance = this;
        }

        private void Start()
        {
#if !VRBUILD && ASA_ENABLED
            _spatialAnchorManager = GetComponent<SpatialAnchorManager>();
            _spatialAnchorManager.enabled = true;
            //_spatialAnchorManager.LogDebug += (sender, args) => Debug.Log($"ASA - Debug: {args.Message}");
            _spatialAnchorManager.Error += (sender, args) => Debug.LogError($"ASA - Error: {args.ErrorMessage}");
            _spatialAnchorManager.AnchorLocated += OnAnchorLocated;
#endif
        }

        /// <summary>
        /// Create an anchor for a game object
        /// The game object will be parented to the created anchor
        /// ASACreationEvent will be fired when the creation is complete, whether success or failure
        /// </summary>
        /// <param name="objectToAnchor">The object to be anchored</param>
        public void CreateAnchorForGameObject(GameObject objectToAnchor)
        {
            _ = CreateAnchorForGameObjectAsync(objectToAnchor);
        }

        public void StopSession()
        {
            if (_anchorWatcher!=null)
            {
                _anchorWatcher.Stop();
                _anchorWatcher = null;
            }

            _spatialAnchorManager.StopSession();
            _spatialAnchorManager.DestroySession();
        }

        public void StartSession(UnityAction onStarted)
        {
            _ = StartSessionAsync(onStarted);
        }

        async Task StartSessionAsync(UnityAction onStarted)
        {
            await _spatialAnchorManager.StartSessionAsync();
            onStarted?.Invoke();
        }

        private async Task CreateAnchorForGameObjectAsync(GameObject objectToAnchor)
        {
            try
            {
                // create a new gameobject that will become our anchor
                GameObject anchorObject = new GameObject();

                //Add and configure ASA components
                CloudNativeAnchor cloudNativeAnchor = anchorObject.AddComponent<CloudNativeAnchor>();
                await cloudNativeAnchor.NativeToCloud();
                CloudSpatialAnchor cloudSpatialAnchor = cloudNativeAnchor.CloudAnchor;
                cloudSpatialAnchor.Expiration = DateTimeOffset.Now.AddDays(EXPIRATION_DAYS);

                //Collect Environment Data
                while (!_spatialAnchorManager.IsReadyForCreate)
                {
                }

                bool saveSucceeded = false;

                if (!_simulateSaveFailures)
                {
                    // save the anchor
                    await _spatialAnchorManager.CreateAnchorAsync(cloudSpatialAnchor);

                    // check success
                    saveSucceeded = cloudSpatialAnchor != null;
                }


                // success?
                if (saveSucceeded)
                {
                    // update the anchor name with the anchor id
                    anchorObject.name = $"Anchor.{cloudSpatialAnchor.Identifier}";

                    // parent our content to the anchor
                    objectToAnchor.transform.SetParent(anchorObject.transform);

                    // success event
                    GlobalEventSystem.Fire<AnchorCreatedEvent>(new AnchorCreatedEvent()
                    {
                        AnchorId = cloudSpatialAnchor.Identifier,
                        AnchorObject = anchorObject
                    });
                }
                else
                {
                    OnFailed();
                }
            }
            catch(Exception ex)
            {
                Debug.LogError($"Exception {ex.ToString()}");
                OnFailed();
            }

            void OnFailed()
            {
                // anchor creation failed
                GlobalEventSystem.Fire<AnchorCreatedEvent>(new AnchorCreatedEvent()
                {
                    AnchorId = null,
                    AnchorObject = null
                });
            }
        }

        /// <summary>
        /// Search for a single anchor
        /// ADALocatedEvent is fired with result
        /// </summary>
        /// <param name="anchorId"></param>
        public void LocateAnchor(string anchorId)
        {
            LocateAnchors(new string[] { anchorId });
        }

        /// <summary>
        /// Search for a set of anchors
        /// ASALocatedEvent is fired with result
        /// </summary>
        /// <param name="anchorIds">List of ASA Ids to search for</param>
        public void LocateAnchors(string[] anchorIds)
        {
            try
            {
                AnchorLocateCriteria anchorLocateCriteria = new AnchorLocateCriteria();
                anchorLocateCriteria.Identifiers = anchorIds;
                anchorLocateCriteria.Strategy = LocateStrategy.VisualInformation;
                _anchorWatcher = _spatialAnchorManager.Session.CreateWatcher(anchorLocateCriteria);

            }
            catch (Exception ex)
            {
                Debug.LogError("Exception " + ex.ToString());

                // send anchor located event
                GlobalEventSystem.Fire<AnchorLocatedEvent>(new AnchorLocatedEvent()
                {
                    AnchorId = null,
                    AnchorObject = null
                });
            }
        }

        /// <summary>
        /// Stop searching for anchors
        /// </summary>
        public void StopLocatingAnchors()
        {
            if (_anchorWatcher != null)
            {
                _anchorWatcher.Stop();
                _anchorWatcher = null;
            }
        }

        /// <summary>
        /// Callback when anchor is located
        /// </summary>
        private void OnAnchorLocated(object sender, AnchorLocatedEventArgs args)
        {
            //Creating and adjusting GameObjects have to run on the main thread. We are using the UnityDispatcher to make sure this happens.
            UnityDispatcher.InvokeOnAppThread(() =>
            {
                if (args.Status == LocateAnchorStatus.Located)
                {
                    // read out Cloud Anchor values
                    CloudSpatialAnchor cloudSpatialAnchor = args.Anchor;
                    Pose anchorPose = cloudSpatialAnchor.GetPose();

                    // create GameObject
                    GameObject anchorGameObject = new GameObject();
                    anchorGameObject.name = $"Anchor.{args.Anchor.Identifier}";
                    anchorGameObject.transform.position = anchorPose.position;
                    anchorGameObject.transform.rotation = anchorPose.rotation;

                    // add and apply cloud anchor which sets the pose
                    CloudNativeAnchor cna = anchorGameObject.AddComponent<CloudNativeAnchor>();
                    cna.CloudToNative(cloudSpatialAnchor);

                    // send anchor located event
                    GlobalEventSystem.Fire<AnchorLocatedEvent>(new AnchorLocatedEvent()
                    {
                        AnchorId = args.Anchor.Identifier,
                        AnchorObject = anchorGameObject
                    });
                }
            });
        }
    }
}
