// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

/// <summary>
/// Rotates a transform around a provided axis at a constant rate each frame.
/// </summary>

using UnityEngine;

public class RotateOverTime : MonoBehaviour
{
	[SerializeField] private Vector3 _rotationAxis;
	[SerializeField] private float _rotationSpeed;

	[Tooltip("[Optional] Set the Transform to rotate. If left empty the Transform of the object this script is attached to will be used.")]
	[SerializeField] private Transform _targetTransformOverride;

	private Transform _targetTransform;

	private void Awake()
	{
		_targetTransform = _targetTransformOverride == null ? transform : _targetTransformOverride;
	}

	private void Update()
	{
		_targetTransform.Rotate(_rotationAxis, _rotationSpeed * Time.deltaTime);
	}
}
