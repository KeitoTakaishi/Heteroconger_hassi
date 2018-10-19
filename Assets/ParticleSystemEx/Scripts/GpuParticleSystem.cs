/*
 * setup
 * lives listをinspectorで指定
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Random = System.Random;
using System.Runtime.InteropServices;

[RequireComponent(typeof(MeshRenderer))]
public class GpuParticleSystem : MonoBehaviour
{
    struct Params
    {
        Vector3 emitPos;
        Vector3 position;
        Vector3 lifeTime;
        Vector4 velocity;

        public Params(Vector3 emitPos, Vector3 pos, float lifeTime)
        {
            this.emitPos = emitPos;
            this.position = pos;
            this.lifeTime = new Vector3(0.0f, lifeTime, -1.0f);
            //this.velocity = Vector3.zero;
            this.velocity = new Vector3(0.0f, UnityEngine.Random.RandomRange(0, 0.005f), 0.0f);
        }
    }

    public struct GPUThreads
    {
        public int x;
        public int y;
        public int z;

        public GPUThreads(uint x, uint y, uint z)
        {
            this.x = (int)x;
            this.y = (int)y;
            this.z = (int)z;
        }
    }

    [Serializable]
    public struct Colors
    {
        public Color startColor;
        public Color endColor;
    }

    [Serializable]
    public struct Sizes
    {
        [Range(0f, 10f)] public float startSize;
        [Range(0f, 10f)] public float endSize;
    }

    [Serializable]
    public struct Lives
    {
        [Range(0f, 60f)] public float minLife;
        [Range(0f, 60f)] public float maxLife;
    }

    #region variables

    private enum ComputeKernels
    {
        Emit,
        Iterator
    }

    #endregion

    private Dictionary<ComputeKernels, int> kernelMap = new Dictionary<ComputeKernels, int>();
    private GPUThreads gpuThreads;

    [SerializeField] int instanceCount = 100000;
    [SerializeField] Mesh instanceMesh;
    [SerializeField] Material instanceMaterial; //<---DMII
    [SerializeField] ComputeShader computeShader;


    private ComputeShader computeShaderInstance;
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
    private Renderer render;


    #region cBffuer
    private ComputeBuffer cBuffer;
    [SerializeField]List<Lives> lives = new List<Lives>();
    [SerializeField] Vector3 additionalVector;
    [SerializeField] float emitterSize = 10f;
    [SerializeField] float convergence = 4f;
    [SerializeField] float viscosity = 5f;
    #endregion


    #region drawmesh args
    private int SubMeshIndex = 0; //<---DMII
    private Bounds bounds; //<---DMII
    private ComputeBuffer argsBuffer; //<---DMII
    #endregion


    #region shaderID 
    private int bufferPropId;
    private int timesPropId;
    private int lifePropId;
    private int modelMatrixPropId;
    private int mousePropId;
    private int convergencePropId;
    private int viscosityPropId;
    private int additionalVectorPropId;
    private Vector2 times;
    #endregion




    private void Initialize()
    {
        times = new Vector2(0, 0);
        bounds = new Bounds(Vector3.zero, new Vector3(100, 100, 100));
        computeShaderInstance = computeShader;

        render = GetComponent<Renderer>();

        uint threadX, threadY, threadZ;



        //castでarray->ComputeKernels型へ
        kernelMap = Enum.GetValues(typeof(ComputeKernels))
           .Cast<ComputeKernels>()
           .ToDictionary(t => t, t => computeShaderInstance.FindKernel(t.ToString()));
        //emit kernelのグループスレッドサイズを取得
        computeShaderInstance.GetKernelThreadGroupSizes(kernelMap[ComputeKernels.Emit], out threadX, out threadY,
            out threadZ);
        gpuThreads = new GPUThreads(threadX, threadY, threadZ);
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);


        //---Vertex,FragmentShader
        bufferPropId = Shader.PropertyToID("buf");
        timesPropId = Shader.PropertyToID("timer");
        lifePropId = Shader.PropertyToID("lifeTime");
        modelMatrixPropId = Shader.PropertyToID("modelMatrix");
        mousePropId = Shader.PropertyToID("mousePos");
        convergencePropId = Shader.PropertyToID("convergence");
        viscosityPropId = Shader.PropertyToID("viscosity");
        additionalVectorPropId = Shader.PropertyToID("additionalVector");
        //---


        UpdateBuffers();

    }

    private void Update()
    {
        if(Time.realtimeSinceStartup < 1.5f)
        {
            return;
        }
        times.x = Time.deltaTime;
        times.y = Time.realtimeSinceStartup;


        //public 変数の設定などを変えた場合update bufferが必要
        //UpdateBuffers();
        

        //<------SubShader----------->
        instanceMaterial.SetPass(0);
        //StructuredBuffer<Params> buf;
        instanceMaterial.SetBuffer(bufferPropId, cBuffer);
        //matrixの転送
        //This property MUST be used instead of Transform.localToWorldMatrix, if you're setting shader parameters.
        var render = GetComponent<Renderer>();
        instanceMaterial.SetMatrix(modelMatrixPropId, render.localToWorldMatrix);
        instanceMaterial.SetVector(timesPropId, times);
        //<-------------------------->


        //<------ComputeShader----------->
        //computeShaderInstance.setBuffer
        computeShader.SetVector("times", times);
        computeShaderInstance.SetFloat(convergencePropId, convergence);
        computeShaderInstance.SetFloat(viscosityPropId, viscosity);
        computeShaderInstance.SetVector(additionalVectorPropId, additionalVector);

        var mousePos = Input.mousePosition;
        mousePos.z = 3.0f;
        mousePos = Camera.main.ScreenToWorldPoint(mousePos);
        computeShader.SetVector(mousePropId, mousePos);
        computeShaderInstance.SetBuffer(kernelMap[ComputeKernels.Emit], bufferPropId, cBuffer);
        computeShaderInstance.Dispatch(kernelMap[ComputeKernels.Emit], Mathf.CeilToInt((float)instanceCount / (float)gpuThreads.x), gpuThreads.y, gpuThreads.z);
        computeShaderInstance.SetBuffer(kernelMap[ComputeKernels.Iterator], bufferPropId, cBuffer);
        computeShaderInstance.Dispatch(kernelMap[ComputeKernels.Iterator], Mathf.CeilToInt((float)instanceCount / (float)gpuThreads.x), gpuThreads.y, gpuThreads.z);

        //Render
        Graphics.DrawMeshInstancedIndirect(instanceMesh, SubMeshIndex, instanceMaterial, bounds, argsBuffer);
    }

    private void Start()
    {
        Initialize();
    }

    void UpdateBuffers()
    {
        if (cBuffer != null)
            cBuffer.Release();

        cBuffer = new ComputeBuffer(instanceCount, Marshal.SizeOf(typeof(Params)));
        Params[] parameters = new Params[cBuffer.count];

        //compute bufferに渡すbufferの初期化
        for (int i = 0; i < instanceCount; i++)
        {
            //var pos = UnityEngine.Random.insideUnitSphere * 15.0f;
            var mousePos = Input.mousePosition;
            mousePos.z = 5.0f;
            mousePos = Camera.main.ScreenToWorldPoint(mousePos);
            var pos = UnityEngine.Random.insideUnitSphere * 5.0f + mousePos;
            var lifeTime = lives[UnityEngine.Random.RandomRange(0, lives.Count)];
            parameters[i] = new Params(UnityEngine.Random.insideUnitSphere, pos, UnityEngine.Random.RandomRange(lifeTime.minLife, lifeTime.maxLife));//v(emitPos, pos)
        }

        cBuffer.SetData(parameters);
        uint numIndices = (instanceMesh != null) ? (uint)instanceMesh.GetIndexCount(0) : 0;
        args[0] = numIndices;
        args[1] = (uint)instanceCount;
        argsBuffer.SetData(args);
        
    }
}

