
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using MSPlayground.Core.Gamestate;
using MSPlayground.Core.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace MSPlayground.Core.Spatial
{
    public class VirtualRoom : MonoBehaviour
    {
        // refpoint names for saving the walls into the gamestate
        const string REFPOINT_FLOOR = "Floor";
        const string REFPOINT_CEILING = "Ceiling";
        const string REFPOINT_WALL = "Wall";
        const string REFPOINT_PLATFORM = "Platform";

        public List<VirtualWall> Platforms = new List<VirtualWall>();
        public List<VirtualWall> Walls = new List<VirtualWall>();
        public VirtualWall Floor = null;
        public VirtualWall Ceiling = null;
        public Transform RoomCenter { get; private set; }

        public GameObject Prefab => _virtualWallPrefab;

        public float CeilingHeight {  get { return Ceiling.transform.position.y - Floor.transform.position.y; } }

        [Tooltip("Prefab of a virtual wall")]
        [SerializeField] GameObject _virtualWallPrefab;

        VirtualWall.RenderMode _wallRenderMode = VirtualWall.RenderMode.Disabled;
        VirtualWall.RenderMode _floorRenderMode = VirtualWall.RenderMode.Disabled;
        VirtualWall.RenderMode _ceilingRenderMode = VirtualWall.RenderMode.Disabled;
        VirtualWall.RenderMode _platformRenderMode = VirtualWall.RenderMode.Disabled;

        void CreateCenterOfRoom()
        {
            (Vector3 minExtents, Vector3 maxExtents) = CalculateRoomExtents();

            Vector3 roomCenter = new Vector3((minExtents.x + maxExtents.x) * 0.5f, (minExtents.y + maxExtents.y)*0.5f, (minExtents.z + maxExtents.z) * 0.5f);

            RoomCenter = new GameObject().transform;
            RoomCenter.name = "Center";
            RoomCenter.SetParent(transform, true);
            RoomCenter.transform.position = roomCenter;
            RoomCenter.transform.rotation = Quaternion.LookRotation(MathHelpers.Vector3AtYPos(Walls[0].transform.position - RoomCenter.position, 0), Vector3.up);
        }

        /// <summary>
        /// Call this when all walls have been added
        /// </summary>
        public void FinalizeRoom()
        {
            CreateCenterOfRoom();

            // sort platforms by decreasing size
            if (Platforms!=null && Platforms.Count>0)
            {
                Platforms.Sort((x, y) => x.AreaSqr < y.AreaSqr ? -1 : 1);
            }
        }

        public VirtualWall CreateVirtualWall(VirtualWall.WallType wallType, Vector3 p0, Vector3 p1, float height)
        {
            Vector3 position = (p0 + p1) * 0.5f;
            Vector3 scale = new Vector3((p0 - p1).magnitude, height, wallType==VirtualWall.WallType.Floor ? VirtualWall.FLOOR_DEPTH : VirtualWall.WALL_DEPTH);
            Quaternion rotation = Quaternion.LookRotation(Vector3.Cross(p1 - p0, Vector3.up), Vector3.up);

            return CreateVirtualWall(wallType, position, scale, rotation);
        }

        public VirtualWall CreateVirtualWall(VirtualWall.WallType wallType, Vector3 position, Vector3 scale, Quaternion rotation)
        {
            GameObject virtualWallGo = GameObject.Instantiate(_virtualWallPrefab);
            Transform wallTransform = virtualWallGo.transform;
            wallTransform.SetParent(transform, true);
            wallTransform.position = position;
            wallTransform.rotation = rotation;

            VirtualWall virtualWall = virtualWallGo.GetComponent<VirtualWall>();
            virtualWall.Init(wallType);
            virtualWall.SetSize(scale);

            if (wallType == VirtualWall.WallType.Ceiling)
            {
                virtualWall.name = "Ceiling";
                Ceiling = virtualWall;
                virtualWall.SetRenderMode(_ceilingRenderMode);
            }
            else if (wallType == VirtualWall.WallType.Floor)
            {
                virtualWall.name = "Floor";
                Floor = virtualWall;
                virtualWall.SetRenderMode(_floorRenderMode);
            }
            else if (wallType==VirtualWall.WallType.Wall)
            {
                virtualWall.name = $"Wall.{Walls.Count}";
                Walls.Add(virtualWall);
                virtualWall.SetRenderMode(_wallRenderMode);
            }
            else if (wallType==VirtualWall.WallType.Platform)
            {
                virtualWall.name = $"Platform.{Platforms.Count}";
                Platforms.Add(virtualWall);
                virtualWall.SetRenderMode(_platformRenderMode);
            }

            return virtualWall;
        }

        /// <summary>
        /// Enable or disable rendering for all walls.
        /// </summary>
        /// <param name="enabled">Whether or not to render a visible material. If false, either disables the renderer or renders an occlusion material depending on wall type.</param>
        /// <param name="debugMode">When visible, either use the debug or virtual material.</param>
        public void SetRenderMode(VirtualWall.RenderMode wallRenderMode, VirtualWall.RenderMode floorRenderMode, VirtualWall.RenderMode ceilingRenderMode, VirtualWall.RenderMode platformRenderMode)
        {
            _wallRenderMode = wallRenderMode;
            _floorRenderMode = floorRenderMode;
            _ceilingRenderMode = ceilingRenderMode;
            _platformRenderMode = platformRenderMode;

            if (Walls != null)
            {
                foreach (VirtualWall wall in Walls)
                {
                    wall.SetRenderMode(_wallRenderMode);
                }
            }

            if (Platforms!=null)
            {
                foreach (VirtualWall wall in Platforms)
                {
                    wall.SetRenderMode(_platformRenderMode);
                }
            }

            if (Floor != null)
            {
                Floor.SetRenderMode(_floorRenderMode);
            }

            if (Ceiling!=null)
            {
                Ceiling.SetRenderMode(_ceilingRenderMode);
            }
        }

        /// <summary>
        /// Write out the virtual room planes to the gamestates RefPointDict so it can be recreated later
        /// </summary>
        /// <param name="gamestate"></param>
        public void WriteToGamestate(GameState gamestate)
        {
            ReferencePointDict refPointDict = gamestate.HubState.ReferencePoints;
            Transform anchorTransform = gamestate.AnchorObject.transform;

            refPointDict.AddRefPoint(REFPOINT_FLOOR, Floor.transform, anchorTransform);
            refPointDict.AddRefPoint(REFPOINT_CEILING, Ceiling.transform, anchorTransform);

            for (int i = 0; i < Walls.Count; i++)
            {
                refPointDict.AddRefPoint($"{REFPOINT_WALL}.{i}", Walls[i].transform, anchorTransform);
            }

            for (int i=0; i < Platforms.Count; i++)
            {
                refPointDict.AddRefPoint($"{REFPOINT_PLATFORM}.{i}", Platforms[i].transform, anchorTransform);
            }
        }

        /// <summary>
        /// Generate the virtual room by lookng up values from GameState reference points
        /// </summary>
        /// <param name="gamestate">gametsate to generate virtual room from</param>
        public void GenerateFromGamestate(GameState gamestate)
        {
            ReferencePointDict refPointDict = gamestate.HubState.ReferencePoints;
            Transform anchorTransform = gamestate.AnchorObject.transform;

            Floor = GenerateWallFromGamestate(REFPOINT_FLOOR, VirtualWall.WallType.Floor);
            Ceiling = GenerateWallFromGamestate(REFPOINT_CEILING, VirtualWall.WallType.Ceiling);

            int index = 0;
            bool foundWall = false;

            // generate walls
            do
            {
                foundWall = GenerateWallFromGamestate($"{REFPOINT_WALL}.{index++}", VirtualWall.WallType.Wall);
            } while (foundWall);

            // generate platforms
            index = 0;
            do
            {
                foundWall = GenerateWallFromGamestate($"{REFPOINT_PLATFORM}.{index++}", VirtualWall.WallType.Platform);
            } while (foundWall);

            FinalizeRoom();

            VirtualWall GenerateWallFromGamestate(string refPointId, VirtualWall.WallType planeType)
            {
                VirtualWall virtualWall = null;

                ReferencePointDict.ReferencePoint refPoint = refPointDict.GetRefPoint(refPointId);

                if (refPoint != null)
                {
                    virtualWall = CreateVirtualWall(planeType, Vector3.zero, Vector3.one, Quaternion.identity);
                    refPoint.ApplyToTransform(virtualWall.transform, anchorTransform);
                    virtualWall.SetSize(virtualWall.transform.localScale);
                }

                return virtualWall;
            }
        }

        /// <summary>
        /// Create a fake room given that a ceiling, floor, and exactly one wall have already been created
        /// </summary>
        /// <param name="roomLength"></param>
        /// <returns></returns>
        public bool GenerateFakeRoomFromPrimaryWall(float roomLength)
        {
            if (Floor==null || Ceiling==null || Walls.Count<1)
            {
                Debug.LogError("CompleteFakeRoom requires exactly ceiling, floor, and 1 wall already created");
                return false;
            }

            // remove all except wall 1
            while (Walls.Count>1)
            {
                VirtualWall wall = Walls[1];

                GameObject.Destroy(wall.gameObject);
                Walls.RemoveAt(1);
            }

            // calculate corners
            VirtualWall primaryWall = Walls[0];
            Vector3 wallEdge0 = primaryWall.transform.position - primaryWall.transform.right * primaryWall.transform.localScale.x * 0.5f;
            Vector3 wallEdge1 = primaryWall.transform.position + primaryWall.transform.right * primaryWall.transform.localScale.x * 0.5f;
            Vector3 wallEdge2 = wallEdge1 - primaryWall.transform.forward * roomLength;
            Vector3 wallEdge3 = wallEdge0 - primaryWall.transform.forward * roomLength;
            float wallHeight = primaryWall.transform.localScale.y;

            // create walls
            CreateVirtualWall(VirtualWall.WallType.Wall, wallEdge1, wallEdge2, wallHeight);
            CreateVirtualWall(VirtualWall.WallType.Wall, wallEdge2, wallEdge3, wallHeight);
            CreateVirtualWall(VirtualWall.WallType.Wall, wallEdge3, wallEdge0, wallHeight);

            FinalizeRoom();

            return true;
        }

        public void GenerateFakeRoom(float floorOffset, float wallHeight, float roomLength, float roomWidth)
        {
            Transform startTransform = Camera.main.transform;
            Vector3 startEulers = startTransform.eulerAngles;

            // create floor
            float floorYPos = startTransform.position.y + floorOffset;
            BoxCollider boxCollider = _virtualWallPrefab.GetComponent<BoxCollider>();
            CreateVirtualWall(VirtualWall.WallType.Floor,
                MathHelpers.Vector3AtYPos(startTransform.position, floorYPos),
                new Vector3(10f, 10f, VirtualWall.FLOOR_DEPTH),
                Quaternion.Euler(90, 0, 0));

            // create ceiling
            float ceilingYPos = floorYPos + wallHeight;
            CreateVirtualWall(VirtualWall.WallType.Ceiling,
                MathHelpers.Vector3AtYPos(startTransform.position, ceilingYPos),
                new Vector3(10f, 10f, VirtualWall.WALL_DEPTH),
                Quaternion.Euler(-90, 0, 0));

            float wallYPos = (floorYPos + ceilingYPos) / 2.0f;

            // create 4 walls

            CreateVirtualWall(VirtualWall.WallType.Wall,
                MathHelpers.Vector3AtYPos(startTransform.position + startTransform.forward * roomLength / 2.0f, wallYPos),
                new Vector3(roomWidth, wallHeight, VirtualWall.WALL_DEPTH),
                Quaternion.Euler(0, startEulers.y, 0));

            CreateVirtualWall(VirtualWall.WallType.Wall,
                MathHelpers.Vector3AtYPos(startTransform.position + startTransform.right * roomWidth / 2.0f, wallYPos),
                new Vector3(roomLength, wallHeight, VirtualWall.WALL_DEPTH),
                Quaternion.Euler(0, startEulers.y + 90.0f, 0));

            CreateVirtualWall(VirtualWall.WallType.Wall,
                MathHelpers.Vector3AtYPos(startTransform.position - startTransform.forward * roomLength / 2.0f, wallYPos),
                new Vector3(roomWidth, wallHeight, VirtualWall.WALL_DEPTH),
                Quaternion.Euler(0, startEulers.y + 180.0f, 0));

            CreateVirtualWall(VirtualWall.WallType.Wall,
                MathHelpers.Vector3AtYPos(startTransform.position - startTransform.right * roomWidth / 2.0f, wallYPos),
                new Vector3(roomLength, wallHeight, VirtualWall.WALL_DEPTH),
                Quaternion.Euler(0, startEulers.y - 90.0f, 0));

            FinalizeRoom();
        }

        public void DestroyWalls()
        {
            if (Floor!=null)
            {
                GameObject.Destroy(Floor.gameObject);
                Floor = null;
            }

            if (Ceiling!=null)
            {
                GameObject.Destroy(Ceiling.gameObject);
                Ceiling = null;
            }

            if (Walls != null)
            {
                foreach (VirtualWall wall in Walls)
                {
                    if (wall != null)
                    {
                        GameObject.Destroy(wall.gameObject);
                    }
                }
                Walls = null;
            }

            if (RoomCenter != null)
            {
                GameObject.Destroy(RoomCenter.gameObject);
                RoomCenter = null;
            }
        }

        /// <summary>
        /// Hepler to raycast against the room
        /// </summary>
        /// <param name="origin">origin of raycast</param>
        /// <param name="direction">direction of raycast</param>
        /// <param name="distance">distance</param>
        /// <param name="raycastHit">hit result</param>
        /// <returns>indicates whether raycast hit or not</returns>
        public bool RaycastAgainstRoom(Vector3 origin, Vector3 direction, float distance, out RaycastHit raycastHit)
        {
            return Physics.Raycast(origin, direction, out raycastHit, distance, 1 << gameObject.layer);
        }

        /// <summary>
        /// Helper to boxcast against the room
        /// </summary>
        /// <param name="origin">origin of boxcast</param>
        /// <param name="halfExtents">half extents of the box to cast</param>
        /// <param name="direction">direction of cast</param>
        /// <param name="boxRotation">orientation of the box</param>
        /// <param name="distance">maximum cast distance</param>
        /// <param name="raycastHit">hit result</param>
        /// <returns></returns>
        public bool BoxcastAgainstRoom(Vector3 origin, Vector3 halfExtents, Vector3 direction, Quaternion boxRotation, float distance, out RaycastHit raycastHit)
        {
            bool hit = Physics.BoxCast(origin, halfExtents, direction, out raycastHit, boxRotation, distance, 1 << gameObject.layer);
            return hit;
        }

        /// <summary>
        /// Calculate the extents of the room
        /// </summary>
        /// <returns>the min and max extents of the room</returns>
        (Vector3,Vector3) CalculateRoomExtents()
        {
            Vector3 minBounds = new Vector3(float.MaxValue, 0, float.MaxValue);
            Vector3 maxBounds = new Vector3(float.MinValue, 0, float.MinValue);

            foreach (VirtualWall wall in Walls)
            {
                minBounds.x = Mathf.Min(minBounds.x, wall.transform.position.x);
                minBounds.z = Mathf.Min(minBounds.z, wall.transform.position.z);
                maxBounds.x = Mathf.Max(maxBounds.x, wall.transform.position.x);
                maxBounds.z = Mathf.Max(maxBounds.z, wall.transform.position.z);
            }

            minBounds.y = Floor.transform.position.y;
            maxBounds.y = Ceiling.transform.position.y;

            return (minBounds, maxBounds);
        }

        /// <summary>
        /// Calculate the extents of the room
        /// </summary>
        /// <returns></returns>
        public Vector3 CalculateRoomSize()
        {
            Vector3 minBounds;
            Vector3 maxBounds;

            (minBounds, maxBounds) = CalculateRoomExtents();

            Vector3 size = new Vector3(maxBounds.x - minBounds.x, 0, maxBounds.z - minBounds.z);
            return size;
        }

        /// <summary>
        /// Set physics states for walls, floor, ceiling, platforms
        /// </summary>
        /// <param name="enable"></param>
        public void EnablePhysics(bool enableWalls, bool enableFloor, bool enableCeiling, bool enablePlatforms)
        {
            if (Walls != null)
            {
                foreach (VirtualWall virtualWall in Walls)
                {
                    virtualWall.EnablePhysics(enableWalls);
                }
            }

            if (Platforms!=null)
            {
                foreach (VirtualWall virtualWall in Platforms)
                {
                    virtualWall.EnablePhysics(enablePlatforms);
                }
            }

            Floor?.EnablePhysics(enableFloor);
            Ceiling?.EnablePhysics(enableCeiling);
        }
    }
}
