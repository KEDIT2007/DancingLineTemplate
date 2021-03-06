using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraBackground : MonoBehaviour
{
    private Camera cam;

    public void Start()
    {
        cam = GetComponent<Camera>();
        if (RenderSettings.fog)
        {
            cam.backgroundColor = RenderSettings.fogColor;
            cam.clearFlags = CameraClearFlags.SolidColor;
        }
        else
        {
            cam.clearFlags = CameraClearFlags.Skybox;
        }
    }

    public void Update()
    {
        if (RenderSettings.fog)
        {
            cam.backgroundColor = RenderSettings.fogColor;
            cam.clearFlags = CameraClearFlags.SolidColor;
            if (RenderSettings.fogMode == FogMode.Linear)
            {
                cam.farClipPlane = RenderSettings.fogEndDistance;
            }
        }
        else
        {
            cam.clearFlags = CameraClearFlags.Skybox;
        }
    }
}
