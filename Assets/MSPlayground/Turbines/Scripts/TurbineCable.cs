
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using MSPlayground.Common.Helper;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurbineCable : MonoBehaviour
{
    public const string CablePulseStrengthProperty = "_PulseStrength";
    public const string CablePulseOffsetProperty = "_PulseOffset";
    public const string CablePulseColorProperty = "_PulseColor";

    [SerializeField] RendererCollectionHelper _rendererHelper;
    [SerializeField] private float _cablePulseSpeedMin = 0.25f;
    [SerializeField] private float _cablePulseSpeedMax = 1.00f;
    [SerializeField] private Color _defaultPulseColor = Color.cyan;
    [SerializeField] private float _direction = 1f;

    float _cableOffset = 0;
    float _power = 0;

    private void Start()
    {
        SetPower(0);
        SetColor(_defaultPulseColor);
        SetForward(true);
    }

    public void SetForward(bool isForward)
    {
        _direction = isForward ? 1f : -1f;
    }

    public void SetPower(float power)
    {
        _power = power;
        _rendererHelper.SetFloat(TurbineCable.CablePulseStrengthProperty, power==0 ? 0 : 1f);
    }

    public void SetMaxSpeed(float maxSpeed)
    {
        _cablePulseSpeedMax = maxSpeed;
    }

    private void Update()
    {
        _cableOffset = (_cableOffset + Mathf.Lerp(_cablePulseSpeedMin, _cablePulseSpeedMax, _power) * Time.deltaTime * _direction) % 1.0f;
        _rendererHelper.SetFloat(TurbineCable.CablePulseOffsetProperty, _cableOffset);
    }

    public void SetColor(Color color)
    {
        _rendererHelper.SetColor(CablePulseColorProperty, color);
    }
}
