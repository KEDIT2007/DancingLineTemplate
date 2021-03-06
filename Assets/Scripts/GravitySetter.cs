using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravitySetter : MonoBehaviour
{
    public Vector3 DefaultGravity = new Vector3(0, -8f, 0);

    void Awake()
    {
        Physics.gravity = DefaultGravity;
    }
}
