using UnityEngine;

public class S06_ImmediateModeTriangle_Finish : MonoBehaviour
{
    [SerializeField] private Material glMaterial;

    [SerializeField] private Vector3 vertexA = new Vector3(0f, 0f, 0f);
    [SerializeField] private Vector3 vertexB = new Vector3(1f, 0f, 0f);
    [SerializeField] private Vector3 vertexC = new Vector3(0.5f, 1f, 0f);

    [SerializeField] private Color triangleColor =
        new Color(1f, 0.6f, 0.2f, 1f);

    void OnRenderObject()
    {
        glMaterial.SetPass(0);

        GL.Begin(GL.TRIANGLES);

        GL.Color(triangleColor);

        GL.Vertex3(vertexA.x, vertexA.y, vertexA.z);
        GL.Vertex3(vertexB.x, vertexB.y, vertexB.z);
        GL.Vertex3(vertexC.x, vertexC.y, vertexC.z);

        GL.End();
    }
}