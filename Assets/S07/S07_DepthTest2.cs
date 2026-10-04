using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class S07_DepthTest2 : MonoBehaviour
{
    [SerializeField] private int canvasWidth = 256;
    [SerializeField] private int canvasHeight = 256;

    // 삼각형 1: 파랑, 더 멀리 있음
    [SerializeField] private Vector3 vertexA1 = new Vector3(130, 210, 0.5f);
    [SerializeField] private Vector3 vertexB1 = new Vector3(85, 50, 0.5f);
    [SerializeField] private Vector3 vertexC1 = new Vector3(220, 50, 0.5f);

    // 삼각형 2: 주황, 더 가까이 있음
    [SerializeField] private Vector3 vertexA2 = new Vector3(75, 175, 0.3f);
    [SerializeField] private Vector3 vertexB2 = new Vector3(30, 70, 0.3f);
    [SerializeField] private Vector3 vertexC2 = new Vector3(165, 70, 0.3f);

    // 삼각형 3: 초록
    [SerializeField] private Vector3 vertexA3 = new Vector3(190, 210, 0.9f);
    [SerializeField] private Vector3 vertexB3 = new Vector3(135, 50, 0.25f);
    [SerializeField] private Vector3 vertexC3 = new Vector3(255, 50, 0.25f);

    [SerializeField] private Color color1 = new Color(0.1f, 0.45f, 1f, 1f);
    [SerializeField] private Color color2 = new Color(1f, 0.35f, 0.1f, 1f);
    [SerializeField] private Color color3 = new Color(0.2f, 0.9f, 0.3f, 1f);
    [SerializeField] private Color backgroundColor = Color.black;

    private Texture2D canvasTexture;
    private RawImage targetImage;

    // 각 픽셀에서 현재까지 가장 가까운 z값을 저장
    private float[,] depthBuffer;

    void OnEnable()
    {
        RedrawAll();
    }

    void OnValidate()
    {
        RedrawAll();
    }

    private void RedrawAll()
    {
        targetImage = GetComponent<RawImage>();

        if (targetImage == null)
        {
            return;
        }

        if (canvasTexture == null ||
            canvasTexture.width != canvasWidth ||
            canvasTexture.height != canvasHeight)
        {
            canvasTexture = new Texture2D(canvasWidth, canvasHeight);
            canvasTexture.filterMode = FilterMode.Point;
        }

        depthBuffer = new float[canvasWidth, canvasHeight];
        ClearCanvasAndDepthBuffer();
        // TODO 0: 이 세 줄의 순서를 바꿔도 결과가 유지되는지 확인
        DrawTriangle(vertexA1, vertexB1, vertexC1, color1);
        DrawTriangle(vertexA2, vertexB2, vertexC2, color2);
        DrawTriangle(vertexA3, vertexB3, vertexC3, color3);


        canvasTexture.Apply();
        targetImage.texture = canvasTexture;
    }

    private void ClearCanvasAndDepthBuffer()
    {
        for (int x = 0; x < canvasWidth; x++)
        {
            for (int y = 0; y < canvasHeight; y++)
            {
                canvasTexture.SetPixel(x, y, backgroundColor);

                // 아직 아무것도 그려지지 않았으므로 가장 먼 값
                depthBuffer[x, y] = float.MaxValue;
            }
        }
    }

    private void DrawTriangle(Vector3 a, Vector3 b, Vector3 c, Color color)
    {
        for (int x = 0; x < canvasWidth; x++)
        {
            for (int y = 0; y < canvasHeight; y++)
            {
                // 현재 검사할 픽셀의 중심 = P
                Vector2 p = new Vector2(x + 0.5f, y + 0.5f);

                float w1;
                float w2;
                float w3;

                bool isInside = TryGetBarycentric(
                    p,
                    new Vector2(a.x, a.y),
                    new Vector2(b.x, b.y),
                    new Vector2(c.x, c.y),
                    out w1,
                    out w2,
                    out w3
                );

                if (isInside)
                {
                    // TODO 1: w1,w2,w3와 a.z, b.z, c.z를 이용해 보간된 z계산 
                    float interpolatedZ = //현재 검사중인 픽셀p에서의 z값
                        w1 * a.z +
                        w2 * b.z +
                        w3 * c.z;

                    // TODO 2: interpolatedZ가 depthBuffer[x,y]보다 작을 때만 갱신
                    if (interpolatedZ < depthBuffer[x, y]) //interpolatedZ가 더 작다 = 새 삼각형이 더 가깝다 = 색 바꾸고 z값도 새값으로
                    {
                        canvasTexture.SetPixel(x,y,color);
                        depthBuffer[x,y] = interpolatedZ;
                    }
                }
            }
        }
    }

    // 가중치를 구하고, 세 가중치가 모두 0 이상이면 true
    private bool TryGetBarycentric(
        Vector2 p,
        Vector2 a,
        Vector2 b,
        Vector2 c,
        out float w1,
        out float w2,
        out float w3)
    {
        float denom =
            a.x * (b.y - c.y) +
            b.x * (c.y - a.y) +
            c.x * (a.y - b.y);

        w1 =
            (p.x * (b.y - c.y) +
            b.x * (c.y - p.y) +
            c.x * (p.y - b.y))
            / denom;

        w2 =
            (a.x * (p.y - c.y) +
            p.x * (c.y - a.y) +
            c.x * (a.y - p.y))
            / denom;

        w3 = 1f - w1 - w2;

        return w1 >= 0f && w2 >= 0f && w3 >= 0f;
    }
}