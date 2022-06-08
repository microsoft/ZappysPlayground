
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSPlayground.Core.Spatial
{
    public class VirtualWall : MonoBehaviour
    {
        public const float FLOOR_DEPTH = 1.0f;
        public const float WALL_DEPTH = 0.1f;

        [SerializeField] Transform _meshTransform;
        [SerializeField] Renderer _renderer;
        [SerializeField] SpriteRenderer _edgeSprite;
        [SerializeField] Material _debugMaterial;
        [SerializeField] Material _virtualMaterial;
        [SerializeField] Material _occlusionMaterial;
        BoxCollider _collider;

        private RenderMode _renderMode;
        private MaterialPropertyBlock _propertyBlock;

        public Renderer Renderer => _renderer;

        public float AreaSqr { get { return _meshTransform.localScale.x * _meshTransform.localScale.x + _meshTransform.localScale.y * _meshTransform.localScale.y; } }

        public WallType Type { get; private set; }

        /// <summary>
        /// Type of plane
        /// </summary>
        public enum WallType
        {
            Wall,
            Floor,
            Ceiling,
            Platform
        }

        /// <summary>
        /// Wall render modes
        /// </summary>
        public enum RenderMode
        {
            ///<summary>Renderer disabled</summary>
            Disabled,
            ///<summary>Render with a debug material</summary>
            Debug,
            ///<summary>Render with a simulated wall material</summary>
            Virtual,
            ///<summary>Render with an invisible occlusion material</summary>
            Occlusion,
        }

        /// <summary>
        /// Initialize the VirtualWall
        /// </summary>
        /// <param name="wallType">the type of plane</param>
        public void Init(WallType wallType)
        {
            _collider = GetComponent<BoxCollider>();
            Type = wallType;
        }

        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();
            SetRenderMode(RenderMode.Disabled);
        }

        /// <summary>
        /// Set whether the wall renderer is disabled, visualized, or uses an occlusion shader
        /// </summary>
        public void SetRenderMode(RenderMode renderMode)
        {
            _renderMode = renderMode;

            switch (renderMode)
            {
                case RenderMode.Disabled:
                    _renderer.enabled = false;
                    _edgeSprite.enabled = false;
                    break;
                case RenderMode.Debug:
                    _renderer.enabled = true;
                    _renderer.sharedMaterial = _debugMaterial;
                    _edgeSprite.enabled = true;
                    break;
                case RenderMode.Virtual:
                    _renderer.enabled = true;
                    _renderer.sharedMaterial = _virtualMaterial;
                    _edgeSprite.enabled = false;
                    break;
                case RenderMode.Occlusion:
                    _renderer.enabled = true;
                    _renderer.sharedMaterial = _occlusionMaterial;
                    _edgeSprite.enabled = false;
                    break;
            }
        }

        /// <summary>
        /// Set the scale and size of components
        /// </summary>
        /// <returns></returns>
        public void SetSize(Vector3 size)
        {
            transform.localScale = size;

            _edgeSprite.transform.localScale = new Vector3(1f / size.x, 1f / size.y, 1f);
            _edgeSprite.size = new Vector2(size.x, size.y);
        }

        /// <summary>
        /// Get the first edge point
        /// </summary>
        /// <returns></returns>
        public Vector3 GetEdge0()
        {
            return _meshTransform.position - _meshTransform.right * _meshTransform.localScale.x * 0.5f;
        }

        /// <summary>
        /// Get the second edge point
        /// </summary>
        /// <returns></returns>
        public Vector3 GetEdge1()
        {
            return _meshTransform.position + _meshTransform.right * _meshTransform.localScale.x * 0.5f;
        }

        /// <summary>
        /// Set physics enabled state
        /// </summary>
        public void EnablePhysics(bool enable)
        {
            GetComponent<Collider>().enabled = enable;
        }

    }
}
