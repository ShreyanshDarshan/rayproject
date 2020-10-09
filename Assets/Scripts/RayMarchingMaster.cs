using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RayMarchingMaster : MonoBehaviour
{
    public ComputeShader RayTracingShader;
    private RenderTexture _target;
    private uint _currentSample = 0;
    private Material _addMaterial;
    public Texture SkyboxTexture;
    public OctreeArray TreeArray;
    public float Roughness = 1.0f;
    private ComputeBuffer buffer;
    private RenderTexture PreviousFrame;
    private RenderTexture OldPos;
    private RenderTexture NewPos;
    private Matrix4x4 _prev_proj;
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
        Graphics.Blit(_target, PreviousFrame);
        RayTracingShader.SetTexture(0, "Result", _target);
        RayTracingShader.SetTexture(0, "PreviousFrame", PreviousFrame);
        Graphics.Blit(NewPos, OldPos);
        RayTracingShader.SetTexture(0, "NewPos", NewPos);
        RayTracingShader.SetTexture(0, "OldPos", OldPos);
        int threadGroupsX = Mathf.CeilToInt(Screen.width / 32.0f);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / 32.0f);
        RayTracingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);
        //// Blit the result texture to the screen
        //Graphics.Blit(_target, destination);
        // Blit the result texture to the screen
        if (_addMaterial == null)
            _addMaterial = new Material(Shader.Find("Hidden/AddShader"));
        _addMaterial.SetFloat("_Sample", 0);
        Graphics.Blit(_target, destination, _addMaterial);
        //GameObject.Find("RawImage").GetComponent<RawImage>().texture = _target;
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
        if (PreviousFrame == null || PreviousFrame.width != Screen.width || PreviousFrame.height != Screen.height)
        {
            // Release render texture if we already have one
            if (PreviousFrame != null)
                PreviousFrame.Release();
            // Get a render PreviousFrame for Ray Tracing
            PreviousFrame = new RenderTexture(Screen.width, Screen.height, 0,
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            PreviousFrame.enableRandomWrite = true;
            PreviousFrame.Create();
        }
        if (OldPos == null || OldPos.width != Screen.width || OldPos.height != Screen.height)
        {
            // Release render texture if we already have one
            if (OldPos != null)
                OldPos.Release();
            // Get a render PreviousFrame for Ray Tracing
            OldPos = new RenderTexture(Screen.width, Screen.height, 0,
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            OldPos.enableRandomWrite = true;
            OldPos.Create();
        }
        if (NewPos == null || NewPos.width != Screen.width || NewPos.height != Screen.height)
        {
            // Release render texture if we already have one
            if (NewPos != null)
                NewPos.Release();
            // Get a render PreviousFrame for Ray Tracing
            NewPos = new RenderTexture(Screen.width, Screen.height, 0,
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            NewPos.enableRandomWrite = true;
            NewPos.Create();
        }
    }

    private void Update()
    {
        if (transform.hasChanged)
        {
            _currentSample = 1;
            transform.hasChanged = false;
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            Roughness *= 2;
        }
        else if (Input.GetKeyDown(KeyCode.K))
        {
            Roughness /= 2;
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
        RayTracingShader.SetMatrix("_WorldToCamera", _prev_proj);
        RayTracingShader.SetMatrix("_PreviousProjection", _camera.projectionMatrix);
        RayTracingShader.SetMatrix("_CameraToWorld", _camera.cameraToWorldMatrix);
        RayTracingShader.SetMatrix("_CameraInverseProjection", _camera.projectionMatrix.inverse);
        RayTracingShader.SetTexture(0, "_SkyboxTexture", SkyboxTexture);
        RayTracingShader.SetVector("_PixelOffset", new Vector2(Random.value, Random.value));//new Vector2(0, 0));
        RayTracingShader.SetVector("_PixelRand", new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)));//new Vector2(0, 0));
        RayTracingShader.SetFloat("_Seed", Random.value);
        RayTracingShader.SetFloat("_Roughness", Roughness);
        RayTracingShader.SetBuffer(0, "octree", buffer);
        _prev_proj = _camera.worldToCameraMatrix;
    }

    private void OnApplicationQuit()
    {
        buffer.Dispose();
        Debug.Log("Disposing Compute Buffer");
    }
}
