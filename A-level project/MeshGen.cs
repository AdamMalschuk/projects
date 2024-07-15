using System.Collections;
using UnityEngine;

public static class MeshGen
{
    public static MeshData genterrainmesh(float[,] heightmap,float heightxxx, AnimationCurve _heightcurve,int lod)//generates mesh and meshdata
    {
        int meshres = (lod == 0) ? 1 : lod * 2;
        AnimationCurve heightcurve = new AnimationCurve (_heightcurve.keys);
        int borderedsize = heightmap.GetLength(0);
        int meshsize = borderedsize - 2*meshres;
        int maxresmeshsize = borderedsize - 2;
        
        float topleftx = (maxresmeshsize - 1) / -2f;
        float topleftz = (maxresmeshsize - 1) / 2f;
        
        int verteciesperline = (meshsize - 1) / meshres + 1;
        MeshData meshdata = new MeshData(verteciesperline);
        int[,] vertexindiceismap = new int[borderedsize, borderedsize];
        int meshvertexindex = 0;
        int borderindex = -1;
        for (int y = 0; y < borderedsize; y += meshres)//creates a verticies map(map of all the points in the mesh)
        {
            for (int x = 0; x < borderedsize; x += meshres)
            {
                bool isbordervertex = y == 0 || y == borderedsize - 1 || x == 0 || x == borderedsize - 1;
                if (isbordervertex)
                {
                    vertexindiceismap[x, y] = borderindex;
                    borderindex--;
                }//generates the next layer of verticies to make smooth connections between chunks
                else
                {
                    vertexindiceismap[x, y] = meshvertexindex;
                    meshvertexindex++;
                }//creates an index of the mesh verticies that are not on the border
            }
        }
        for (int y = 0; y < borderedsize; y+=meshres)//adds the heights to the verticies data and creates triangles for rendering
        {
            for (int x = 0; x < borderedsize; x+=meshres)
            {
                int vertexindex = vertexindiceismap[x, y];
                Vector2 percent = new Vector2((x - meshres) / (float)meshsize, (y - meshres) / (float)meshsize);
                float height = heightcurve.Evaluate(heightmap[x, y]) * heightxxx;
                Vector3 vertexposition = new Vector3(topleftx + percent.x * maxresmeshsize, height, topleftz - percent.y * maxresmeshsize);
                meshdata.addvertex(vertexposition, percent, vertexindex);                
                if(x < borderedsize-1 && y < borderedsize - 1)
                {
                    int a = vertexindiceismap[x, y];
                    int b = vertexindiceismap[x + meshres, y];
                    int c = vertexindiceismap[x, y + meshres];
                    int d = vertexindiceismap[x + meshres, y + meshres];
                    meshdata.addtriangle(a,d,c);
                    meshdata.addtriangle(d,a,b);

                }
                vertexindex++;
            }
        }
        return (meshdata);
    }
}
public class MeshData//stores the mesh data
{
    Vector3[] verticies, borderverticies;
    int[] triangles, bordertriangles;
    int triangleindex, bordertriangleindex;
    Vector2[] uvs;
    public MeshData(int verteciesperline)
    {
        verticies = new Vector3[verteciesperline * verteciesperline];
        uvs = new Vector2[verteciesperline * verteciesperline];
        triangles = new int[(verteciesperline - 1) * (verteciesperline - 1) * 6];
        borderverticies = new Vector3[verteciesperline * 4 + 4];
        bordertriangles = new int[24 * verteciesperline];
    }
    public void addvertex(Vector3 vpos, Vector2 uv, int vertexindex)
    {
        if (vertexindex < 0)
        {
            borderverticies[-vertexindex - 1] = vpos;
        }
        else
        {
            verticies[vertexindex] = vpos;
            uvs[vertexindex] = uv;
        }
    }
    public void addtriangle(int a, int b, int c)//creates and manages triangles for main mesh and border trinagles
    {
        if (a < 0 || b < 0 || c < 0)
        {
            bordertriangles[bordertriangleindex] = a;
            bordertriangles[bordertriangleindex + 1] = b;
            bordertriangles[bordertriangleindex + 2] = c;
            bordertriangleindex += 3;
        }
        else
        {
            triangles[triangleindex] = a;
            triangles[triangleindex + 1] = b;
            triangles[triangleindex + 2] = c;
            triangleindex += 3;
        }
    }
    Vector3[] calculatenormals()//applies normals to the triangles(line at 90 degrees to the plane of the triangle) which is used for rendering)
    {
        Vector3[] vertexnormals = new Vector3[verticies.Length];
        int trianglecount = triangles.Length/3;
        for(int i = 0; i < trianglecount; i++)
        {
            int normaltriangleindex = i * 3;
            int vetexindexa = triangles[normaltriangleindex];
            int vetexindexb = triangles[normaltriangleindex+1];
            int vetexindexc = triangles[normaltriangleindex+2];
            Vector3 trianglenormal = surfacenormalfromindicies(vetexindexa, vetexindexb, vetexindexc);
            vertexnormals[vetexindexa] += trianglenormal;
            vertexnormals[vetexindexb] += trianglenormal;
            vertexnormals[vetexindexc] += trianglenormal;
        }
        int bordertrianglecount = bordertriangles.Length / 3;
        for (int i = 0; i < bordertrianglecount; i++)
        {
            int normaltriangleindex = i * 3;
            int vetexindexa = bordertriangles[normaltriangleindex];
            int vetexindexb = bordertriangles[normaltriangleindex + 1];
            int vetexindexc = bordertriangles[normaltriangleindex + 2];
            Vector3 trianglenormal = surfacenormalfromindicies(vetexindexa, vetexindexb, vetexindexc);
            if (vetexindexa >= 0)
            {
                vertexnormals[vetexindexa] += trianglenormal;
            }
            if (vetexindexb >= 0)
            {
                vertexnormals[vetexindexb] += trianglenormal;
            }
            if (vetexindexc >= 0)
            {
                vertexnormals[vetexindexc] += trianglenormal;
            }
        }

        for (int i = 0; i < vertexnormals.Length; i++)
        {
            vertexnormals[i].Normalize();
        }
        return vertexnormals;
    }
    Vector3 surfacenormalfromindicies(int indexa, int indexb, int indexc)//calculates normals
    {
        Vector3 pointa = (indexa<0)?borderverticies[-indexa-1]:verticies[indexa];
        Vector3 pointb = (indexb < 0) ? borderverticies[-indexb - 1] : verticies[indexb];
        Vector3 pointc = (indexc < 0) ? borderverticies[-indexc - 1] : verticies[indexc];
        Vector3 sideab = pointb - pointa;
        Vector3 sideac = pointc - pointa;
        return Vector3.Cross(sideab, sideac).normalized;

    }
    public Mesh createmesh()//applies all calculated values to a mesh object
    {
        Mesh mesh = new Mesh();
        mesh.vertices = verticies;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.normals = calculatenormals();
        return (mesh);
    }
}
