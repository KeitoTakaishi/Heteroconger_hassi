﻿using System.Collections;
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
        float lifeTime;

        public Params(Vector3 emitPos, Vector3 pos)
        {
            this.emitPos = emitPos;
            this.position = pos;
            this.lifeTime = 3.0f;
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
    private int cachedInstanceCount = -1;
    private ComputeBuffer cBuffer;
    private ComputeBuffer argsBuffer; //<---DMII
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
    private Renderer render;
    private Vector2 times;

    private int SubMeshIndex = 0; //<---DMII
    private Bounds bounds; //<---DMII




    private int bufferPropId;
    private int timesPropId;
    private int lifePropId;
    private int modelMatrixPropId;
    private int mousePropId;



    private void Initialize()
    {
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
        //---


        UpdateBuffers();

    }

    private void Update()
    {
        times.x = Time.deltaTime;
        times.y += Time.deltaTime;


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
        var mousePos = Input.mousePosition;
        mousePos.z = 5.0f;
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

        for (int i = 0; i < instanceCount; i++)
        {
            //var pos = UnityEngine.Random.insideUnitSphere * 15.0f;
            var mousePos = Input.mousePosition;
            mousePos.z = 5.0f;
            mousePos = Camera.main.ScreenToWorldPoint(mousePos);
            var pos = UnityEngine.Random.insideUnitSphere * 5.0f + mousePos;
            parameters[i] = new Params(UnityEngine.Random.insideUnitSphere, pos);
        }

        cBuffer.SetData(parameters);
        uint numIndices = (instanceMesh != null) ? (uint)instanceMesh.GetIndexCount(0) : 0;
        args[0] = numIndices;
        args[1] = (uint)instanceCount;
        argsBuffer.SetData(args);
        cachedInstanceCount = instanceCount;
    }
}

