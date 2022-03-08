using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RailPoint
{
    public Vector3 point;
    [Range(0, 25)]
    public float radius;
    public float calculatedRadius;
    public float calculatedDistance;
    public bool hasRadius;
    public float angle;
    public RailPoint(Vector3 point)
    {
        this.point = point;
        radius = 0;
        hasRadius = false;
        angle = 0;
    }
    public RailPoint(Vector3 point, float radius)
    {
        this.point = point;
        this.radius = radius;
        hasRadius = true;
        angle = 0;
    }
}
public class RailGenPoint
{
    public Vector3 pos;
    public float angle;
    public bool isEdge;

    public RailGenPoint(Vector3 pos, float angle, bool isEdge)
    {
        this.pos = pos;
        this.angle = angle;
        this.isEdge = isEdge;
    }
}