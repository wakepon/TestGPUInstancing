using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class TestGPUInstancing : MonoBehaviour
{
    [SerializeField] private Mesh mesh;
    [SerializeField] private Material material;
    [SerializeField] private Vector3[] positions;
    [SerializeField] private Vector3[] rotations;
    [SerializeField] private Vector3[] sizes;
    [SerializeField] private Color[] colors;

    private GraphicsBuffer _argsBuffer;
    private readonly uint[] _args = new uint[5];

    private GraphicsBuffer _positionBuffer;
    private static readonly int PositionID = Shader.PropertyToID("_Positions");

    private GraphicsBuffer _rotationBuffer;
    private static readonly int RotationID = Shader.PropertyToID("_Rotations");

    private GraphicsBuffer _colorBuffer;
    private static readonly int ColorID = Shader.PropertyToID("_Colors");

    private GraphicsBuffer _sizeBuffer;
    private static readonly int SizeID = Shader.PropertyToID("_Sizes");

    private static readonly Bounds DrawBounds = new Bounds(Vector3.zero, Vector3.one * 1000);

    private void Awake()
    {
        _args[0] = mesh.GetIndexCount(0);
    }

    private void Update()
    {
        UpdateView();
        Draw();
    }

    private void UpdateView()
    {
        int count = positions.Length;
        UpdateArgs(count);

        UpdateColors(colors);
        UpdateRotations(rotations);
        UpdateSizes(sizes);
        UpdatePositions(positions);
    }

    private void UpdateArgs(int count)
    {
        _argsBuffer?.Release();

        _args[1] = (uint) count;
        _argsBuffer = new GraphicsBuffer(
            GraphicsBuffer.Target.IndirectArguments,
            1,
            _args.Length * sizeof(uint));

        _argsBuffer.SetData(_args);
    }

    private void UpdatePositions(Vector3[] nextPositionList)
    {
        _positionBuffer?.Release();
        _positionBuffer = new GraphicsBuffer(
            GraphicsBuffer.Target.Structured,
            nextPositionList.Length,
            Marshal.SizeOf<Vector3>());

        _positionBuffer.SetData(nextPositionList);
        material.SetBuffer(PositionID, _positionBuffer);
    }

    private void UpdateRotations(Vector3[] nextRotationList)
    {
        _rotationBuffer?.Release();
        _rotationBuffer = new GraphicsBuffer(
            GraphicsBuffer.Target.Structured,
            nextRotationList.Length,
            Marshal.SizeOf<Vector3>());
    
        _rotationBuffer.SetData(nextRotationList);
        material.SetBuffer(RotationID, _rotationBuffer);
    }
    
    private void UpdateColors(Color[] nextColorList)
    {
        _colorBuffer?.Release();
        _colorBuffer = new GraphicsBuffer(
            GraphicsBuffer.Target.Structured,
            nextColorList.Length,
            Marshal.SizeOf<Color>());
    
        _colorBuffer.SetData(nextColorList);
        material.SetBuffer(ColorID, _colorBuffer);
    }
    
    private void UpdateSizes(Vector3[] nextSizeList)
    {
        _sizeBuffer?.Release();
        _sizeBuffer = new GraphicsBuffer(
            GraphicsBuffer.Target.Structured,
            nextSizeList.Length,
            Marshal.SizeOf<Vector3>());
    
        _sizeBuffer.SetData(nextSizeList);
        material.SetBuffer(SizeID, _sizeBuffer);
    }

    private void Draw()
    {
        if (_argsBuffer == null)
            return;
        Graphics.DrawMeshInstancedIndirect(
            mesh,
            0,
            material,
            DrawBounds,
            _argsBuffer);
    }

    private void OnDestroy()
    {
        _argsBuffer?.Dispose();
        _positionBuffer?.Dispose();
        _rotationBuffer?.Dispose();
        _colorBuffer?.Dispose();
    }
}