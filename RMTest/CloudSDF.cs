using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CloudSDF : MonoBehaviour
{
    public Camera currentCamera;

    public GameObject[] sphere;
    public Material mat;

    public int stepCount; // 遍历次数
    public float stepSize; // 遍历步长
    public float k;

    public Mesh mesh;

    private void Awake()
    {
        //currentCamera = GetComponent<Camera>();
        currentCamera.depthTextureMode = DepthTextureMode.Depth;
        //this.RenderCloud();
    }

    private void Start()
    {
        //this.RenderCloud();
    }

    private void Update()
    {
        this.RenderCloud();
    }

    private void OnValidate()
    {
        // 方便调整参数，观察效果
        Shader.SetGlobalFloat("_StepCount", stepCount);
        Shader.SetGlobalFloat("_StepSize", stepSize);
        Shader.SetGlobalFloat("_K", k);
    }

    void RenderCloud()
    {
        Shader.SetGlobalVector("_SpherePos", sphere[0].transform.position);
        Shader.SetGlobalVector("_SpherePos1", sphere[1].transform.position);
        
        for (int i = 0; i < sphere.Length; i++)
        {
            Graphics.DrawMesh(mesh, sphere[i].transform.position, Quaternion.identity, mat, 0);
        }
    }
}