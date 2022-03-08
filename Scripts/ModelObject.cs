using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class ModelObject : MonoBehaviour
{
    MeshFilter mf;
    Mesh mesh;
    private void OnEnable()
    {
        mf = gameObject.GetComponent<MeshFilter>();

        if(mf && mf.sharedMesh)
        {
            if(MeshStore.railMeshes.Contains(mf.sharedMesh))
            {
                Debug.Log("Duplicate");
                mf.sharedMesh = Instantiate(mf.sharedMesh);
            }
            MeshStore.railMeshes.Add(mf.sharedMesh);
            mesh = mf.sharedMesh;
        }

        //Debug.Log(MeshStore.railMeshes.Count + " meshes in memory!");
    }
    private void OnDisable()
    {
        if(mesh && MeshStore.railMeshes.Contains(mesh))
        {
            MeshStore.railMeshes.Remove(mesh);
        }
        //Debug.Log(MeshStore.railMeshes.Count + " meshes in memory!");
    }
}
