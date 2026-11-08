using System.Collections.Generic;
using UnityEngine;

// 중간 선택 ①: 앞뒷면과 옆면을 연결해 두께 있는 별 생성
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Mid_StarMesh : MonoBehaviour
{
    public float outerRadius = 0.7f, innerRadius = 0.3f, depth = 0.18f;
    public static Mesh Build(float outer, float inner, float depth)
    {
        var v = new List<Vector3>(); var t = new List<int>(); var uv = new List<Vector2>();
        for (int side = -1; side <= 1; side += 2)
        for (int i = 0; i < 10; i++)
        {
            int j = (i + 1) % 10;
            float a = Mathf.PI / 2 + i * Mathf.PI / 5;
            float b = Mathf.PI / 2 + j * Mathf.PI / 5;
            float r1 = i % 2 == 0 ? outer : inner, r2 = j % 2 == 0 ? outer : inner;
            Vector3 p = new Vector3(Mathf.Cos(a)*r1, Mathf.Sin(a)*r1, 0);
            Vector3 q = new Vector3(Mathf.Cos(b)*r2, Mathf.Sin(b)*r2, 0);
            int start = v.Count;
            v.Add(new Vector3(0, 0, side * depth));
            v.Add(side > 0 ? p : q); v.Add(side > 0 ? q : p);
            t.Add(start); t.Add(start+1); t.Add(start+2);
            for(int k=0;k<3;k++) uv.Add(new Vector2(v[start+k].x/(2*outer)+.5f,v[start+k].y/(2*outer)+.5f));
        }
        var m = new Mesh { name = "Mid_OriginalStar" };
        m.SetVertices(v); m.SetTriangles(t,0); m.SetUVs(0,uv);
        m.RecalculateNormals(); m.RecalculateTangents(); m.RecalculateBounds(); return m;
    }
    void Awake()
    {
        if(GetComponent<MeshFilter>().sharedMesh==null)
            GetComponent<MeshFilter>().sharedMesh=Build(outerRadius,innerRadius,depth);
    }
}
