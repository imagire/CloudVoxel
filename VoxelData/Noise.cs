using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class Noise
{
    static float random(float x)
    {
        float a = Mathf.Cos(x / 2) + Mathf.Sin(x / 5);
        return a;
    }

    static Vector3 hash33(Vector3 pos)
    {
        float n = Mathf.Sin(Vector3.Dot(pos, new Vector3(7, 157, 113)));
        return new Vector3(0.2097152f, 0.262144f, 0.32768f) * n;
    }

    public static float Perlin3D(float x, float y, float z)
    {
        float ab = Mathf.PerlinNoise(x, y);
        float bc = Mathf.PerlinNoise(y, z);
        float ac = Mathf.PerlinNoise(x, z);

        float ba = Mathf.PerlinNoise(y, x);
        float cb = Mathf.PerlinNoise(z, y);
        float ca = Mathf.PerlinNoise(z, x);

        //Vector3 lerp = Vector3.Lerp(new Vector3(x, y, z), new Vector3(x + 1, y + 1, z + 1), 6);

        float abc = ab + bc + ac + ba + cb + ca;
        return abc / 6f;
    }

    public static float ZNoise(float x, float scale)
    {
        float i = Mathf.Floor(x);
        float f = x - (int)x;
        float f2 = f * f * f * (f * (f * 6.0f - 15.0f) + 10.0f);
        float z1 = random(x / scale * 2) * scale;
        float z2 = random(x / scale * 2 + 1) * scale;
        float z = interpolate(z1, z2, f2);
        return z;
    }
    public static float WNoise(float x, float y)
    {
        Vector2 pos = new Vector2(x, y);

        Vector2 rightUp = new Vector2((int)x + 1, (int)y + 1);
        Vector2 rightDown = new Vector2((int)x + 1, (int)y);
        Vector2 leftUp = new Vector2((int)x, (int)y + 1);
        Vector2 leftDown = new Vector2((int)x, (int)y);

        //计算x上的插值
        float v1 = dotGridGradient(leftDown, pos);
        float v2 = dotGridGradient(rightDown, pos);
        float interpolation1 = interpolate(v1, v2, x - (int)x);

        //计算y上的插值
        float v3 = dotGridGradient(leftUp, pos);
        float v4 = dotGridGradient(rightUp, pos);
        float interpolation2 = interpolate(v3, v4, x - (int)x);

        float value = interpolate(interpolation1, interpolation2, y - (int)y);
        return value;
    }

    public static Vector3 Worley(Vector3 pos, float scale)
    {
        //pos = pos * scale;
        float dis = 1000;

        for(int x = -1; x < 2; x++)
        {
            for(int z = -1; z < 2; z++)
            {
                for(int y = -1; y < 2; y++)
                {
                    Vector3 nearCell = pos + new Vector3(x, y, z) * 2;
                    Vector3 worldPos = nearCell + hash33(nearCell) * 10;
                    dis = Mathf.Min(dis, Vector3.Distance(worldPos, pos));
                }
            }
        }
        //if(dis > 1)
            dis = (int)(dis * 5);

        Vector3 normal = pos.normalized * 2;
        normal = new Vector3((int)normal.x,(int)normal.y,(int)normal.z);
        Vector3 Npos = pos + (2 - dis) * normal * scale;

        return Npos;
    }

    static float interpolate(float a0, float a1, float w)
    {
        return Mathf.SmoothStep(a0, a1, w);
    }

    static Vector2 RandomVec2(Vector2 p)
    {
        //float n = Mathf.Sin(666 + p.x * 5678 + p.y * 1234) * 4321;
        float n = Mathf.Sin(p.x + p.y) * 4321;
        return new Vector2(Mathf.Sin(n), Mathf.Cos(n));
    }

    static float dotGridGradient(Vector2 p1, Vector2 p2)
    {
        Vector2 gradient = RandomVec2(p1);
        Vector2 offset = p2 - p1;
        return Vector2.Dot(gradient, offset) / 2 + 0.5f;
    }

    public static float fbmNoise(float x, float y, int layer)
    {
        float value = 0;
        float frequency = 1;
        float amplitude = 0.5f;
        for (int i = 0; i < layer; i++)
        {
            value += WNoise(x * frequency, y * frequency) * amplitude;
            frequency *= 2;
            amplitude *= 0.5f;
        }
        return value;
    }

}
