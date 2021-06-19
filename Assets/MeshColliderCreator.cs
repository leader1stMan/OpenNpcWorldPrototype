using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshColliderCreator : MonoBehaviour
{
    MeshFilter[] Meshes;
    // Start is called before the first frame update
    void Start()
    {
        Meshes = GetComponentsInChildren<MeshFilter>();

        foreach (var mesh in Meshes)
        {
            if (mesh.gameObject.GetComponent<Collider>() == null)
            {
                var collider = mesh.gameObject.AddComponent<MeshCollider>();
                collider.sharedMesh = mesh.mesh;
            }
        }
    }
}
