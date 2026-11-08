using UnityEngine;

// 중간 선택 ④: 매다는 점 기준으로 장식 왕복 회전
[RequireComponent(typeof(MeshFilter))]
public class Mid_OrnamentSwing : MonoBehaviour
{
    public float amplitude = 12f, frequency = 0.32f, phase;
    public Vector3 pivot = Vector3.zero;
    public bool animate = true;
    public float CurrentAngle { get; private set; }
    Mesh runtimeMesh, source;
    Vector3[] original, originalNormals, output, normals;
    void Awake()
    {
        source=GetComponent<MeshFilter>().sharedMesh;
        if(source==null){enabled=false;return;}
        runtimeMesh=Instantiate(source);runtimeMesh.name="Mid_Ornament_Runtime";runtimeMesh.MarkDynamic();
        GetComponent<MeshFilter>().sharedMesh=runtimeMesh;
        original=source.vertices;originalNormals=source.normals;
        output=new Vector3[original.Length];normals=new Vector3[original.Length];
    }
    public static Matrix4x4 PivotMatrix(Vector3 pivot,float degrees)
    {
        float a=degrees*Mathf.Deg2Rad,c=Mathf.Cos(a),s=Mathf.Sin(a);
        Matrix4x4 r=Matrix4x4.identity;r.m00=c;r.m01=-s;r.m10=s;r.m11=c;
        Matrix4x4 to=Matrix4x4.identity;to.m03=pivot.x;to.m13=pivot.y;to.m23=pivot.z;
        Matrix4x4 from=Matrix4x4.identity;from.m03=-pivot.x;from.m13=-pivot.y;from.m23=-pivot.z;
        // 오른쪽부터 중심을 원점으로 이동 → 회전 → 원래 위치로 이동
        return to*r*from;
    }
    void Update()
    {
        if(!animate||runtimeMesh==null)return;
        CurrentAngle=amplitude*Mathf.Sin(Time.time*2*Mathf.PI*frequency+phase);
        Matrix4x4 m=PivotMatrix(pivot,CurrentAngle);
        for(int i=0;i<original.Length;i++)
        {output[i]=m.MultiplyPoint3x4(original[i]);normals[i]=m.MultiplyVector(originalNormals[i]);}
        runtimeMesh.vertices=output;runtimeMesh.normals=normals;runtimeMesh.RecalculateBounds();
    }
    void OnDestroy(){if(runtimeMesh!=null)Destroy(runtimeMesh);}
}
