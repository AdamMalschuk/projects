using System.Collections;
using UnityEngine;

public static class Noise
{
    public enum Normalizemode {local,global}
    public static float[,] GNM(int MW,int MH,float scale,int octaves,int seed, float persistance, float lacunarity,Vector2 offset, Normalizemode normalizemode)
    {//takes all of the inputs for the following calculations to create the noisemap
        System.Random prng = new System.Random(seed);//random nuber based off of seed
        float[,] noismap = new float[MW, MH];//creates new 2D array of floats
        Vector2[] octaveoffsets = new Vector2[octaves];
        float maxposheight = 0;
        float amplitude = 1, frequency = 1;
        for (int i = 0; i < octaves; i++)//creates random offsets for functions which will be overlapped with perlin noise to make a more realistic landscae
        {
            float offsetx = prng.Next(-100000, 100000) + offset.x;
            float offsety = prng.Next(-100000, 100000) - offset.y;
            octaveoffsets[i] = new Vector2(offsetx, offsety);
            maxposheight += amplitude;
            amplitude *= persistance;
        }
        if (scale <= 0)//sets minimum scale otherwise there would benumbers that would break the functions 
        {
            scale = 0.0001f;
        }
        float maxlocalnoiseheight = float.MinValue;//sets max and min vlaues to avoid infinite spikes of doom
        float minlocalnoiseheight = float.MaxValue;
        float hwidth = MW / 2, hheight = MH / 2;
        for (int y = 0; y < MH; y++)//calculates and applies all of the heights for the mesh into a 2D array of points
        {
            for (int x = 0; x < MW; x++)
            {
                amplitude = 1;
                frequency = 1;
                float noiseheight = 0;
                for (int i = 0; i < octaves; i++)
                {
                    float samplex = (x- hwidth + octaveoffsets[i].x) / scale * frequency;
                    float sampley = (y- hheight + octaveoffsets[i].y) / scale * frequency;
                    float perlinsample = Mathf.PerlinNoise(samplex, sampley) * 2 - 1;
                    noiseheight += perlinsample * amplitude;
                    amplitude *= persistance;
                    frequency *= lacunarity;
                }
                if (noiseheight > maxlocalnoiseheight)
                {
                    maxlocalnoiseheight = noiseheight;
                }
                else if (noiseheight < minlocalnoiseheight)
                {
                    minlocalnoiseheight = noiseheight;
                }
                noismap[x, y] = noiseheight;
            }
        }
        for (int y = 0; y < MH; y++)
        {
            for (int x = 0; x < MW; x++)
            {
                if (normalizemode == Normalizemode.local)//normalise modes so you can set it to be optimised for one chunk or many, i mostly use global but possibly later
                {                                        //it can be added as an option for the player
                    noismap[x, y] = Mathf.InverseLerp(minlocalnoiseheight, maxlocalnoiseheight, noismap[x, y]);
                }else if(normalizemode == Normalizemode.global)
                {
                    float normalizedheight = (noismap[x, y]+1)/(2f*maxposheight/3f);
                    noismap[x, y] = Mathf.Clamp(normalizedheight,0f,int.MaxValue);
                }
            }
        }
     
        return (noismap);//returns the final noisemap
        
    }
}
