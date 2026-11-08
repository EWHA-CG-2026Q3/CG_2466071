using UnityEngine;

// 중간 필수 ④: 회전행렬을 직접 만들어 별 정점과 법선에 적용
[RequireComponent(typeof(MeshFilter))]
public class Mid_StarRotation : MonoBehaviour
{
    public float degreesPerSecond = 18f;
    public bool animate = true;
    public float CurrentAngle { get; private set; }
    Mesh runtimeMesh, source;
    Vector3[] original, originalNormals, output, normals;
    void Awake()
    {
        source=GetComponent<MeshFilter>().sharedMesh;
        if(source==null){enabled=false;return;}
        runtimeMesh=Instantiate(source); runtimeMesh.name="Mid_Star_Runtime"; runtimeMesh.MarkDynamic();
        GetComponent<MeshFilter>().sharedMesh=runtimeMesh;
        original=source.vertices; originalNormals=source.normals;
        output=new Vector3[original.Length]; normals=new Vector3[original.Length];
    }
    public static Matrix4x4 RotationY(float degrees)
    {
        float a=degrees*Mathf.Deg2Rad, c=Mathf.Cos(a), s=Mathf.Sin(a);
        Matrix4x4 r=Matrix4x4.identity;
        r.m00=c; r.m02=s; r.m20=-s; r.m22=c;
        return r;
    }
    void Update()
    {
        if(!animate||runtimeMesh==null)return;
        CurrentAngle=(Time.time*degreesPerSecond)%360;
        Matrix4x4 r=RotationY(CurrentAngle);
        for(int i=0;i<original.Length;i++)
        {
            output[i]=r.MultiplyPoint3x4(original[i]);
            normals[i]=r.MultiplyVector(originalNormals[i]);
        }
        runtimeMesh.vertices=output; runtimeMesh.normals=normals; runtimeMesh.RecalculateBounds();
    }
    void OnDestroy(){if(runtimeMesh!=null)Destroy(runtimeMesh);}
}
