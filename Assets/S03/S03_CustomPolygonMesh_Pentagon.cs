using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class S03_CustomPolygonMesh_Pentagon : MonoBehaviour
{
    void Start()
    {
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(0f, 1f, 0f),        //0
            new Vector3(0.95f, 0.3f, 0f),   //1
            new Vector3(0.6f, -0.8f, 0f),   //2
            new Vector3(-0.6f, -0.8f, 0f),  //3
            new Vector3(-0.95f, 0.3f, 0f)   //4
        };

        int[] triangles = new int[]
        {
          0, 1, 2,
          0, 2, 3,
          0, 3, 4  
        };

        Mesh mesh = new Mesh();     //빈 mesh(도형) 하나 만들기
        mesh.vertices = vertices;   //만든 정점 정보 넣기
        mesh. triangles = triangles; //입력한 정점들로 삼각형 만들기
        mesh.RecalculateNormals();   //면이 어느 방향을 바라보고 있는지 다시 계산(방향정보) 빛계산가능해짐

        GetComponent<MeshFilter>().mesh = mesh; //만든 mesh 실제 게임오브젝트의 매쉬필터에 넣기(어떤 매쉬 사용할 지)
        GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit")); 
        //그 mesh를 어떻게 화면에 그릴 지
    }
}