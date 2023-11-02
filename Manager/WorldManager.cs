using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class WorldManager : MonoBehaviour
{
    public Material cloudMat;
    private Container container;
    public float scale;

    public Vector2 offset;

    private List<Vector3> plane;
    private List<int> planeVertexIndex;
    Vector3[] faceVertices = new Vector3[4];

    List<Vector3> v = new List<Vector3>();

    //public VoxelColor[] worldColors;
    // Start is called before the first frame update
    void Start()
    {
        if(_instance != null)
        {
            if (_instance != this)
                Destroy(this);
        }
        else
        {
            _instance = this;
        }

        GameObject cont = new GameObject("Container");
        cont.transform.parent = transform;
        container = cont.AddComponent<Container>();
        container.Initialize(cloudMat, Vector3.zero);

        plane = new List<Vector3>();
        planeVertexIndex = new List<int>();
        v = new List<Vector3>();
        
        container.m_Camera = Camera.main;

        int count = 0;
        //container.n = scale;

        //int randomXbottom = Random.Range(0, 8);
        //int randomXheight = Random.Range(8, 32);
        /*float Xheight = 32f;
        //float Zy = Noise.ZNoise(Xheight);
        for (int x = (int)-Xheight; x < Xheight; x++)
        {
            //int randomZbottom = Random.Range(0, 8);
            //int randomZheight = Random.Range(8, 32);
            //float 
            //float Zheight = Noise.ZNoise(Mathf.Abs(x + offset.x), scale);
            //float Zbuttom = Noise.ZNoise(x - offset.x, scale);
            float Zheight = Noise.ZNoise(Mathf.Abs(x + offset.x), scale);
            float Zbuttom = Noise.ZNoise(x - offset.x, scale);

            if (Zheight + Zbuttom < 5)
            {
                Zheight = 0;
                Zbuttom = 0;
            }
                
            for (int z = (int)-Zbuttom; z < Zheight; z++)
            {
                //int randomYbottom = Random.Range(0, 8);
                //int randomYheight = Random.Range(0, 32);
                float Ynoise = Mathf.PerlinNoise(x / scale + offset.x, z / scale + offset.y);
                float Ybuttom = Ynoise * 8;
                float Yheight = Ynoise * 16;
                for (int y = (int)-Ybuttom; y < Yheight; y++)
                {
                    container[new Vector3(x, y, z)] = new Voxel() { ID = 1 };
                }
            }
        }*/

        for(int x = 0; x < 32; x++)
        {
            for(int z = 0; z < 32; z++)
            {
                for(int y = 0; y < 32; y++)
                {
                    //Vector3 pos = Noise.Worley(new Vector3(x, y, z), scale);
                    //container[pos] = new Voxel() { ID = 1 };
                    if(Noise.Perlin3D(x * scale, y * scale, z * scale) >= 0.5)
                    {
                        container[new Vector3(x, y, z)] = new Voxel() { ID = 1 };
                        plane.Add(new Vector3(x, y, z));
                        planeVertexIndex.Add(count);
                        count++;
                    }
                }
            }
        }

        container.GenerateMesh();
        //container.UploadMesh();
        for(int i = 0; i < container.transform.childCount; i++)
        {
            container.initializeChild(cloudMat, Vector3.zero, container.transform.GetChild(i).gameObject);
            container.UploadChildMesh(container.transform.GetChild(i).gameObject);
        }
    }

    private static WorldManager _instance;

    public static WorldManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<WorldManager>();

            return _instance;
        }
    }

    private void Update()
    {
        for(int i = 0; i < plane.Count; i++)
        {
            for (int j = 0; j < planeVertices.Length; j++)
            {
                faceVertices[j] = planeVertices[j] + plane[i];
                Vector3[] d = new Vector3[planeVertices.Length];
                d[j] = faceVertices[j] - plane[i];
                d[j] = Vector3.Cross(Vector3.Cross(d[j], Camera.main.transform.forward), Camera.main.transform.forward);
                faceVertices[j] = plane[i] + d[j] * Mathf.Sqrt(3);

                v.Add(faceVertices[j]);
            }
            //Debug.Log(plane.Count + "," +v.Count + "," + container.meshData.vertices.Count);
        }

        /*for (int i = 0; i < container.meshData.vertices.Count; i++)
        {
            container.meshData.vertices[i] = v[i];
        }*/
        
        //container.n = scale;
        for (int i = 0; i < container.transform.childCount; i++)
        {
            //container.initializeChild(cloudMat, Vector3.zero, container.transform.GetChild(i).gameObject);
            container.UploadChildMesh(container.transform.GetChild(i).gameObject);
        }
    }

    static readonly Vector3[] planeVertices = new Vector3[4]
    {
        new Vector3(-2, -2, 0),//0 -1
        new Vector3(2, -2, 0),//1 1
        new Vector3(-2, 2, 0),//2 -1
        new Vector3(2, 2, 0),//3 1
    };
}
