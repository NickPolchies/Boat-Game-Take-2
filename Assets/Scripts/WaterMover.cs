using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterMover : MonoBehaviour
{
    private MeshFilter meshFilter;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
    }

    void Update()
    {
        Vector3[] verts = meshFilter.mesh.vertices;
        for (int i = 0; i < verts.Length; i++)
        {
            verts[i].y = WaveManager.instance.GetWaveHeight(transform.position.x + verts[i].x * transform.localScale.x, transform.position.z + verts[i].z * transform.localScale.z);
        }

        meshFilter.mesh.vertices = verts;
        meshFilter.mesh.RecalculateNormals();
    }
}
