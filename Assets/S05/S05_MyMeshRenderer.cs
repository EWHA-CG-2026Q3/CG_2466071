using UnityEngine;
using UnityEngine.UI;

public class S05_MyMeshRenderer : MonoBehaviour
{
    [SerializeField] private int canvasWidth = 256;
    [SerializeField] private int canvasHeight = 256;
    [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 1f); //검정

    [Header("무늬 실습 (줄무늬·체스판 공용)")]
    [SerializeField] private int patternSize = 16;
    [SerializeField] private Color colorA = new Color(1f, 1f, 1f, 1f); //흰색
    [SerializeField] private Color colorB = new Color(0.3f, 0.5f, 0.8f, 1f); //하늘색

    private Texture2D canvasTexture;
    private RawImage targetImage;

    void Start()
    {
        targetImage = GetComponent<RawImage>();

        // 1. 빈 텍스처 생성
        canvasTexture = new Texture2D(canvasWidth, canvasHeight);

        // 2. 픽셀 경계를 또렷하게
        canvasTexture.filterMode = FilterMode.Point;

        // 3. 과제 실습
        //FillBackground(colorA);
        //FillRandom();
        //FillVerticalStripes(patternSize, colorA, colorB);
        FillCheckerboard(patternSize, colorA, colorB);

        // 4. SetPixel로 바꾼 내용을 실제 텍스처에 반영
        canvasTexture.Apply();

        // 5. RawImage에 연결
        targetImage.texture = canvasTexture;
    }

    private void FillBackground(Color color)
    {
        for (int x = 0; x < canvasWidth; x++)
        {
            for (int y = 0; y < canvasHeight; y++)
            {
                canvasTexture.SetPixel(x, y, color);
            }
        }
    }

    private void FillRandom()
    {
        for (int x = 0; x < canvasWidth; x++)
        {
            for (int y = 0; y < canvasHeight; y++)
            {
                Color randomColor = new Color(
                    Random.value,
                    Random.value,
                    Random.value,
                    1f
                );

                canvasTexture.SetPixel(x, y, randomColor);
            }
        }
    }

    private void FillVerticalStripes(int width, Color colorA, Color colorB)
    {
        for (int x = 0; x < canvasWidth; x++)
        {
            bool isColorA = (x / width) % 2 == 0;
            Color stripeColor = isColorA ? colorA : colorB;

            for (int y = 0; y < canvasHeight; y++)
            {
                canvasTexture.SetPixel(x, y, stripeColor);
            }
        }
    }

    private void FillCheckerboard(int size, Color colorA, Color colorB)
    {
        for (int x = 0; x < canvasWidth; x++)
        {
            for (int y = 0; y < canvasHeight; y++)
            {
                bool isColorA = ((x / size) + (y / size)) % 2 == 0;
                Color pixelColor = isColorA ? colorA : colorB;

                canvasTexture.SetPixel(x, y, pixelColor);
            }
        }
    }

}