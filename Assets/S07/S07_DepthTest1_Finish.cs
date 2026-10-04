using UnityEngine;
using UnityEngine.UI;

public class S07_DepthTest1_Finish : MonoBehaviour
{
    [SerializeField] private int canvasWidth = 256;
    [SerializeField] private int canvasHeight = 256;

    // 삼각형 1: 파랑, 더 멀리 있음
    [SerializeField] private Vector3 vertexA1 = new Vector3(130, 210, 0.7f);
    [SerializeField] private Vector3 vertexB1 = new Vector3(85, 50, 0.7f);
    [SerializeField] private Vector3 vertexC1 = new Vector3(220, 50, 0.7f);

    // 삼각형 2: 주황, 더 가까이 있음
    [SerializeField] private Vector3 vertexA2 = new Vector3(75, 175, 0.3f);
    [SerializeField] private Vector3 vertexB2 = new Vector3(30, 70, 0.3f);
    [SerializeField] private Vector3 vertexC2 = new Vector3(165, 70, 0.3f);

    [SerializeField] private Color color1 = new Color(0.1f, 0.45f, 1f, 1f);
    [SerializeField] private Color color2 = new Color(1f, 0.35f, 0.1f, 1f);
    [SerializeField] private Color backgroundColor = Color.black;

    private Texture2D canvasTexture;
    private RawImage targetImage;

    // 각 픽셀에서 현재까지 가장 가까운 z값을 저장
    private float[,] depthBuffer;

    void Start()
    {
        targetImage = GetComponent<RawImage>();

        if (targetImage == null)
        {
            Debug.LogError("RawImage가 필요합니다.");
            return;
        }

        canvasTexture = new Texture2D(canvasWidth, canvasHeight);
        canvasTexture.filterMode = FilterMode.Point;

        depthBuffer = new float[canvasWidth, canvasHeight];

        // 배경과 깊이 버퍼는 시작할 때 한 번만 초기화
        ClearCanvasAndDepthBuffer();

        // 일부러 가까운 주황을 먼저 그림
        DrawTriangle(vertexA2, vertexB2, vertexC2, color2);

        // 멀리 있는 파랑을 나중에 그려도,
        // 겹친 곳은 depth test 때문에 주황이 유지됨
        DrawTriangle(vertexA1, vertexB1, vertexC1, color1);

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
                    // P에서의 z값을 가중치로 계산
                    float interpolatedZ =
                        w1 * a.z +
                        w2 * b.z +
                        w3 * c.z;

                    // 작은 z가 더 가까운 것으로 약속
                    if (interpolatedZ < depthBuffer[x, y])
                    {
                        canvasTexture.SetPixel(x, y, color);
                        depthBuffer[x, y] = interpolatedZ;
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