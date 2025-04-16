using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class ShaderTool
{
    private static Dictionary<Type, ComputeBuffer> typeBufferPair = new();

    public static ComputeBuffer GetComputeBuffer<T>() where T : struct => typeBufferPair[typeof(T)];

    public static ComputeBuffer CreateComputeBuffer<T>(int count, bool createUnique = false) where T : struct
    {
        if (!createUnique && typeBufferPair.ContainsKey(typeof(T)) && typeBufferPair[typeof(T)].count == count) return null;

        if (typeBufferPair.TryGetValue(typeof(T), out ComputeBuffer buffer)) buffer.Release();

        typeBufferPair[typeof(T)] = new(count, Marshal.SizeOf<T>());
        return typeBufferPair[typeof(T)];
    }

    public static void SetBuffer<T>(T[] data) where T : struct
    {
        typeBufferPair[typeof(T)].SetData(data);
    }

    public static void InitRenderTexture(ref RenderTexture texture)
    {
        if (texture == null || texture.width != Screen.width || texture.height != Screen.height)
        {
            if (texture != null) texture.Release();

            texture = new RenderTexture(Screen.width, Screen.height, 0,
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);

            texture.enableRandomWrite = true;
            texture.Create();
        }
    }
}
