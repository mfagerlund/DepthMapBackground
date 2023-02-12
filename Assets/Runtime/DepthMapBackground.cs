using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;

public class DepthMapBackground : MonoBehaviour
{
    [field: SerializeField] private MeshFilter _meshFilter;
    [field: SerializeField] private Texture2D _depthMap;

    [field: Header("Blur")] [field: SerializeField, Range(0, 12)]
    private int _boxBlurRange = 1;

    [field: SerializeField, Range(0, 12)] private int _boxBlurIterations = 1;

    [field: Header("Range Select")] [field: SerializeField, Range(0, 1f)]
    private float _minDepth = 0;

    [field: SerializeField, Range(0, 1f)] private float _maxDepth = 1;

    [field: Header("Depth Scaler"), SerializeField]
    private AnimationCurve _depthCurve;

    [field: Header("Divisions")] [field: SerializeField, Range(0, 16), Tooltip("Defines how high the resolution of the mesh will be, 2^SubDivisions, to be exact.")]
    private int _subDivisions = 6;

    public void GenerateMesh()
    {
        if (_meshFilter == null)
        {
            _meshFilter = GetComponent<MeshFilter>();
        }

        if (_meshFilter == null)
        {
            throw new InvalidOperationException("No Mesh Filter specified!");
        }

        var pixels = _depthMap.GetPixels();
        var maxDivisions = 1 << _subDivisions;
        var raw = new Vector2Int(_depthMap.width, _depthMap.height);
        var rawDepths = GetDepths(raw.x, raw.y, pixels);

        var wh = raw.x >= raw.y
            ? new Vector2Int(maxDivisions, maxDivisions * raw.y / raw.x)
            : new Vector2Int(maxDivisions * raw.x / raw.y, maxDivisions);

        var scale = new Vector2((float)raw.x / wh.x, (float)raw.y / wh.y);
        var mesh = new Mesh { name = "DepthMapMesh" };

        var depths = GetDownscaledDepths(wh, scale, rawDepths, raw);
        BlurDepths(depths);


        // Jagged array would be faster, I know.
        var vertexIds = new int[wh.x, wh.y];
        var vertexList = new List<Vector3>(wh.x * wh.y);
        var depthList = new List<float>(wh.x * wh.y);
        var uvs = new List<Vector2>(wh.x * wh.y);
        var div = new Vector2(1f / wh.x, 1f / wh.y);

        ForXy(wh, p =>
        {
            var depth = depths[p.x, p.y];
            if (_depthCurve != null && _depthCurve.length > 1)
            {
                depth = _depthCurve.Evaluate(depth);
            }

            var vertex =
                new Vector3(
                    (p.x - wh.x / 2f) / maxDivisions,
                    (p.y - wh.y / 2f) / maxDivisions,
                    depth - 0.5f);
            vertexIds[p.x, p.y] = vertexList.Count;
            vertexList.Add(vertex);
            depthList.Add(depths[p.x, p.y]);
            uvs.Add(Vector2.Scale(p, div));
        });


        var triangles = new List<int>();
        ForXy(wh - Vector2Int.one, p =>
        {
            var p00 = vertexIds[p.x, p.y];
            var p10 = vertexIds[p.x + 1, p.y];
            var p01 = vertexIds[p.x, p.y + 1];
            var p11 = vertexIds[p.x + 1, p.y + 1];

            if (IsInRange(p11) && IsInRange(p10) && IsInRange(p00))
            {
                triangles.Add(p11);
                triangles.Add(p10);
                triangles.Add(p00);
            }

            if (IsInRange(p01) && IsInRange(p11) && IsInRange(p00))
            {
                triangles.Add(p01);
                triangles.Add(p11);
                triangles.Add(p00);
            }

            bool IsInRange(int vertexId)
            {
                var depth = depthList[vertexId];
                return depth >= _minDepth && depth <= _maxDepth;
            }
        });

        mesh.indexFormat = IndexFormat.UInt32;
        mesh.vertices = vertexList.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        Debug.Log($"Number of vertices: {mesh.vertexCount}, Number of triangles: {mesh.triangles.Length}");
        _meshFilter.mesh = mesh;
    }

    private void BlurDepths(float[,] depths)
    {
        if (_boxBlurRange > 0 && _boxBlurIterations > 0)
        {
            BoxBlur.FastBoxBlur(depths, _boxBlurRange, _boxBlurIterations);
        }
    }

    private static float[,] GetDownscaledDepths(Vector2Int wh, Vector2 scale, float[,] rawDepths, Vector2Int raw)
    {
        var depths = new float[wh.x, wh.y];
        ForXy(wh, p =>
        {
            var depthMapPoint = Vector2.Scale(p, scale);
            depths[p.x, p.y] = rawDepths[
                Mathf.Clamp((int)depthMapPoint.x, 0, raw.x),
                Mathf.Clamp((int)depthMapPoint.y, 0, raw.y)];
        });
        return depths;
    }

    private static float[,] GetDepths(int w, int h, Color[] pixels)
    {
        var depths = new float[w, h];
        ForXy(new Vector2Int(w, h), p =>
        {
            var index = p.y * w + p.x;
            depths[p.x, p.y] = 1 - pixels[index].r;
        });
        return depths;
    }
    
    public static void ForXy(Vector2Int vector2Int, Action<Vector2Int> action)
    {
        var v = new Vector2Int();
        for (int y = 0; y < vector2Int.y; y++)
        {
            v.y = y;
            for (int x = 0; x < vector2Int.x; x++)
            {
                v.x = x;
                action(v);
            }
        }
    }
}