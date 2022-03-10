using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

[CustomEditor(typeof(RailMaker))]
public class RailMakerEditor : Editor
{
    int selectedPoint = 0;

    SerializedProperty shapeType;
    SerializedProperty railRadius;
    SerializedProperty railPhysicsMaterial;
    SerializedProperty maxArcLengthPerSegment;
    SerializedProperty extraColliderLength;
    SerializedProperty railMaterial;

    SerializedProperty hasFrontPost;
    SerializedProperty frontPostRadius;
    SerializedProperty hasBackPost;
    SerializedProperty backPostRadius;
    SerializedProperty uvTiling;
    SerializedProperty enableAutoRegenerate;

    private void OnEnable()
    {
        shapeType = serializedObject.FindProperty("shapeType");
        railRadius = serializedObject.FindProperty("railRadius");
        railMaterial = serializedObject.FindProperty("railMaterial");
        maxArcLengthPerSegment = serializedObject.FindProperty("maxArcLengthPerSegment");
        extraColliderLength = serializedObject.FindProperty("extraColliderLength");
        railPhysicsMaterial = serializedObject.FindProperty("railPhysicsMaterial");

        hasFrontPost = serializedObject.FindProperty("hasFrontPost");
        frontPostRadius = serializedObject.FindProperty("frontPostRadius");
        hasBackPost = serializedObject.FindProperty("hasBackPost");
        backPostRadius = serializedObject.FindProperty("backPostRadius");
        uvTiling = serializedObject.FindProperty("uvTiling");
        enableAutoRegenerate = serializedObject.FindProperty("enableAutoRegenerate");

        Undo.undoRedoPerformed += UndoRegenerateCallback;
    }
    private void OnDisable()
    {
        Undo.undoRedoPerformed -= UndoRegenerateCallback;
    }
    private void OnSceneGUI()
    {
        RailMaker rm = target as RailMaker;

        Vector3 lastPoint = Vector3.zero;

        for (int i = 0; i < rm.points.Count; i++)
        {
            Vector3 worldSpacePoint = rm.transform.TransformPoint(rm.points[i].point);

            if (i != 0)
            {
                Handles.DrawAAPolyLine(3, lastPoint, worldSpacePoint);

                if (rm.points[i].hasRadius && i != rm.points.Count - 1)
                {
                    DrawSmoothedCorner(rm, rm.points[i], rm.points[i - 1], rm.points[i + 1]);
                }
            }
            if (i == selectedPoint)
            {
                Handles.color = Color.green;
                Handles.SphereHandleCap(0, worldSpacePoint, Quaternion.identity, 0.5f, Event.current.type);
                Handles.color = Color.white;
            }

            Vector3 newPoint = Handles.PositionHandle(worldSpacePoint, rm.transform.rotation);
            if (newPoint != worldSpacePoint)
            {
                selectedPoint = i;
                Undo.RecordObject(rm, "Moved Point");
                rm.points[i].point = rm.transform.InverseTransformPoint(newPoint);
                Regenerate();
            }
            lastPoint = worldSpacePoint;
        }
    }

    public override void OnInspectorGUI()
    {
        RailMaker rm = target as RailMaker;


        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            GUILayout.Label("Rail Settings: ", EditorStyles.boldLabel);

            serializedObject.Update();

            EditorGUILayout.PropertyField(shapeType);
            EditorGUILayout.PropertyField(railMaterial);
            EditorGUILayout.PropertyField(maxArcLengthPerSegment);
            EditorGUILayout.PropertyField(railRadius);
            EditorGUILayout.PropertyField(extraColliderLength);
            EditorGUILayout.PropertyField(railPhysicsMaterial);

            EditorGUILayout.Space(10);

            if (serializedObject.ApplyModifiedProperties())
            {
                Undo.RecordObject(rm, "Change Rail");
                switch (rm.shapeType)
                {
                    case RailShapeType.Circle:
                        rm.shape = RailShape.GenerateCircle(12);
                        break;
                    case RailShapeType.Square:
                        rm.shape = RailShape.GenerateSquare();
                        break;
                    case RailShapeType.None:
                        rm.shape = null;
                        break;
                    case RailShapeType.Custom:
                        break;
                    default:
                        break;
                }

                if (rm.shape != null && rm.modelObject)
                {
                    var mr = rm.modelObject.GetComponent<MeshRenderer>();
                    if (mr)
                        mr.sharedMaterial = rm.railMaterial;
                }

                Regenerate();
            }
        }
        GUILayout.Space(20);

        //Points
        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            GUILayout.Label("Points: ", EditorStyles.boldLabel);
            GUILayout.Label("Selected Point: " + selectedPoint);

            if (rm.points[selectedPoint].hasRadius)
                GUILayout.Label("Actual Radius: " + rm.points[selectedPoint].calculatedRadius);


            GUILayout.Space(10);

            Undo.RecordObject(rm, "Change point");
            EditorGUI.BeginChangeCheck();

            rm.points[selectedPoint].point = EditorGUILayout.Vector3Field("Position", rm.points[selectedPoint].point);

            if (selectedPoint == 0 || selectedPoint == rm.points.Count - 1)
                GUI.enabled = false;
            rm.points[selectedPoint].hasRadius = EditorGUILayout.ToggleLeft("Has Radius:", rm.points[selectedPoint].hasRadius);
            if (rm.points[selectedPoint].hasRadius)
                rm.points[selectedPoint].radius = EditorGUILayout.Slider("Radius: ", rm.points[selectedPoint].radius, 0, 20);
            GUI.enabled = true;

            rm.points[selectedPoint].angle = EditorGUILayout.Slider("Angle: ", rm.points[selectedPoint].angle, -90, 90);

            if (EditorGUI.EndChangeCheck())
            {
                Regenerate();
            }

            GUILayout.Space(10);



            using (new GUILayout.HorizontalScope())
            {
                using (new GUILayout.VerticalScope())
                {
                    GUI.enabled = selectedPoint != 0;
                    if (GUILayout.Button("Previous Point"))
                    {
                        selectedPoint = (selectedPoint - 1 + rm.points.Count) % rm.points.Count;
                        SceneView.RepaintAll();
                    }
                    GUI.enabled = true;

                    if (GUILayout.Button("Insert Point"))
                    {
                        Undo.RecordObject(rm, "Insert Point");

                        Vector3 newPos = rm.points[selectedPoint].point + new Vector3(0, 0, 5);
                        if (selectedPoint < rm.points.Count - 1)
                        {
                            newPos = (rm.points[selectedPoint].point + rm.points[selectedPoint + 1].point) / 2;
                        }

                        rm.points.Insert(selectedPoint + 1, new RailPoint(newPos));
                        selectedPoint = selectedPoint + 1;
                        Regenerate();
                    }
                }

                using (new GUILayout.VerticalScope())
                {
                    GUI.enabled = selectedPoint != rm.points.Count - 1;
                    if (GUILayout.Button("Next Point"))
                    {
                        selectedPoint = (selectedPoint + 1) % rm.points.Count;
                        SceneView.RepaintAll();
                    }
                    GUI.enabled = true;

                    GUI.enabled = rm.points.Count > 2;
                    if (GUILayout.Button("Delete Point"))
                    {
                        Undo.RecordObject(rm, "Delete Point");
                        rm.points.RemoveAt(selectedPoint);
                        if (selectedPoint == rm.points.Count)
                            rm.points[selectedPoint - 1].hasRadius = false;
                        else if (selectedPoint == 0)
                            rm.points[0].hasRadius = false;

                        selectedPoint = Mathf.Min(selectedPoint, rm.points.Count - 1);
                        Regenerate();
                    }
                    GUI.enabled = true;
                }
            }
            GUILayout.Space(10);
            if (GUILayout.Button("Add new Point"))
            {
                Undo.RecordObject(rm, "Add Point");
                Vector3 lastPos = rm.points.Count > 0 ? rm.points[rm.points.Count - 1].point : Vector3.zero;
                rm.points.Add(new RailPoint(lastPos + new Vector3(0, 0, 5)));
                selectedPoint = rm.points.Count - 1;
                Regenerate();
            }
        }

        GUILayout.Space(20);

        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            GUILayout.Label("Cosmetics: ", EditorStyles.boldLabel);
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Posts: ", EditorStyles.boldLabel);

                serializedObject.Update();

                EditorGUILayout.PropertyField(hasFrontPost);
                if (rm.hasFrontPost)
                    EditorGUILayout.PropertyField(frontPostRadius);

                EditorGUILayout.PropertyField(hasBackPost);
                if (rm.hasBackPost)
                    EditorGUILayout.PropertyField(backPostRadius);

                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(uvTiling);

                if (serializedObject.ApplyModifiedProperties())
                {
                    Regenerate();
                }
            }
        }

        GUILayout.Space(20);

        serializedObject.Update();

        EditorGUILayout.PropertyField(enableAutoRegenerate);

        if (serializedObject.ApplyModifiedProperties())
        {
            Regenerate();
        }

        if (GUILayout.Button("Regenerate"))
        {
            Regenerate(true);
        }
    }

    [MenuItem("Tools/RailMaker/Create_Rail")]
    public static void CreateRail()
    {
        Camera sceneCam = SceneView.lastActiveSceneView.camera;
        Vector3 spawnPos = sceneCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));

        GameObject go = new GameObject("new_Rail");
        go.transform.position = spawnPos;

        var rm = go.AddComponent<RailMaker>();

        rm.Reset();

        Undo.RegisterCreatedObjectUndo(go, "Create new Rail");

        Selection.activeGameObject = go;

    }

    public void UndoRegenerateCallback()
    {
        Regenerate();
    }

    public void Regenerate(bool forceRegenerate = false)
    {
        RailMaker rm = target as RailMaker;

        if (!rm)
            return;

        if (!forceRegenerate && !rm.enableAutoRegenerate)
            return;

        if (rm.points.Count < 2)
        {
            Debug.LogError("There need to be at least 2 Points to generate a Rail!");
            return;
        }
        GenerateMesh(rm);
        GenerateCollision(rm);
    }
    public void GenerateMesh(RailMaker rm)
    {
        if (rm.shape == null)
        {

            if (rm.modelObject)
            {
                Undo.RecordObject(rm, "Remove ModelObject");
                Undo.DestroyObjectImmediate(rm.modelObject);
            }
            return;
        }
        if (!rm.modelObject)
        {
            rm.modelObject = new GameObject("Model");
            rm.modelObject.transform.parent = rm.transform;
            rm.modelObject.transform.localPosition = Vector3.zero;
            rm.modelObject.transform.localRotation = Quaternion.identity;
            rm.modelObject.transform.localScale = Vector3.one;
            var meshFilter = rm.modelObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = new Mesh();

            rm.modelObject.AddComponent<ModelObject>();
            var mr = rm.modelObject.AddComponent<MeshRenderer>();
            mr.sharedMaterial = rm.railMaterial;
            Undo.RegisterCreatedObjectUndo(rm.modelObject, "CreateModelObject");
        }
        MeshFilter mf = rm.modelObject.GetComponent<MeshFilter>();
        if (!mf)
        {
            Debug.LogError("No Meshfilter attached!");
            return;
        }
        if (mf.sharedMesh == null)
        {
            mf.sharedMesh = new Mesh();
        }
        Mesh mesh = mf.sharedMesh;
        mesh.Clear();

        var points = GetGenPoints(rm, true);

        List<Vector3> vertices = new List<Vector3>(rm.shape.vertices.Count * points.Count);
        List<Vector2> uvs = new List<Vector2>();

        List<Vector3> capVerts = new List<Vector3>();
        List<Vector3> capNorms = new List<Vector3>();
        List<Vector2> capUvs = new List<Vector2>();

        List<Vector3> normals = new List<Vector3>();

        int newPointCount = 0;

        float currDistance = 0;

        for (int i = 0; i < points.Count; i++)
        {
            Quaternion localRailDirection = Quaternion.identity;
            Vector2 scewVector = Vector2.zero;

            bool addCap = true;

            if (i == 0)
                localRailDirection = Quaternion.LookRotation(points[i + 1].pos - points[i].pos);
            else if (i == points.Count - 1)
            {
                localRailDirection = Quaternion.LookRotation(points[i].pos - points[i - 1].pos);
            }
            else
            {
                addCap = false;
                Vector3 entry = (points[i].pos - points[i - 1].pos).normalized;
                Vector3 exit = (points[i + 1].pos - points[i].pos).normalized;
                localRailDirection = Quaternion.LookRotation(points[i].pos - points[i - 1].pos);

                Quaternion targetRailDirection = Quaternion.LookRotation(entry.normalized + exit.normalized);

                float angle = (180 - Vector3.Angle(entry, exit)) / 2;
                float skewedLength = 1 / Mathf.Sin(angle * Mathf.Deg2Rad);
                float skewedDiff = Mathf.Tan(Mathf.PI / 2 - angle * Mathf.Deg2Rad);
                scewVector = (Quaternion.Inverse(targetRailDirection) * (entry - exit)).normalized * (skewedDiff);

                //Debug.Log("direction: " + scewVector);
                //Debug.Log("skewedLength: " + skewedLength);
                //Debug.Log("skewedDiff: " + skewedDiff);
                //Debug.Log("Angle: " + angle);
            }

            Vector3 currCenterPos = points[i].pos;

            float cosAngle = Mathf.Cos(-points[i].angle * Mathf.Deg2Rad);
            float sinAngle = Mathf.Sin(-points[i].angle * Mathf.Deg2Rad);

            bool isEdge = points[i].isEdge && i > 0 && i < points.Count - 1;

            for (int k = 0; k < (isEdge ? 2 : 1); k++)
            {
                for (int j = 0; j < rm.shape.vertices.Count; j++)
                {
                    Vector2 pos = rm.shape.vertices[j];
                    Vector2 normal = rm.shape.normals[j];

                    if (!Mathf.Approximately(points[i].angle, 0))
                    {
                        pos = new Vector2(pos.x * cosAngle - pos.y * sinAngle, pos.x * sinAngle + pos.y * cosAngle);
                        normal = new Vector2(normal.x * cosAngle - normal.y * sinAngle, normal.x * sinAngle + normal.y * cosAngle);
                    }

                    Quaternion normalRot = localRailDirection;
                    if (k == 1)
                        normalRot = Quaternion.LookRotation(points[i + 1].pos - points[i].pos);

                    Vector3 newVert = currCenterPos + localRailDirection * (new Vector3(pos.x, pos.y, Vector2.Dot(pos, scewVector)) * rm.railRadius);
                    Vector3 newNormal = normalRot * normal;

                    vertices.Add(newVert);
                    normals.Add(newNormal.normalized);
                    uvs.Add(new Vector2(rm.shape.us[j], currDistance / rm.uvTiling));

                    if (addCap)
                    {
                        bool inFront = i == 0;
                        capVerts.Add(newVert);
                        capNorms.Add(localRailDirection * (inFront ? Vector3.back : Vector3.forward));
                        capUvs.Add(new Vector2(rm.shape.us[j], currDistance / rm.uvTiling));
                    }
                }
                newPointCount++;
            }

            if (i != points.Count - 1)
                currDistance += Vector3.Distance(points[i].pos, points[i + 1].pos);
        }

        int[] tris = new int[(newPointCount - 1) * rm.shape.lines.Count * 3 + rm.shape.capTris.Count * 2];

        for (int seg = 0; seg < newPointCount - 1; seg++)
        {
            for (int i = 0; i < rm.shape.lines.Count / 2; i++)
            {
                int VertSegmentCount = rm.shape.vertices.Count;

                int currTriIdx = seg * rm.shape.lines.Count * 3 + i * 6;

                int vert1 = rm.shape.lines[i * 2] + seg * VertSegmentCount;
                int vert2 = rm.shape.lines[(i * 2 + 1) % rm.shape.lines.Count] + seg * VertSegmentCount;
                int vert3 = rm.shape.lines[i * 2] + (1 + seg) * VertSegmentCount;
                int vert4 = rm.shape.lines[(i * 2 + 1) % rm.shape.lines.Count] + (1 + seg) * VertSegmentCount;

                //Debug.Log("vert1: " + vertices[vert1]);
                //Debug.Log("vert2: " + vertices[vert2]);
                //Debug.Log("vert3: " + vertices[vert3]);
                //Debug.Log("vert4: " + vertices[vert4]);

                tris[currTriIdx] = vert2;
                tris[currTriIdx + 1] = vert1;
                tris[currTriIdx + 2] = vert3;

                tris[currTriIdx + 3] = vert4;
                tris[currTriIdx + 4] = vert2;
                tris[currTriIdx + 5] = vert3;
            }
        }

        int vertStartPoint = vertices.Count;
        int triStartPoint = (newPointCount - 1) * rm.shape.lines.Count * 3;



        //Caps
        for (int j = 0; j < 2; j++)
        {
            for (int i = 0; i < rm.shape.capTris.Count; i++)
            {
                tris[triStartPoint + j * rm.shape.capTris.Count + i] = vertStartPoint + j * rm.shape.vertices.Count + rm.shape.capTris[j == 0 ? i : rm.shape.capTris.Count - i - 1];
            }
        }


        vertices.AddRange(capVerts);
        normals.AddRange(capNorms);
        uvs.AddRange(capUvs);

        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.triangles = tris;
        mesh.SetUVs(0, uvs);

        //mesh.RecalculateNormals();

        mf.name = "RailMesh";

        mf.sharedMesh = mesh;
    }
    public void GenerateCollision(RailMaker rm)
    {
        var points = GetGenPoints(rm, false);

        if (rm.collisionObjects.Any(x => !x))
        {
            Undo.RecordObject(rm, "Clean collisionObjects List");
            rm.collisionObjects = rm.collisionObjects.Where(x => x).ToList();
        }

        if (rm.collisionObjects.Count < points.Count - 1)
        {
            int requiredNewObjectsCount = points.Count - 1 - rm.collisionObjects.Count;
            for (int i = 0; i < requiredNewObjectsCount; i++)
            {
                GameObject newCollObj = new GameObject("Collision_" + rm.collisionObjects.Count);
                newCollObj.transform.parent = rm.transform;
                newCollObj.transform.localPosition = Vector3.zero;
                newCollObj.AddComponent<CapsuleCollider>();
                Undo.RegisterCreatedObjectUndo(newCollObj, "CreateCollisionObject");
                Undo.RecordObject(rm, "Added CollisionObject");
                rm.collisionObjects.Add(newCollObj);
            }
        }
        if (rm.collisionObjects.Count > points.Count - 1)
        {
            int objectsToDeleteCount = rm.collisionObjects.Count - (points.Count - 1);

            for (int i = 0; i < objectsToDeleteCount; i++)
            {
                GameObject toRemove = rm.collisionObjects[rm.collisionObjects.Count - 1];
                Undo.DestroyObjectImmediate(toRemove);
                Undo.RecordObject(rm, "Removed CollisionObject");
                rm.collisionObjects.Remove(toRemove);
            }
        }

        for (int i = 0; i < rm.collisionObjects.Count; i++)
        {
            var currObj = rm.collisionObjects[i];

            CapsuleCollider cc = currObj.GetComponent<CapsuleCollider>();
            if (!cc)
            {
                Debug.LogError("No CapsuleCollider attached to " + currObj.name);
                continue;
            }

            Vector3 startPoint = points[i].pos;
            Vector3 endPoint = points[i + 1].pos;

            cc.sharedMaterial = rm.railPhysicsMaterial;

            Quaternion rot = Quaternion.LookRotation(endPoint - startPoint);

            float angle = (points[i].angle + points[i + 1].angle) / 2 * Mathf.Deg2Rad;
            if (!Mathf.Approximately(angle, 0))
            {
                Vector3 up = rot * new Vector3(Mathf.Sin(angle), Mathf.Cos(angle));
                rot = Quaternion.LookRotation(endPoint - startPoint, up);
            }

            currObj.transform.localRotation = rot;
            currObj.transform.localPosition = (startPoint + endPoint) / 2;
            cc.radius = rm.railRadius;
            cc.height = (startPoint - endPoint).magnitude + rm.railRadius * 2 + rm.extraColliderLength;
            cc.direction = 2;
        }
    }

    List<RailGenPoint> GetGenPoints(RailMaker rm, bool isModel)
    {
        List<RailGenPoint> points = new List<RailGenPoint>();

        Undo.RecordObject(rm, "Recalculate calculated Values");

        for (int i = 0; i < rm.points.Count; i++)
        {

            if (rm.points[i].hasRadius && i > 0 && i < rm.points.Count - 1)
            {
                float radius, distance;
                points.AddRange(GetSmoothedCorner(rm, rm.points[i], rm.points[i - 1], rm.points[i + 1], out radius, out distance));
                rm.points[i].calculatedRadius = radius;
                rm.points[i].calculatedDistance = distance;
            }
            else
            {
                if (rm.hasFrontPost && i == 0 && isModel)
                {
                    points.Add(new RailGenPoint(rm.points[i].point + new Vector3(0, -10, 0) + (rm.points[i].point - rm.points[i].point).normalized * 0.1f, rm.points[i].angle, true));
                }

                points.Add(new RailGenPoint(rm.points[i].point, rm.points[i].angle, true));
                rm.points[i].calculatedRadius = 0;
                rm.points[i].calculatedDistance = 0;

                if (rm.hasBackPost && i == rm.points.Count - 1 && isModel)
                {
                    points.Add(new RailGenPoint(rm.points[i].point + new Vector3(0, -20, 0) + (rm.points[i].point - rm.points[i - 1].point).normalized * 0.1f, rm.points[i].angle, true));
                }
            }
        }
        return points;
    }

    float GetActualRadius(RailMaker rm, RailPoint point, RailPoint previous, RailPoint next, out Vector3 entryDir, out Vector3 exitDir, out float angle, out float distance, out bool reachesPrevPoint, out bool reachesNextPoint)
    {
        entryDir = point.point - previous.point;
        exitDir = next.point - point.point;

        angle = Vector3.Angle(-entryDir, exitDir) * Mathf.Deg2Rad;

        float previousDistance = Vector3.Distance(point.point, previous.point) - rm.railRadius;
        if (previous.hasRadius)
        {
            previousDistance = previousDistance - previous.calculatedDistance;
        }

        float nextDistance = Vector3.Distance(point.point, next.point) - rm.railRadius;
        if (next.hasRadius)
            nextDistance = nextDistance * point.radius / (point.radius + next.radius);

        float distanceCandidate = point.radius / Mathf.Tan(angle / 2);

        distance = Mathf.Min(distanceCandidate, previousDistance, nextDistance);

        reachesPrevPoint = Mathf.Approximately(distance, previousDistance) && previous.hasRadius;
        reachesNextPoint = Mathf.Approximately(distance, Vector3.Distance(point.point, next.point)) && next.hasRadius;

        return distance * Mathf.Tan(angle / 2);
    }

    List<RailGenPoint> GetSmoothedCorner(RailMaker rm, RailPoint point, RailPoint previous, RailPoint next, out float radius, out float distance)
    {
        List<RailGenPoint> points = new List<RailGenPoint>();

        Vector3 entryDir, exitDir;
        float angle;
        bool reachesPrevPoint, reachesNextPoint;

        radius = GetActualRadius(rm, point, previous, next, out entryDir, out exitDir, out angle, out distance, out reachesPrevPoint, out reachesNextPoint);

        Vector3 cross = Vector3.Cross(entryDir, exitDir);
        Vector3 centerDir = exitDir.normalized - entryDir.normalized;
        float centerLength = distance / Mathf.Cos(angle / 2);
        Vector3 centerPos = point.point + centerDir.normalized * centerLength;

        Quaternion rot = Quaternion.LookRotation(cross, -centerDir);

        float betweenAngle = Vector3.Angle(entryDir, exitDir) * Mathf.Deg2Rad;

        float arcLength = ((2 * radius) * Mathf.PI) * (betweenAngle / (2 * Mathf.PI));

        int segmentCount = Mathf.CeilToInt(arcLength / rm.maxArcLengthPerSegment) + 2;

        float prevDistance = Vector3.Distance(point.point, previous.point);
        float nextDistance = Vector3.Distance(point.point, next.point);

        float segmentDistance = (distance * 2 / segmentCount); //Not arc Length!

        int startIdx = reachesPrevPoint ? 1 : 0;
        int endIdx = reachesNextPoint ? segmentCount - 1 : segmentCount;

        for (int i = startIdx; i < endIdx; i++)
        {
            float currAngle = Mathf.Lerp(betweenAngle, -betweenAngle, i / (float)(segmentCount - 1));
            Vector3 segVec = new Vector3(Mathf.Sin(currAngle / 2), Mathf.Cos(currAngle / 2)) * radius;

            float segTotalDistance = (prevDistance - distance) + i * segmentDistance;
            float segAngle;
            if (segTotalDistance < prevDistance)
            {
                segAngle = Mathf.Lerp(previous.angle, point.angle, segTotalDistance / prevDistance);
            }
            else
            {
                segAngle = Mathf.Lerp(point.angle, next.angle, (segTotalDistance - prevDistance) / nextDistance);
            }

            points.Add(new RailGenPoint(rot * segVec + centerPos, segAngle, false));
        }

        return points;
    }

    void DrawSmoothedCorner(RailMaker rm, RailPoint point, RailPoint previous, RailPoint next)
    {
        Vector3 entryDir, exitDir;
        float angle, distance;
        bool _;

        float radius = GetActualRadius(rm, point, previous, next, out entryDir, out exitDir, out angle, out distance, out _, out _);
        if (Mathf.Abs(angle - Mathf.PI) < 0.1f)
            return;

        Vector3 cross = Vector3.Cross(entryDir, exitDir);
        Vector3 centerDir = exitDir.normalized - entryDir.normalized;
        float centerLength = distance / Mathf.Cos(angle / 2);
        Vector3 centerPos = point.point + centerDir.normalized * centerLength;

        Vector3 firstTangentDir = (point.point - entryDir.normalized * distance) - centerPos;

        Handles.color = Color.green;
        Handles.DrawAAPolyLine(6, rm.transform.TransformPoint(-entryDir.normalized * distance + point.point), rm.transform.TransformPoint(point.point), rm.transform.TransformPoint(exitDir.normalized * distance + point.point));

        //Handles.DrawWireDisc(rm.transform.TransformPoint(centerPos), rm.transform.TransformDirection(cross), actualRadius);
        Handles.DrawWireArc(rm.transform.TransformPoint(centerPos), rm.transform.TransformDirection(cross), rm.transform.TransformDirection(firstTangentDir), Vector3.Angle(entryDir, exitDir), radius, 6f);
        Handles.color = Color.white;
    }
}
