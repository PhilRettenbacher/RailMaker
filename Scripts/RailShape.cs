using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RailShape
{
    public List<Vector2> vertices;
    public List<Vector2> normals;
    public List<float> us;
    public List<int> lines;
    public List<int> capTris;

    public  static RailShape GenerateCircle(int subdivisions)
    {
        if (subdivisions <= 2)
        {
            Debug.LogError("A circle needs at least 3 Points");
            return null;
        }   

        List<Vector2> vertices = new List<Vector2>();
        List<Vector2> normals = new List<Vector2>();
        List<int> lines = new List<int>();
        List<int> capTris = new List<int>();
        List<float> us = new List<float>();

        for(int i = 0; i<subdivisions; i++)
        {
            float x = Mathf.Sin((float)i / subdivisions * Mathf.PI * 2);
            float y = Mathf.Cos((float)i / subdivisions * Mathf.PI * 2);
            Vector2 pos = new Vector2(x, y);
            vertices.Add(pos);
            normals.Add(pos);
            us.Add((Mathf.InverseLerp(0, subdivisions - 1, i) + 0.5f) % 1);

            lines.Add(i);
            lines.Add((i + 1) % subdivisions);
        }

        for(int i = 2; i<subdivisions; i++)
        {
            capTris.Add(0);
            capTris.Add(i-1);
            capTris.Add(i);
        }

        RailShape shape = new RailShape();
        shape.vertices = vertices;
        shape.normals = normals;
        shape.lines = lines;
        shape.capTris = capTris;
        shape.us = us;

        return shape;
    }
    public static RailShape GenerateSquare()
    {
        List<Vector2> vertices = new List<Vector2>();
        List<Vector2> normals = new List<Vector2>();
        List<int> lines = new List<int>();
        List<int> capTris = new List<int>();
        List<float> us = new List<float>();

        Vector2 vec1 = new Vector2(-1, 1);
        Vector2 vec2 = new Vector2(1, 1);
        Vector2 vec3 = new Vector2(1, -1);
        Vector2 vec4 = new Vector2(-1, -1);

        vertices.Add(vec1);
        us.Add(0.125f * 3);
        vertices.Add(vec2);
        us.Add(0.125f * 5);

        vertices.Add(vec2);
        us.Add(0.125f * 5);
        vertices.Add(vec3);
        us.Add(0.125f * 7);

        vertices.Add(vec3);
        us.Add(0.125f * 7);
        vertices.Add(vec4);
        us.Add(0.125f * 1);

        vertices.Add(vec4);
        us.Add(0.125f * 1);
        vertices.Add(vec1);
        us.Add(0.125f * 3);

        normals.Add(Vector2.up);
        normals.Add(Vector2.up);
        normals.Add(Vector2.right);
        normals.Add(Vector2.right);
        normals.Add(Vector2.down);
        normals.Add(Vector2.down);
        normals.Add(Vector2.left);
        normals.Add(Vector2.left);

        for (int i = 0; i< 8; i++)
        {
            lines.Add(i);
        }

        capTris.Add(0);
        capTris.Add(1);
        capTris.Add(3);

        capTris.Add(5);
        capTris.Add(0);
        capTris.Add(3);

        RailShape shape = new RailShape();
        shape.vertices = vertices;
        shape.normals = normals;
        shape.lines = lines;
        shape.capTris = capTris;
        shape.us = us;

        return shape;
    }
}

public enum RailShapeType
{
    Circle,
    Square,
    None,
    Custom
}
