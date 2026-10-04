using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RawImage))]
public class S07_Clipping : MonoBehaviour
{
    [SerializeField] private int canvasWidth = 256;
    [SerializeField] private int canvasHeight = 256;
    [SerializeField] private int clipMargin = 40;

    // 왼쪽(x < 40)과 위쪽(y > 216) 경계를 넘도록 직접 설정
    [SerializeField] private List<Vector2> polygon = new List<Vector2>
    {
        new Vector2(10, 130),
        new Vector2(130, 250),
        new Vector2(246, 130)
    };

    [SerializeField] private Color fillColor =
        new Color(1f, 0.4f, 0.2f, 1f);

    [SerializeField] private Color backgroundColor = Color.black;
    [SerializeField] private Color outlineColor = Color.gray;

    private Texture2D canvasTexture;
    private RawImage targetImage;

    private void OnEnable()
    {
        RedrawAll();
    }

    private void OnValidate()
    {
        RedrawAll();
    }

    private void RedrawAll()
    {
        targetImage = GetComponent<RawImage>();

        if (targetImage == null || canvasWidth <= 0 || canvasHeight <= 0)
            return;

        if (canvasTexture == null ||
            canvasTexture.width != canvasWidth ||
            canvasTexture.height != canvasHeight)
        {
            canvasTexture = new Texture2D(canvasWidth, canvasHeight);
            canvasTexture.filterMode = FilterMode.Point;
        }

        FillBackground();
        DrawMarginOutline();

        // 각 경계로 자른 결과를 다음 경계의 입력으로 사용
        List<Vector2> clipped = polygon;
        clipped = ClipLeft(clipped, clipMargin);
        clipped = ClipRight(clipped, canvasWidth - clipMargin);
        clipped = ClipBottom(clipped, clipMargin);
        clipped = ClipTop(clipped, canvasHeight - clipMargin);

        FillPolygon(clipped, fillColor);

        canvasTexture.Apply();
        targetImage.texture = canvasTexture;
    }

    private void FillBackground()
    {
        for (int x = 0; x < canvasWidth; x++)
        {
            for (int y = 0; y < canvasHeight; y++)
            {
                canvasTexture.SetPixel(x, y, backgroundColor);
            }
        }
    }

    // 클리핑 영역을 나타내는 회색 사각형
    private void DrawMarginOutline()
    {
        int left = clipMargin;
        int right = canvasWidth - clipMargin;
        int bottom = clipMargin;
        int top = canvasHeight - clipMargin;

        for (int x = left; x <= right; x++)
        {
            canvasTexture.SetPixel(x, bottom, outlineColor);
            canvasTexture.SetPixel(x, top, outlineColor);
        }

        for (int y = bottom; y <= top; y++)
        {
            canvasTexture.SetPixel(left, y, outlineColor);
            canvasTexture.SetPixel(right, y, outlineColor);
        }
    }

    // 왼쪽 경계: x >= boundary가 안쪽
    private List<Vector2> ClipLeft(
        List<Vector2> input, float boundary)
    {
        List<Vector2> output = new List<Vector2>();

        for (int i = 0; i < input.Count; i++)
        {
            Vector2 current = input[i];
            Vector2 previous =
                input[(i - 1 + input.Count) % input.Count];

            bool currentInside = current.x >= boundary;
            bool previousInside = previous.x >= boundary;

            if (currentInside)
            {
                if (!previousInside)
                {
                    output.Add(
                        GetIntersectionX(previous, current, boundary)
                    );
                }

                output.Add(current);
            }
            else if (previousInside)
            {
                output.Add(
                    GetIntersectionX(previous, current, boundary)
                );
            }
        }

        return output;
    }

    // TODO: ClipLeft를 참고해서 오른쪽 경계(x <= boundary)로 자르는 함수를 완성
    private List<Vector2> ClipRight(
        List<Vector2> input, float boundary)
    {
        List<Vector2> output = new List<Vector2>();

        for (int i = 0; i < input.Count; i++)
        {
            Vector2 current = input[i];
            Vector2 previous =
                input[(i - 1 + input.Count) % input.Count];

            bool currentInside = current.x <= boundary;
            bool previousInside = previous.x <= boundary;

            if (currentInside)
            {
                if (!previousInside)
                {
                    output.Add(
                        GetIntersectionX(previous, current, boundary)
                    );
                }

                output.Add(current);
            }
            else if (previousInside)
            {
                output.Add(
                    GetIntersectionX(previous, current, boundary)
                );
            }
        }

        return output;
    }

    // TODO: 아래쪽 경계(y >= boundary)로 자르는 함수를 완성
    private List<Vector2> ClipBottom(
        List<Vector2> input, float boundary)
    {
        List<Vector2> output = new List<Vector2>();

        for (int i = 0; i < input.Count; i++)
        {
            Vector2 current = input[i];
            Vector2 previous =
                input[(i - 1 + input.Count) % input.Count];

            bool currentInside = current.y >= boundary;
            bool previousInside = previous.y >= boundary;

            if (currentInside)
            {
                if (!previousInside)
                {
                    output.Add(
                        GetIntersectionY(previous, current, boundary)
                    );
                }

                output.Add(current);
            }
            else if (previousInside)
            {
                output.Add(
                    GetIntersectionY(previous, current, boundary)
                );
            }
        }

        return output;
    }

    // TODO: 위쪽 경계(y <= boundary)로 자르는 함수를 완성
    private List<Vector2> ClipTop(
        List<Vector2> input, float boundary)
    {
        List<Vector2> output = new List<Vector2>();

        for (int i = 0; i < input.Count; i++)
        {
            Vector2 current = input[i];
            Vector2 previous =
                input[(i - 1 + input.Count) % input.Count];

            bool currentInside = current.y <= boundary;
            bool previousInside = previous.y <= boundary;

            if (currentInside)
            {
                if (!previousInside)
                {
                    output.Add(
                        GetIntersectionY(previous, current, boundary)
                    );
                }

                output.Add(current);
            }
            else if (previousInside)
            {
                output.Add(
                    GetIntersectionY(previous, current, boundary)
                );
            }
        }

        return output;
    }

    // 세로 경계선 x = boundaryX와 변의 교점
    private Vector2 GetIntersectionX(
        Vector2 p1, Vector2 p2, float boundaryX)
    {
        float t = (boundaryX - p1.x) / (p2.x - p1.x);

        return new Vector2(
            boundaryX,
            p1.y + t * (p2.y - p1.y)
        );
    }

    // TODO: GetIntersectionX를 참고해서 y 기준 교차점을 구하는 함수를 완성
    private Vector2 GetIntersectionY(
        Vector2 p1, Vector2 p2, float boundaryY)
    {
        float t = (boundaryY - p1.y) / (p2.y - p1.y);

        return new Vector2(
            p1.x + t * (p2.x - p1.x),
            boundaryY
        );
    }

    // 잘린 다각형을 여러 삼각형으로 나누어 채우기
    private void FillPolygon(List<Vector2> poly, Color color)
    {
        if (poly.Count < 3)
            return;

        for (int i = 1; i < poly.Count - 1; i++)
        {
            DrawTriangle(
                poly[0],
                poly[i],
                poly[i + 1],
                color
            );
        }
    }

    private void DrawTriangle(
        Vector2 a, Vector2 b, Vector2 c, Color color)
    {
        for (int x = 0; x < canvasWidth; x++)
        {
            for (int y = 0; y < canvasHeight; y++)
            {
                Vector2 p = new Vector2(x + 0.5f, y + 0.5f);

                if (IsInsideTriangle(p, a, b, c))
                {
                    canvasTexture.SetPixel(x, y, color);
                }
            }
        }
    }

    private bool IsInsideTriangle(
        Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float denom =
            a.x * (b.y - c.y) +
            b.x * (c.y - a.y) +
            c.x * (a.y - b.y);

        if (Mathf.Abs(denom) < 0.000001f)
            return false;

        float w1 =
            (p.x * (b.y - c.y) +
             b.x * (c.y - p.y) +
             c.x * (p.y - b.y))
            / denom;

        float w2 =
            (a.x * (p.y - c.y) +
             p.x * (c.y - a.y) +
             c.x * (a.y - p.y))
            / denom;

        float w3 = 1f - w1 - w2;

        return w1 >= 0f && w2 >= 0f && w3 >= 0f;
    }
}