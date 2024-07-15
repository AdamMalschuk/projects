using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class mapgen : MonoBehaviour
{
    public enum Drawmode {Noisemap,colourmap,mesh,fallofmap};
    public Noise.Normalizemode normalizemode;
    public Drawmode drawmode;
    public const int chunksize = 239;
    [Range(0, 6)]
    public int levelofdetail;
    public int octaves, seed;
    public float noisescale, lacunarity, heightmultipl;
    public AnimationCurve meshheightcurve;
    [Range(0, 1)]
    public float persistance;
    float[,] fallofmap;
    public bool autoUpdate, usefallofmap;
    public Teraintypes[] region;
    public Vector2 offset;
    Queue<Mapthreadinfo<Mapdata>> mapdatathreadinfoQ = new Queue<Mapthreadinfo<Mapdata>>();
    Queue<Mapthreadinfo<MeshData>> MDTIQ = new Queue<Mapthreadinfo<MeshData>>();
    void Awake()
    {
        fallofmap = fallofgen.genfallofmap(chunksize);//for single chunk worlds

    }
    public void Drawmapineditor()//seperate drawmodes for development
    {
        Mapdata mapdata = GenerateMap(Vector2.zero);
        Mapdisplay display = FindObjectOfType<Mapdisplay>();
        if (drawmode == Drawmode.Noisemap)//for drawing a black to white gradient of the noise map's heightmap
        {
            display.Drawtexture(texturegenerator.texfrmhmp(mapdata.heightmap));
        }
        else if (drawmode == Drawmode.colourmap)//for drawing the colours to see what world would look like
        {
            display.Drawtexture(texturegenerator.texfrmcolourmp(mapdata.colourmap, chunksize, chunksize));

        }
        else if (drawmode == Drawmode.mesh)//for drawing the mesh to see what the map would look like
        {
            display.DrawMesh(MeshGen.genterrainmesh(mapdata.heightmap, heightmultipl, meshheightcurve, levelofdetail), (texturegenerator.texfrmcolourmp(mapdata.colourmap, chunksize, chunksize)));
        }
        else if(drawmode == Drawmode.fallofmap)//to see what map would be like for single chunk
        {
            display.Drawtexture(texturegenerator.texfrmhmp(fallofgen.genfallofmap(chunksize)));
        }
    }
    public void RQMD(Vector2 centre, Action<Mapdata> callback)//request mapdata and starts a new thread to do so
    {
        ThreadStart threadstart = delegate
        {
            Mapdatathread(centre, callback);
        };
        new Thread(threadstart).Start();
    }
    void Mapdatathread(Vector2 centre, Action<Mapdata> callback)//the new thread that was started
    {
        Mapdata mapdata = GenerateMap(centre);//generates mapdata
        lock (mapdatathreadinfoQ)
        {
            mapdatathreadinfoQ.Enqueue(new Mapthreadinfo<Mapdata>(callback, mapdata));
        }
    }
    public void RQMeshD(Mapdata mapdata, int lod, Action<MeshData> callback)//request meshdata and starts a new thread
    {
        ThreadStart threadstart = delegate
        {
            MeshdataThread(mapdata, lod, callback);
        };
        new Thread(threadstart).Start();
    }
    void MeshdataThread(Mapdata mapdata, int lod, Action<MeshData> callback)
    {
        MeshData meshdata = MeshGen.genterrainmesh(mapdata.heightmap, heightmultipl, meshheightcurve, lod);//generates meshdata
        lock (MDTIQ)
        {
            MDTIQ.Enqueue(new Mapthreadinfo<MeshData>(callback, meshdata));
        }
    }
    void Update()//every frame dequeues the threadinfo 
    {
        if (mapdatathreadinfoQ.Count > 0)
        {
            for(int i = 0; i < mapdatathreadinfoQ.Count; i++)
            {
                Mapthreadinfo<Mapdata> threadinfo = mapdatathreadinfoQ.Dequeue();
                threadinfo.callback(threadinfo.parameter);
            }
        }
        if (MDTIQ.Count > 0)
        {
            for (int i = 0; i < MDTIQ.Count; i++)
            {
                Mapthreadinfo<MeshData> threadinfo = MDTIQ.Dequeue();
                threadinfo.callback(threadinfo.parameter);
            }
        }
    }
    Mapdata GenerateMap(Vector2 centre)//generates the map
    {
        float[,] noisemap = Noise.GNM(chunksize+2, chunksize+2, noisescale, octaves, seed, persistance, lacunarity, centre + offset, normalizemode);
        Color[] coloumap = new Color[chunksize * chunksize];
        for (int y = 0; y < chunksize; y++)
        {
            for (int x = 0; x < chunksize; x++)
            {
                if (usefallofmap)
                {
                    noisemap[x, y] = Mathf.Clamp01(noisemap[x, y] - fallofmap[x, y]);//creates falloff effect if wanted
                }
                float cheigt = noisemap[x, y];
                for (int i = 0; i < region.Length; i++)//applies colours to the map
                {
                    if(cheigt >= region[i].height)
                    {
                        coloumap[y * chunksize + x] = region[i].colour;

                    }
                    else {
                        break;
                    }
                }
            }
        }
        return (new Mapdata(noisemap, coloumap));
    }
    private void OnValidate()//validations for noise map generation
    {
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 0)
        {
            octaves = 0;
        }
        fallofmap = fallofgen.genfallofmap(chunksize);
    }
    struct Mapthreadinfo<T>//structs to initialise variables
    {
        public readonly Action<T> callback;
        public readonly T parameter;
        public Mapthreadinfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}
[System.Serializable]
public struct Teraintypes
{
    public float height;
    public Color colour;
    public string tname;
}
public struct Mapdata
{
    public readonly float[,] heightmap;
    public readonly Color[] colourmap;
    public Mapdata(float[,] heightmap, Color[] colourmap)
    {
        this.heightmap = heightmap;
        this.colourmap = colourmap;
    }
}