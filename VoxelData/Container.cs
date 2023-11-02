using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Container : MonoBehaviour
{
    public Vector3 containerPosition;

    private Dictionary<Vector3, Voxel> data;
    public MeshData meshData = new MeshData();

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;

    public Camera m_Camera;

    public float d0;

    public void Initialize(Material mat, Vector3 position)
    {
        ConfigureComponent();
        data = new Dictionary<Vector3, Voxel>();
        meshRenderer.sharedMaterial = mat;
        containerPosition = position;
    }

    public void initializeChild(Material mat, Vector3 position, GameObject g)
    {
        ConfigureChildComponent(g);
        data = new Dictionary<Vector3, Voxel>();
        meshRenderer.sharedMaterial = mat;
        containerPosition = position;
    }

    public void ClearData()
    {
        data.Clear();
    }

    private void ConfigureComponent()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();
    }

    private void ConfigureChildComponent(GameObject g)
    {
        meshFilter = g.GetComponent<MeshFilter>();
        meshRenderer = g.GetComponent<MeshRenderer>();
        meshCollider = g.GetComponent<MeshCollider>();
    }

    public void GenerateMesh()
    {
        meshData.ClearData();

        Vector3 blockPos;
        Voxel block;

        int counter = 0;
        Vector3[] faceVertices = new Vector3[4];
        Vector2[] faceUVs = new Vector2[4];
        Vector3[] d = new Vector3[4];
        Vector3 cam = m_Camera.transform.forward.normalized;

        //VoxelColor voxelColor;
        //Color voxelColorAlpha;
        //Vector2 voxelSmoothness;

        GameObject g = new GameObject("Chunk");
        g.transform.parent = this.transform;
        meshFilter = g.AddComponent<MeshFilter>();
        meshRenderer = g.AddComponent<MeshRenderer>();
        meshCollider = g.AddComponent<MeshCollider>();

        //Plane
        foreach (KeyValuePair<Vector3, Voxel> kvp in data)
        {
            if (kvp.Value.ID == 0)
                continue;

            blockPos = kvp.Key;
            block = kvp.Value;

            //voxelColor = WorldManager.Instance.worldColors[block.ID - 1];
            //voxelColorAlpha = voxelColor.color;
            //voxelColorAlpha.a = 1;
            // voxelSmoothness = new Vector2(voxelColor.metallic, voxelColor.smoothness);
            

            for (int j = 0; j < 4; j++)
            {
                faceVertices[j] = planeVertices[planeVertexIndex[0, j]] + blockPos;//¶¥µã×ø±ê              
                d[j] = faceVertices[j] - blockPos;
                d[j] = Vector3.Cross(Vector3.Cross(d[j], cam), cam);//.normalized;
                faceVertices[j] = blockPos + d[j] * Mathf.Sqrt(2);

                faceUVs[j] = planeUVs[j];
            }
            for (int j = 0; j < 6; j++)
            {
                //Debug.Log(meshData.vertices.Count);
                meshData.vertices.Add(faceVertices[planeTris[0, j]]);
                meshData.UVs.Add(faceUVs[planeTris[0, j]]);

                meshData.triangles.Add(counter++);
            }
        }

        //Cube
        /*foreach (KeyValuePair<Vector3, Voxel> kvp in data)
        {
            if (kvp.Value.ID == 0)
                continue;

            blockPos = kvp.Key;
            block = kvp.Value;

            //voxelColor = WorldManager.Instance.worldColors[block.ID - 1];
            //voxelColorAlpha = voxelColor.color;
            //voxelColorAlpha.a = 1;
            // voxelSmoothness = new Vector2(voxelColor.metallic, voxelColor.smoothness);

            for (int i = 0; i < 6; i++)
            {
                if (this[blockPos + voxelFaceChecks[i]].isSolid)
                    continue;
                
                //Collect the appropriate vertices from the default vertices and add the block position
                for (int j = 0; j < 4; j++)
                {
                    faceVertices[j] = voxelVertices[voxelVertexIndex[i, j]] + blockPos;
                    faceUVs[j] = voxelUVs[j];
                }
                for (int j = 0; j < 6; j++)
                {
                    if (meshData.vertices.Count < 65536)
                    {
                        Debug.Log(meshData.vertices.Count);
                        meshData.vertices.Add(faceVertices[voxelTris[i, j]]);
                        meshData.UVs.Add(faceUVs[voxelTris[i, j]]);

                        meshData.triangles.Add(counter++);
                    }
                    else
                    {
                        Debug.Log(meshData.vertices.Count);
                        g = new GameObject("Chunk");
                        g.transform.parent = this.transform;
                        meshFilter = g.AddComponent<MeshFilter>();
                        meshRenderer = g.AddComponent<MeshRenderer>();
                        meshCollider = g.AddComponent<MeshCollider>();

                        i = 0;
                        j = 0;

                        //UploadChildMesh(g);
                    }
                }
            }
        }*/

        /*List<CombineInstance> combine = new List<CombineInstance>();

        List<List<CombineInstance>> combineLists = new List<List<CombineInstance>>();
        int vertexCount = 0;
        combineLists.Add(new List<CombineInstance>());
        for (int i = 0; i < combine.Count; i++)
        {
            vertexCount += combine[i].mesh.vertexCount;
            if (vertexCount > 65536)
            {
                vertexCount = 0;
                combineLists.Add(new List<CombineInstance>());
                i--;
            }
            else
            {
                combineLists.Last().Add(combine[i]);
            }
        }

        //Transform meshys = new GameObject("Chunk").transform;
        foreach(List<CombineInstance> list in combineLists)
        {
            GameObject g = new GameObject("Chunk");
            g.transform.parent = this.transform;
            MeshFilter mf = g.AddComponent<MeshFilter>();
            MeshRenderer mr = g.AddComponent<MeshRenderer>();
            mf.mesh.CombineMeshes(list.ToArray());
        }*/
    }

    public void UploadMesh()
    {
        meshData.UploadMesh();

        if (meshRenderer == null)
            ConfigureComponent();

        meshFilter.mesh = meshData.mesh;

        if (meshData.vertices.Count > 3)
            meshCollider.sharedMesh = meshData.mesh;
    }

    public void UploadChildMesh(GameObject g)
    {
        meshData.UploadMesh();

        if (meshRenderer == null)
            ConfigureChildComponent(g);

        meshFilter.mesh = meshData.mesh;

        if (meshData.vertices.Count > 3)
            meshCollider.sharedMesh = meshData.mesh;
    }

    public Voxel this[Vector3 index]
    {
        get
        {
            if (data.ContainsKey(index))
                return data[index];
            else
                return emptyVoxel;
        }
        set
        {
            if (data.ContainsKey(index))
                data[index] = value;
            else
                data.Add(index, value);
        }
    }

    public static Voxel emptyVoxel = new Voxel() { ID = 0 };

    #region Mesh Data

    public struct MeshData
    {
        public Mesh mesh;
        public List<Vector3> vertices;
        public List<int> triangles;
        public List<Vector2> UVs;

        public List<Vector3> d;

        public List<List<Vector3>> verticesList;

        public List<Vector2> UVs2;
        public List<Color> colors;

        public bool Initialized;

        public void ClearData()
        {
            if(!Initialized)
            {
                vertices = new List<Vector3>();
                triangles = new List<int>();
                UVs = new List<Vector2>();

                d = new List<Vector3>();

                verticesList = new List<List<Vector3>>();

                UVs2 = new List<Vector2>();
                colors = new List<Color>();

                Initialized = true;
                mesh = new Mesh();
            }
            else
            {
                vertices.Clear();
                triangles.Clear();
                UVs.Clear();

                d.Clear();

                UVs2.Clear();
                colors.Clear();

                mesh.Clear();
            }
        }
        public void UploadMesh(bool sharedVertices = false)
        {
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0, false);
            mesh.SetUVs(0, UVs);

            mesh.SetUVs(2, UVs2);
            mesh.SetColors(colors);

            mesh.Optimize();

            mesh.RecalculateNormals();

            mesh.RecalculateBounds();

            mesh.UploadMeshData(false);
        }
    }

    #endregion


    #region Voxel Statics
    static readonly Vector3[] voxelVertices = new Vector3[8]
    {
        new Vector3(0, 0, 0),//0
        new Vector3(1, 0, 0),//1
        new Vector3(0, 1, 0),//2
        new Vector3(1, 1, 0),//3

        new Vector3(0, 0, 1),//4
        new Vector3(1, 0, 1),//5
        new Vector3(0, 1, 1),//6
        new Vector3(1, 1, 1),//7
    };

    static readonly Vector3[] voxelFaceChecks = new Vector3[6]
    {
        new Vector3(0, 0, -1),
        new Vector3(0, 0, 1),
        new Vector3(-1, 0, 0),
        new Vector3(1, 0, 0),
        new Vector3(0, -1, 0),
        new Vector3(0, 1, 0)
    };

    static readonly int[,] voxelVertexIndex = new int[6, 4]
    {
        {0, 1, 2, 3},
        {4, 5, 6, 7},
        {4, 0, 6, 2},
        {5, 1, 7, 3},
        {0, 1, 4, 5},
        {2, 3, 6, 7},
    };

    static readonly Vector2[] voxelUVs = new Vector2[4]
    {
        new Vector2(0, 0),
        new Vector2(0, 1),
        new Vector2(1, 0),
        new Vector2(1, 1),
    };

    static readonly int[,] voxelTris = new int[6, 6]
    {
        {0, 2, 3, 0, 3, 1},
        {0, 1, 2, 1, 3, 2},
        {0, 2, 3, 0, 3, 1},
        {0, 1, 2, 1, 3, 2},
        {0, 1, 2, 1, 3, 2},
        {0, 2, 3, 0 ,3, 1},
    };

    #endregion


    #region VoxelPlane Statics

    static readonly Vector3[] planeVertices = new Vector3[4]
    {
        new Vector3(-3, -3, 0),//0 -1
        new Vector3(3, -3, 0),//1 1
        new Vector3(-3, 3, 0),//2 -1
        new Vector3(3, 3, 0),//3 1
    };

    static readonly Vector3[] planeFaceChecks = new Vector3[1]
    {
        new Vector3(1, 0, 0),
    };

    static readonly int[,] planeVertexIndex = new int[1, 4]
    {
        {0, 1, 2, 3},
    };

    static readonly Vector2[] planeUVs = new Vector2[4]
    {
        new Vector2(0, 0),
        new Vector2(0, 1),
        new Vector2(1, 0),
        new Vector2(1, 1),
    };

    static readonly int[,] planeTris = new int[1, 6]
    {
        {0, 2, 3, 0, 3, 1},
    };

    #endregion
}
