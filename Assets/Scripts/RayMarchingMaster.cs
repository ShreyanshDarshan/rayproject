﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayMarchingMaster : MonoBehaviour
{
    public ComputeShader RayTracingShader;
    private RenderTexture _target;
    private uint _currentSample = 0;
    private Material _addMaterial;
    public Texture SkyboxTexture;
    public OctreeArray TreeArray;
    private ComputeBuffer buffer;
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        SetShaderParameters();
        Render(destination);
    }
    private void Render(RenderTexture destination)
    {
        // Make sure we have a current render target
        InitRenderTexture();
        // Set the target and dispatch the compute shader
        RayTracingShader.SetTexture(0, "Result", _target);
        int threadGroupsX = Mathf.CeilToInt(Screen.width / 32.0f);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / 32.0f);
        RayTracingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);
        //// Blit the result texture to the screen
        //Graphics.Blit(_target, destination);
        // Blit the result texture to the screen
        if (_addMaterial == null)
            _addMaterial = new Material(Shader.Find("Hidden/AddShader"));
        _addMaterial.SetFloat("_Sample", _currentSample);
        Graphics.Blit(_target, destination, _addMaterial);
        _currentSample++;
    }
    private void InitRenderTexture()
    {
        //_currentSample = 0;
        if (_target == null || _target.width != Screen.width || _target.height != Screen.height)
        {
            // Release render texture if we already have one
            if (_target != null)
                _target.Release();
            // Get a render target for Ray Tracing
            _target = new RenderTexture(Screen.width, Screen.height, 0,
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            _target.enableRandomWrite = true;
            _target.Create();
        }
    }

    private void Update()
    {
        if (transform.hasChanged)
        {
            _currentSample = 0;
            transform.hasChanged = false;
        }
    }

    private Camera _camera;
    private void Start()
    {
        _camera = GetComponent<Camera>();
        buffer = new ComputeBuffer(TreeArray.headindex, 60);
        buffer.SetData(TreeArray.AllNodes);
    }

    private void SetShaderParameters()
    {
        RayTracingShader.SetMatrix("_CameraToWorld", _camera.cameraToWorldMatrix);
        RayTracingShader.SetMatrix("_CameraInverseProjection", _camera.projectionMatrix.inverse);
        RayTracingShader.SetTexture(0, "_SkyboxTexture", SkyboxTexture);
        RayTracingShader.SetVector("_PixelOffset", new Vector2(Random.value, Random.value));
        RayTracingShader.SetBuffer(0, "octree", buffer);
    }

    private void OnApplicationQuit()
    {
        buffer.Dispose();
        Debug.Log("Disposing Compute Buffer");
    }
}
