using UnityEngine;
using System.Collections.Generic;

public class Point
{
    public float x;
    public float y;
    public float z;

    public Point(float x, float y, float z = 0.0f)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Point(Vector3 v)
    {
        x = v.x;
        y = v.y;
        z = v.z;
    }

    public Vector3 VersVector3()
    {
        return new Vector3(x, y, z);
    }
}