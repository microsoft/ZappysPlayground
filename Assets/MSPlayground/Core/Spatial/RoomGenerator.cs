
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using MSPlayground.Core.Gamestate;
using MSPlayground.Core.Utils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace MSPlayground.Core.Spatial
{
    public abstract class RoomGenerator : MonoBehaviour
    {
        protected ARPlaneManager _arPlaneManager;

        public VirtualRoom VirtualRoom { get; protected set; }

        [Tooltip("Prefab of a virtual room")]
        [SerializeField] GameObject _virtualRoomPrefab;

        [Tooltip("Floor offset from camera position when in editor / floor not found")]
        [SerializeField] protected float _defaultFloorOffset = -1.5f;

        [Tooltip("Wall height used when in editor / ceiling not found")]
        [SerializeField] protected float _defaultWallHeight = 2.8f;

        [Tooltip("Room length used when generating fake room")]
        [SerializeField] protected float _defaultRoomLength = 6.0f;

        [Tooltip("Room width used when generating fake room")]
        [SerializeField] protected float _defaultRoomWidth = 5.0f;

        /// <summary>
        /// Start
        /// </summary>
        protected virtual void Start()
        {
            _arPlaneManager = GameObject.FindObjectOfType<ARPlaneManager>();

            // disable the plane manager until we want planes
            EnableARFPlaneTracking(false);
        }

        /// <summary>
        /// Reset and setup for a new scan
        /// </summary>
        public virtual void Reset()
        {
            ResetVirtualRoom();
        }

        public abstract void StartRoomScan(bool fakeWalls);
        public abstract void EnableDebugRendering(bool enable);

        /// <summary>
        /// Reset the virtual room
        /// </summary>
        protected void ResetVirtualRoom()
        {
            if (VirtualRoom != null)
            {
                VirtualRoom.DestroyWalls();
                GameObject.Destroy(VirtualRoom.gameObject);
                VirtualRoom = null;
            }

            VirtualRoom = GameObject.Instantiate(_virtualRoomPrefab).GetComponent<VirtualRoom>();
        }

        /// <summary>
        /// Enable/disable ARFoundation plane tracking
        /// </summary>
        /// <param name="enable">desired enabled state</param>
        protected void EnableARFPlaneTracking(bool enable)
        {
            _arPlaneManager.enabled = enable;
        }

        /// <summary>
        /// Called internally when the room generation is complete
        /// </summary>
        protected void RoomGenerationComplete()
        {
            GlobalEventSystem.Fire<RoomScanCompleteEvent>(new RoomScanCompleteEvent());
        }

        /// <summary>
        /// Ignore scanning and create fake walls, primarily used in editor
        /// </summary>
        protected void BuildFakeRoom()
        {
            ResetVirtualRoom();
            VirtualRoom.GenerateFakeRoom(_defaultFloorOffset, _defaultWallHeight, _defaultRoomLength, _defaultRoomWidth);
        }

        /// <summary>
        /// Save out the virtual walls into the gamestate
        /// </summary>
        /// <param name="refPointDict"></param>
        public void WriteVirtualRoomToGamestate(GameState gamestate)
        {
            VirtualRoom.WriteToGamestate(gamestate);

            // anchor the room
            VirtualRoom.transform.SetParent(gamestate.AnchorObject.transform, true);
        }

        /// <summary>
        /// Generate virtual room from a saved gamestate
        /// </summary>
        /// <param name="gamestate">the gamestate to lookup walls from</param>
        public void GenerateVirtualRoomFromGamestate(GameState gamestate)
        {
            VirtualRoom.GenerateFromGamestate(gamestate);

            // anchor the room
            VirtualRoom.transform.SetParent(gamestate.AnchorObject.transform, true);
        }
    }
}
