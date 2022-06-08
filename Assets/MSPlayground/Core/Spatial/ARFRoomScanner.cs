
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using MSPlayground.Core.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace MSPlayground.Core.Spatial
{
    public class ARFRoomScanner : RoomGenerator
    {
        [SerializeField] LayerMask _arPlanesLayer;
        [SerializeField] RoomScanTargetController _roomScanTargetController;
        [SerializeField] int _numSpokes = 64;
        [SerializeField] float _defaultRayLength = 6.0f;
        [SerializeField] int _minSpokesBetweenGems = 2;
        [SerializeField] int _numSpokesPerFrame = 2;

        [Header("Intitial Scan")]
        [SerializeField] int _numGemsInitialScan = 6;
        [SerializeField] float _gemDistanceInitialScan = 1.25f;

        [Header("Gem Info")]
        [SerializeField] float[] _gemHeights = { 0f, 0.5f, 0, -0.5f };
        [SerializeField] float _gemDistance = 2.0f;
        int _totalGemsDisplayed = 0;

        [Header("Room Requirements")]
        [SerializeField] float _minLongestWallLength = 3.0f;
        [SerializeField] float _minRoomSize = 8.0f;
        [SerializeField] float _minPlatformHeight = 0.25f;
        [SerializeField] float _maxPlatformHeight = 1.5f;

        [Header("Debug")]
        [SerializeField] bool _skipScanning = false;
        [SerializeField] bool _arfPlaneRenderingEnabled = false;
        [SerializeField] bool _debugRenderingEnabled = false;

        Vector3 _scanningOrigin;

        bool _floorIsValid = false;
        float _floorHeight = float.MaxValue;
        bool _ceilingIsValid = false;
        float _ceilingHeight = float.MinValue;

        // debug
        Transform _debugContainer;
        bool _arfPlaneRenderingActive = false;

        #region RaycastResult
        class RaycastResult
        {
            enum State
            {
                NoRaycast,
                RaycastHit,
                RaycastMissed
            }

            public int Id;
            public Vector3 StartPos;
            public Vector3 Direction;
            public Vector3 Normal;
            private ARPlane ARPlane;
            public string ARPlaneId;
            public Vector3 HitPos;
            public float RayLength;            
            public DebugSphere DebugMarker;
            State _state;

            public bool DidRaycastHit {  get { return _state == State.RaycastHit; } }
            public bool DidRaycastMiss { get { return _state == State.RaycastMissed; } }
            public bool DidRaycast {  get { return _state != State.NoRaycast; } }

            public RaycastResult(int id, Vector3 startPos, Vector3 direction)
            {
                Init(id, startPos, direction);
            }

            public void Init(int id, Vector3 startPos, Vector3 direction)
            {
                Id = id;
                StartPos = startPos;
                Direction = direction;
                ARPlane = null;
                ARPlaneId = null;
                HitPos = Vector3.zero;
                RayLength = -1f;
                _state = State.NoRaycast;
            }

            public void SetRayHit(ARPlane arPlane, Vector3 hitPos)
            {
                ARPlane = arPlane;
                ARPlaneId = arPlane.trackableId.ToString();
                HitPos = hitPos;
                Normal = arPlane.normal;
                _state = State.RaycastHit;
            }

            public void SetRayMissed(float rayLength)
            {
                ARPlane = null;
                ARPlaneId = null;
                RayLength = rayLength;
                _state = State.RaycastMissed;
            }
        }
        #endregion

        #region WallInfo
        class WallInfo
        {
            public RaycastResult r0;
            public RaycastResult r1;

            public float LengthSqr { get { return MathHelpers.HorizontalSqrMagnitude(r0.HitPos - r1.HitPos); } }
        }
        #endregion

        /// <inheritdoc/>
        protected override void Start()
        {
            base.Start();

            _roomScanTargetController.Deactivate();
        }

        /// <inheritdoc/>
        public override void Reset()
        {
            base.Reset();

            _roomScanTargetController.Deactivate();

            if (_debugContainer!=null)
            {
                GameObject.Destroy(_debugContainer.gameObject);
            }
            _debugContainer = new GameObject("ARFRoomScanner.Debug").transform;

            EnableDebugRendering(_debugRenderingEnabled);
        }

        /// <summary>
        /// Enable / disable ARF plane rendering
        /// </summary>
        /// <param name="enable"></param>
        void EnableARFPlaneRendering(bool enable)
        {
            _arfPlaneRenderingActive = enable;

            foreach (ARPlane plane in _arPlaneManager.trackables)
            {
                plane.EnableRendering(enable);
            }
        }

        /// <summary>
        /// Callback called when ARF planes have changed
        /// </summary>
        /// <param name="args">planes changed state</param>
        void OnPlanesChanged(ARPlanesChangedEventArgs args)
        {
            foreach (ARPlane arPlane in args.added)
            {
                // by default planes will be rendered.  so we only need to set the rendering state if rendering is disabled
                if (!_arfPlaneRenderingActive)
                {
                    arPlane.EnableRendering(false);
                }
            }
        }

        /// <summary>
        /// Start room scan process
        /// Will fire RoomScanCompleteEvent when complete
        /// </summary>
        public override void StartRoomScan(bool createFakeWalls)
        {
            StartCoroutine(ScanRoomCR(createFakeWalls || _skipScanning));
        }

        IEnumerator ScanRoomCR(bool createFakeWalls)
        {
            yield return null;

            bool roomBuilt = false;

            if (!createFakeWalls)
            {
                _totalGemsDisplayed = 0;
                _scanningOrigin = Vector3.zero;

                RaycastResult[] rayHits = CreateRayHitInfos();

                // start tracking planes
                ActivateARFTrackables(true);
                EnableARFPlaneRendering(false);
                _arPlaneManager.planesChanged += OnPlanesChanged;
                EnableARFPlaneTracking(true);

                // initial scan around the room
                yield return GazeAroundRoomCR(_numGemsInitialScan, _gemDistanceInitialScan);

                // reset plane rendering
                EnableARFPlaneRendering(_arfPlaneRenderingEnabled);

                // Note: This coroutine is meant to be run in parallel, not yielded.
                Coroutine castRaysCR = StartCoroutine(CastRaysCR(rayHits));

                // update all player targets
                yield return StartCoroutine(TestRaycastResultsCR(rayHits));

                // find ceiling and floor
                FindCeilingAndFloor();

                // stop scanning
                EnableARFPlaneTracking(false);

                // stop running coroutines
                StopCoroutine(castRaysCR);
                _roomScanTargetController.Deactivate();
                _arPlaneManager.planesChanged -= OnPlanesChanged;

                // disable debug rendering
                EnableARFPlaneRendering(false);
                EnableDebugRendering(false);

                // build the room
                try
                {
                    roomBuilt = BuildVirtualRoom(rayHits);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("Exception " + ex.ToString());
                }

                // find platforms
                BuildPlatforms(_floorHeight + _minPlatformHeight, _floorHeight + _maxPlatformHeight);

                // disable the trackables - this disables physics
                ActivateARFTrackables(false);
            }

            // build fake room if build failed
            if (!roomBuilt)
            {
                ResetVirtualRoom();
                BuildFakeRoom();
            }

            DebugMenu.RemoveButton("ARFoundation/Debug Gaze");

            // completion audio
            GetComponent<AudioSource>()?.Play();

            // room complete
            VirtualRoom.SetRenderMode(VirtualWall.RenderMode.Debug, VirtualWall.RenderMode.Debug, VirtualWall.RenderMode.Debug, VirtualWall.RenderMode.Debug);
            RoomGenerationComplete();
        }

        void BuildPlatforms(float minHeight, float maxHeight)
        {
            foreach (ARPlane arPlane in _arPlaneManager.trackables)
            {
                // classification and alignment are incorrect so testing alignment from normal
                if (MathHelpers.IsVectorPointingUp(arPlane.normal) && 
                    arPlane.transform.position.y >= minHeight && 
                    arPlane.transform.position.y <= maxHeight)
                {                  
                    bool isReachable = false;

                    // is the platform reachable
                    List<RaycastHit> hits = PhysicsHelpers.RaycastAllSorted(_scanningOrigin, arPlane.transform.position - _scanningOrigin, 10f, _arPlanesLayer);
                    if (hits.Count>0 && hits[0].collider.gameObject==arPlane.gameObject)
                    {
                        isReachable = true;
                    }

                    // add to virtualroom
                    if (isReachable)
                    {
                        VirtualRoom.CreateVirtualWall(VirtualWall.WallType.Platform,
                            arPlane.transform.position,
                            new Vector3(arPlane.size.x, arPlane.size.y, 1.0f),
                            Quaternion.Euler(90, arPlane.transform.eulerAngles.y, 0));
                    }
                }
            }
        }

        bool BuildVirtualRoom(RaycastResult[] rayHits)
        {
            List<WallInfo> wallInfos = RaycastsToWalls(rayHits);
            return BuildVirtualRoomFromWalls(wallInfos);
        }

        /// <summary>
        /// Spawn gems around the origin for the user to gaze at.  
        /// This will ensure that the device has at least seen the entire room
        /// </summary>
        /// <param name="numTargets">how many gem targets to show</param>
        /// <param name="targetDistance">distance of each target from the origin</param>
        /// <returns></returns>
        IEnumerator GazeAroundRoomCR(int numTargets, float targetDistance)
        {
            float radsPerTarget = 2 * Mathf.PI / (float)numTargets;
            float currentRads = 0;

            for (int i=0; i < numTargets; i++)
            {
                currentRads += radsPerTarget;

                float gemHeight = _gemHeights[_totalGemsDisplayed % _gemHeights.Length];
                Vector3 fwd = new Vector3(Mathf.Sin(currentRads), 0, Mathf.Cos(currentRads));
                Vector3 targetPos = MathHelpers.Vector3AtYPos(_scanningOrigin + fwd * targetDistance, gemHeight);

                _roomScanTargetController.Activate(targetPos);

                while (!_roomScanTargetController.IsComplete)
                {
                    yield return null;
                }

                _roomScanTargetController.Deactivate();
                _totalGemsDisplayed++;

                yield return new WaitForSeconds(0.5f);
            }
        }

        void FindCeilingAndFloor()
        {
            _floorIsValid = false;
            _ceilingIsValid = false;
            _floorHeight = float.MaxValue;
            _ceilingHeight = float.MinValue;

            // first pass to find floor and ceiling based on classification
            foreach (ARPlane plane in _arPlaneManager.trackables)
            {
                if (plane.classification == UnityEngine.XR.ARSubsystems.PlaneClassification.Floor &&
                    plane.transform.position.y < _floorHeight)
                {
                    _floorIsValid = true;
                    _floorHeight = plane.transform.position.y;
                }
                    
                // use classification to detect ceiling
                if (plane.classification == UnityEngine.XR.ARSubsystems.PlaneClassification.Ceiling &&
                    plane.transform.position.y > _ceilingHeight)
                {
                    _ceilingIsValid = true;
                    _ceilingHeight = plane.transform.position.y;
                }
            }

            // missing floor?
            if (!_floorIsValid)
            {
                foreach (ARPlane plane in _arPlaneManager.trackables)
                {
                    // use normal to find lowest floor
                    if (plane.transform.position.y < _floorHeight &&
                        MathHelpers.IsVectorPointingUp(plane.normal))
                    {
                        _floorIsValid = true;
                        _floorHeight = plane.transform.position.y;
                    }
                }
            }

            // missing ceiling?
            if (!_ceilingIsValid)
            {
                foreach (ARPlane plane in _arPlaneManager.trackables)
                {
                    // use normal to find lowest floor
                    if (plane.transform.position.y > _ceilingHeight &&
                        MathHelpers.IsVectorPointingUp(-plane.normal))
                    {
                        _ceilingIsValid = true;
                        _ceilingHeight = plane.transform.position.y;
                    }
                }
            }
        }

        /// <summary>
        /// This method tests the raycasts tested synchronously in CastRaysCR.
        /// If a raycasts missed, pop a gem target for the user to gaze at.
        /// After a gem, don't show another one for _numSpokesToSkip spokes
        /// </summary>
        /// <param name="spokes">the raycast spokes</param>
        /// <returns></returns>
        IEnumerator TestRaycastResultsCR(RaycastResult[] spokes)
        {
            int _numSpokesToSkip = 0;

            foreach (RaycastResult spoke in spokes)
            {
                // wait until we've attempted to raycast this spoke.  raycasts happen synchronously in CastRaysCR
                yield return new WaitUntil(() => spoke.DidRaycast);

                _numSpokesToSkip--;
                if (_numSpokesToSkip <= 0)
                {
                    // no raycast hit, activate the player target
                    if (spoke.DidRaycastMiss)
                    {
                        // activate target
                        float gemHeight = _gemHeights[_totalGemsDisplayed % _gemHeights.Length];
                        float indicatorDistance = _gemDistance;
                        Vector3 targetPos = MathHelpers.Vector3AtYPos(spoke.StartPos + spoke.Direction * indicatorDistance, gemHeight);

                        _roomScanTargetController.Activate(targetPos);

                        // wait for player to gaze at / cancel target
                        while (!_roomScanTargetController.IsComplete)
                        {
                            yield return null;
                        }

                        // target indicator complete
                        _roomScanTargetController.Deactivate();
                        _totalGemsDisplayed++;

                        // after a gem skip the next _minSpokesBetweenGems raycasts
                        _numSpokesToSkip = _minSpokesBetweenGems;
                    }
                }
            }

            yield return null;
        }

        RaycastResult[] CreateRayHitInfos()
        {
            // build a list of raycast results in all directions
            RaycastResult[] rayHitInfo = new RaycastResult[_numSpokes];
            for (int i = 0; i < _numSpokes; i++)
            {
                float rads = 2 * Mathf.PI / (float)_numSpokes * i;
                rayHitInfo[i] = new RaycastResult(i, _scanningOrigin, new Vector3(Mathf.Sin(rads), 0, Mathf.Cos(rads)));
            }

            return rayHitInfo;
        }

        IEnumerator CastRaysCR(RaycastResult[] rayHits)
        {
            int numSpokesCurrentFrame = 0;
            Color debugColor = Color.red;

            while (true)
            {
                debugColor = (debugColor == Color.red) ? Color.blue : Color.red;
                
                for (int i = 0; i < _numSpokes; i++)
                {
                    RaycastResult rayHit = rayHits[i];

                    (ARPlane hitPlane, Vector3 hitPos) = RaycastToWall(rayHit.StartPos,
                        rayHit.Direction,
                        _defaultRayLength);

                    if (hitPlane != null)
                    {
                        rayHit.SetRayHit(hitPlane, hitPos);

                        // create debug marker
                        if (rayHit.DebugMarker == null)
                        {
                            rayHit.DebugMarker = DebugSphere.Create(_debugContainer, hitPos, 0.1f, debugColor);
                        }
                        // update debug marker
                        else
                        {
                            rayHit.DebugMarker.transform.position = hitPos;
                            rayHit.DebugMarker.SetColor(debugColor);
                            rayHit.DebugMarker.gameObject.SetActive(true);
                        }
                    }
                    else
                    {
                        // deactivate debug marker
                        if (rayHit.DebugMarker != null)
                        {
                            rayHit.DebugMarker.gameObject.SetActive(false);
                        }

                        rayHit.SetRayMissed(_defaultRayLength);
                    }

                    numSpokesCurrentFrame++;
                    if (numSpokesCurrentFrame > _numSpokesPerFrame)
                    {
                        yield return null;
                        numSpokesCurrentFrame = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Find the longest wall identified by our raycasts
        /// </summary>
        /// <param name="raycastResults">list of circular raycast results</param>
        /// <returns>index of longest wall</returns>
        (int,WallInfo) FindLongestWall(List<WallInfo> wallInfos)
        {
            // find the longest wall
            int longestWallIndex = -1;
            float longestWallSize = -1;
            WallInfo longestWall = null;

            for (int wallIndex = 0; wallIndex < wallInfos.Count; wallIndex++)
            {
                WallInfo currentWall = wallInfos[wallIndex];

                float wallSize = currentWall.LengthSqr;
                if (longestWall==null || wallSize > longestWallSize)
                {
                    longestWall = currentWall;
                    longestWallIndex = wallIndex;
                    longestWallSize = wallSize;
                }
            }

            return (longestWallIndex, longestWall);
        }

        List<WallInfo> RaycastsToWalls(RaycastResult[] raycastResults)
        {
            List<WallInfo> wallInfos = new List<WallInfo>();

            // group the raycasts into endpoints grouped by ARPlane ID
            string prevPlane = null;
            WallInfo wallInfo = null;
            foreach (RaycastResult raycast in raycastResults)
            {
                if (raycast.ARPlaneId != null)
                {
                    if (raycast.ARPlaneId == prevPlane)
                    {
                        wallInfo.r1 = raycast;
                    }
                    else
                    {
                        // nuke current wall if incomplete (only one corner)
                        if (wallInfo!=null && wallInfo.r1==null)
                        {
                            wallInfos.RemoveAt(wallInfos.Count - 1);
                            wallInfo = null;
                        }

                        // create new corner
                        wallInfo = new WallInfo() { r0 = raycast };
                        wallInfos.Add(wallInfo);
                        prevPlane = raycast.ARPlaneId;
                    }
                }
            }

            // combine the first and last walls if they're the same plane id
            if (wallInfos.Count > 0)
            {
                WallInfo firstWall = wallInfos[0];
                WallInfo lastWall = wallInfos[wallInfos.Count - 1];
                if (firstWall.r0.ARPlaneId == lastWall.r0.ARPlaneId)
                {
                    firstWall.r0 = lastWall.r0;
                    wallInfos.RemoveAt(wallInfos.Count - 1);
                }
            }

            return wallInfos;
        }

        bool BuildVirtualRoomFromWalls(List<WallInfo> wallInfos)
        {
            ResetVirtualRoom();

            // update ceiling and floor height based on detected or not detected
            if (!_floorIsValid)
            {
                if (_ceilingIsValid)
                {
                    _floorHeight = _ceilingHeight - _defaultWallHeight;
                    _floorIsValid = true;
                }
                else
                {
                    _floorHeight = _scanningOrigin.y - _defaultFloorOffset;
                    _floorIsValid = true;
                }
            }

            if (!_ceilingIsValid)
            {
                _ceilingHeight = _floorHeight + _defaultWallHeight;
                _ceilingIsValid = true;
            }

            // create floor
            BoxCollider boxCollider = VirtualRoom.Prefab.GetComponent<BoxCollider>();
            VirtualRoom.CreateVirtualWall(VirtualWall.WallType.Floor,
                MathHelpers.Vector3AtYPos(_scanningOrigin, _floorHeight),
                new Vector3(10f, 10f, VirtualWall.FLOOR_DEPTH),
                Quaternion.Euler(90, 0, 0));

            // create ceiling
            VirtualRoom.CreateVirtualWall(VirtualWall.WallType.Ceiling,
                MathHelpers.Vector3AtYPos(_scanningOrigin, _ceilingHeight),
                new Vector3(10f, 10f, VirtualWall.WALL_DEPTH),
                Quaternion.Euler(-90, 0, 0));

            // common wall params based on floor/ceiling
            float wallHeight = _ceilingHeight - _floorHeight;
            float wallYPos = _floorHeight + wallHeight * 0.5f;

            // find the longest wall
            (int longestWallIndex, WallInfo longestWall) = FindLongestWall(wallInfos);
            if (longestWall.LengthSqr < (_minLongestWallLength*_minLongestWallLength))
            {
                ScreenLog.Log($"Scan failed.  Longest wall too short {Mathf.Sqrt(longestWall.LengthSqr):0.#} < {_minLongestWallLength:0.#}");
                Debug.Log($"BuildVirtualRoomFromWalls failed.  Longest wall is too short: {Mathf.Sqrt(longestWall.LengthSqr)} < {_minLongestWallLength}");
                return false;
            }

            // build the virtual walls and the edges between them
            int currentWallIndex = longestWallIndex;
            do
            {
                WallInfo currentWall = wallInfos[currentWallIndex];
                VirtualRoom.CreateVirtualWall(VirtualWall.WallType.Wall, MathHelpers.Vector3AtYPos(currentWall.r0.HitPos,wallYPos), MathHelpers.Vector3AtYPos(currentWall.r1.HitPos,wallYPos), wallHeight);

                int nextWallIndex = (currentWallIndex + 1) % wallInfos.Count;
                WallInfo nextWall = wallInfos[nextWallIndex];
                VirtualRoom.CreateVirtualWall(VirtualWall.WallType.Wall, MathHelpers.Vector3AtYPos(currentWall.r1.HitPos,wallYPos), MathHelpers.Vector3AtYPos(nextWall.r0.HitPos,wallYPos), wallHeight);

                currentWallIndex = nextWallIndex;
            } while (currentWallIndex != longestWallIndex);

            // room is created, is it big enough?
            Vector3 roomSize = VirtualRoom.CalculateRoomSize();
            if (roomSize.x < _minRoomSize || roomSize.z < _minRoomSize)
            {
                ScreenLog.Log($"Scan failed.  Too small {roomSize.x:0.#} x {roomSize.z:0.#} < {_minRoomSize:0.#} x {_minRoomSize:0.#}");
                Debug.Log($"BuildVirtualRoomFromWalls failed.  Not big enough: {roomSize.x} x {roomSize.z} < {_minRoomSize} x {_minRoomSize}");
                VirtualRoom.GenerateFakeRoomFromPrimaryWall(_defaultRoomLength);
                return true;
            }

            // mark the room as complete
            VirtualRoom.FinalizeRoom();

            return true;
        }

        IEnumerator WaitForARFPlanesCR(float delaySeconds)
        {
            bool planesChanged = false;

            _arPlaneManager.planesChanged += _OnPlanesChanged;
            EnableARFPlaneTracking(true);

            while (!planesChanged)
            {
                yield return null;
            }

            _arPlaneManager.planesChanged -= _OnPlanesChanged;

            yield return new WaitForSeconds(delaySeconds);

            void _OnPlanesChanged(ARPlanesChangedEventArgs args)
            {
                planesChanged = true;
            }
        }

        void DestroyDebugObjects()
        {
            _debugContainer.gameObject.DestroyChildren();
        }

        (ARPlane,Vector3) RaycastToWall(Vector3 startPos, Vector3 direction, float distance)
        {
            List<RaycastHit> hits = PhysicsHelpers.RaycastAllSorted(startPos, direction, distance, _arPlanesLayer);
            foreach (RaycastHit hitInfo in hits)
            {
                ARPlane arPlane = hitInfo.collider.GetComponent<ARPlane>();
                Vector3 hitPos = hitInfo.point;

                Debug.Assert(arPlane != null, "Hit an object with no ARPlane");

                if (arPlane != null)
                {
                    // we only care about walls
                    if (arPlane.classification == UnityEngine.XR.ARSubsystems.PlaneClassification.Wall)
                    {
                        return (arPlane, hitPos);
                    }
                }
            }

            return (null, Vector3.zero);
        }

        public override void EnableDebugRendering(bool enable)
        {
            if (_debugContainer!=null)
            {
                _debugContainer.gameObject.SetActive(enable);
            }
        }

        /// <summary>
        /// Activate / deactivate the trackables.  
        /// To enable rendering, use EnableRendering.
        /// This is used to disable physics
        /// </summary>
        void ActivateARFTrackables(bool activated)
        {
            foreach (ARPlane plane in _arPlaneManager.trackables)
            {
                plane.gameObject.SetActive(activated);
            }
        }
    }
}
