using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour, IResetable
{
    public Transform Line;
    public Vector3 pivotOffset = Vector3.zero;
    private float x = 0.0f;
    private float y = 0.0f;
    private float z = 0.0f;
    [HideInInspector]
    public Vector3 position = Vector3.zero;
    public float targetX = 45f;
    public float targetY = 45f;
    public float targetZ = 0f;
    private Vector3 Velocity = Vector3.zero;
    public float TargetDistance = 20f;
    public float SmoothTime = 1f;
    public float needtime = 1f;
    private float xVelocity = 1f;
    private float yVelocity = 1f;
    private float zVelocity = 1f;
    private bool lineDead = false;

    void Die(object sender, LineEventArgs e)
    {
        //Debug.Log("Line Died");
        lineDead = true;
    }

    void Start()
    {
        FindObjectOfType<MainLine>().LineDie += Die;
        position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        x = targetX;
        y = targetY;
        z = targetZ;
        var angles = transform.eulerAngles;
    }

    void FixedUpdate()
    {
        if (!lineDead)
        {
            x = Mathf.SmoothDampAngle(x, targetX, ref xVelocity, needtime);
            y = Mathf.SmoothDampAngle(y, targetY, ref yVelocity, needtime);
            z = Mathf.SmoothDampAngle(z, targetZ, ref zVelocity, needtime);
            Quaternion rotation = Quaternion.Euler(x, y, z);
            transform.position = position;
            position = Vector3.SmoothDamp(transform.position, Line.position + rotation * new Vector3(0, 0, -TargetDistance) + pivotOffset, ref Velocity, SmoothTime);
            transform.rotation = rotation;
        }
    }

    [LastCallWhenReset, FirstCallWhenReset]
    public void ObjectReset()
    {
        FindObjectOfType<MainLine>().LineDie += Die;
        lineDead = false;
        transform.localRotation = Quaternion.Euler(x, y, z);
        float st = SmoothTime;
        SmoothTime = 0;
        FixedUpdate();
        SmoothTime = st;
    }
}
