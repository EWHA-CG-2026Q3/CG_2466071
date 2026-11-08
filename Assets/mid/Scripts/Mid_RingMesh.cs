using System.Collections.Generic;
using UnityEngine;

// 중간 필수 ①: 반복문으로 정점과 삼각형을 만들어 원형 화단 생성
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Mid_RingMesh : MonoBehaviour
{
    [Range(8, 128)] public int segments = 64;
    public float outerRadius = 3.7f;
    public float innerRadius = 3.25f;
    public float height = 0.65f;

    public static Mesh Build(int n, float outer, float inner, float h)
    {
        n = Mathf.Clamp(n, 8, 128);
        outer = Mathf.Max(outer, 0.02f);
        inner = Mathf.Clamp(inner, 0.001f, outer - 0.001f);
        h = Mathf.Max(h, 0.01f);
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var uv = new List<Vector2>();
        for (int i = 0; i < n; i++)
        {
            float a = 2 * Mathf.PI * i / n;
            float b = 2 * Mathf.PI * (i + 1) / n;
            Vector3 oa = new Vector3(outer * Mathf.Cos(a), 0, outer * Mathf.Sin(a));
            Vector3 ob = new Vector3(outer * Mathf.Cos(b), 0, outer * Mathf.Sin(b));
            Vector3 ia = new Vector3(inner * Mathf.Cos(a), 0, inner * Mathf.Sin(a));
            Vector3 ib = new Vector3(inner * Mathf.Cos(b), 0, inner * Mathf.Sin(b));
            Vector3 up = Vector3.up * h;
            // 윗면·바깥 벽·안쪽 벽·밑면 구성, 네 정점마다 삼각형 두 개 생성
            AddQuad(vertices, triangles, uv, oa + up, ia + up, ib + up, ob + up);
            AddQuad(vertices, triangles, uv, oa, oa + up, ob + up, ob);
            AddQuad(vertices, triangles, uv, ia, ib, ib + up, ia + up);
            AddQuad(vertices, triangles, uv, oa, ob, ib, ia);
        }
        var mesh = new Mesh { name = "Mid_LoopGeneratedRing" };
        mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0); mesh.SetUVs(0, uv);
        mesh.RecalculateNormals(); mesh.RecalculateTangents(); mesh.RecalculateBounds();
        return mesh;
    }

    static void AddQuad(List<Vector3> v, List<int> t, List<Vector2> uv,
        Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        int s = v.Count;
        v.Add(a); v.Add(b); v.Add(c); v.Add(d);
        uv.Add(new Vector2(0, 0)); uv.Add(new Vector2(0, 1));
        uv.Add(new Vector2(1, 1)); uv.Add(new Vector2(1, 0));
        t.Add(s); t.Add(s+1); t.Add(s+2);
        t.Add(s); t.Add(s+2); t.Add(s+3);
    }

    void Awake()
    {
        // 저장된 메시가 없을 때만 복원, 평소에는 씬에 저장한 메시 사용
        if (GetComponent<MeshFilter>().sharedMesh == null)
            GetComponent<MeshFilter>().sharedMesh = Build(segments, outerRadius, innerRadius, height);
    }
}
