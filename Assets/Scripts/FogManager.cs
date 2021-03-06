using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FogManager : MonoBehaviour, IResetable
{
    public bool UseFog;
    public Color FogColor;
    public float FogStart, FogEnd;

    private bool resetActivation;
    private Color resetColor;
    private float resetStart, resetEnd;

    public void Awake()
    {
        RenderSettings.fog = UseFog;
        if (RenderSettings.fog)
        {
            RenderSettings.fogMode = FogMode.Linear;
        }
        RenderSettings.fogColor = FogColor;
        RenderSettings.fogStartDistance = FogStart;
        RenderSettings.fogEndDistance = FogEnd;

        resetActivation = RenderSettings.fog;
        resetColor = RenderSettings.fogColor;
        resetStart = RenderSettings.fogStartDistance;
        resetEnd = RenderSettings.fogEndDistance;
    }

    public void ObjectReset()
    {
        DOTween.Kill("FogChanger");
        RenderSettings.fog = resetActivation;
        RenderSettings.fogColor = resetColor;
        RenderSettings.fogStartDistance = resetStart;
        RenderSettings.fogEndDistance  = resetEnd;
    }
}
