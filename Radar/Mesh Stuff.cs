using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshStuff : MonoBehaviour
{
    
    GameObject plane;
    Mesh mesh;
    int[] triangles;
    Vector3[] vertices, normals, temp;
    Tri[] Trilist;

    // Start is called before the first frame update
    void Start()
    {
        plane = GameObject.Find("plane");
        mesh = plane.GetComponent<MeshFilter>().mesh;
        triangles = mesh.triangles;
        vertices = mesh.vertices;
        normals = mesh.normals;
        for (int i = 0; i < normals.Length; i += 1){
            for (int j = 0; j < 2; j += 1){
                temp[j] = vertices[triangles[i*3+j-2]];
            }
            Trilist[i].Trivertices = temp;
            Trilist[i].normal = normals[i];
        }
        Debug.Log(Trilist);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public class Tri : MonoBehaviour{
        public Vector3[] Trivertices;
        public Vector3 normal;
    }
}