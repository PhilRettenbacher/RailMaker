using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[SelectionBase]
public class RailMaker : RailDefinition
{
    //RailSegments Data
    //RailSegment: Type (flat, round, spline)

    public List<RailPoint> points = new List<RailPoint>();

    [Range(0.02f, 2f)]
    public float railRadius = .15f;

    [Range(0.2f, 5f)]
    public float maxArcLengthPerSegment = 0.5f;

    public float extraColliderLength = 0f;

    [Min(0.1f), ]
    public float uvTiling = 1f;

    public bool enableAutoRegenerate = false;

    public RailShape shape;

    public Material railMaterial;

    public RailShapeType shapeType;

    public GameObject modelObject;

    public List<GameObject> collisionObjects = new List<GameObject>();

    public PhysicMaterial railPhysicsMaterial;

    //Cosmetics
    public bool hasFrontPost;
    [Range(0, 5)]
    public float frontPostRadius;
    public bool hasBackPost;
    [Range(0, 5)]
    public float backPostRadius;

    public void Reset()
    {
        points.Clear();
        points.Add(new RailPoint(new Vector3(0, 2, 0)));
        points.Add(new RailPoint(new Vector3(0, 2, 5)));

        foreach(Transform trans in transform.Cast<Transform>().ToArray())
        {
            if (Application.isPlaying)
                Destroy(trans.gameObject);
            else
                DestroyImmediate(trans.gameObject);
        }

        Debug.Log(transform.childCount);

        hasFrontPost = true;
        frontPostRadius = 0;
        hasBackPost = true;
        backPostRadius = 0;

        uvTiling = 1;

        shape = RailShape.GenerateCircle(12);

        maxArcLengthPerSegment = 0.5f;

        railRadius = .15f;
    }
}
