using System;
using System.IO;
using Barracuda;
using TFClassify;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;

public enum Mode
{
    Detect,
    Classify,
}

public class PhoneCamera : MonoBehaviour
{
    private static Texture2D boxOutlineTexture;
    private static GUIStyle labelStyle;

    private float cameraScale = 1f;
    private float shiftX = 0f;
    private float shiftY = 0f;
    private float scaleFactor = 1;
    private bool camAvailable;

    private WebCamTexture backCamera;
    private Texture defaultBackground;

    private bool isWorking = false;
    public Classifier classifier;
    public Detector detector;

    private IList<BoundingBox> boxOutlines;


    public Mode mode;
    public RawImage background;
    public AspectRatioFitter fitter;
    public Text uiText;


    private void Start()
    {
        this.backCamera = new WebCamTexture();
        this.background.texture = this.backCamera;
        this.backCamera.Play();
        camAvailable = true;

        boxOutlineTexture = new Texture2D(1, 1);
        boxOutlineTexture.SetPixel(0, 0, Color.red);
        boxOutlineTexture.Apply();

        labelStyle = new GUIStyle();
        labelStyle.fontSize = 50;
        labelStyle.normal.textColor = Color.red;

        CalculateShift(this.mode == Mode.Detect ? Detector.IMAGE_SIZE : Classifier.IMAGE_SIZE);
    }


    private void Update()
    {
        if (!this.camAvailable)
        {
            return;
        }

        float ratio = (float)backCamera.width / (float)backCamera.height;
        fitter.aspectRatio = ratio;

        float scaleX = cameraScale;
        float scaleY = backCamera.videoVerticallyMirrored ? -cameraScale : cameraScale;
        background.rectTransform.localScale = new Vector3(scaleX, scaleY, 1f);

        int orient = -backCamera.videoRotationAngle;
        background.rectTransform.localEulerAngles = new Vector3(0, 0, orient);

        if (orient != 0)
        {
            this.cameraScale = (float)Screen.width / Screen.height;
        }

        if (this.mode == Mode.Detect)
        {
            TFDetect();
        }
        else
        {
            TFClassify();
        }
    }


    public void OnGUI()
    {
        if (this.boxOutlines != null && this.boxOutlines.Any())
        {
            foreach (var outline in this.boxOutlines)
            {
                DrawBoxOutline(outline, scaleFactor, shiftX, shiftY);
            }
        }
    }


    private void CalculateShift(int inputSize)
    {
        int smallest;

        if (Screen.width < Screen.height)
        {
            smallest = Screen.width;
            this.shiftY = (Screen.height - smallest) / 2f;
        }
        else
        {
            smallest = Screen.height;
            this.shiftX = (Screen.width - smallest) / 2f;
        }

        this.scaleFactor = smallest / (float)inputSize;
    }


    private void TFClassify()
    {
        if (this.isWorking)
        {
            return;
        }

        this.isWorking = true;
        StartCoroutine(ProcessImage(Classifier.IMAGE_SIZE, result =>
        {
            StartCoroutine(this.classifier.Classify(result, probabilities =>
            {
                this.uiText.text = String.Empty;

                if (probabilities.Any())
                {
                    for (int i = 0; i < 3; i++)
                    {
                        this.uiText.text += probabilities[i].Key + ": " + String.Format("{0:0.000}%", probabilities[i].Value) + "\n";
                    }
                }

                Resources.UnloadUnusedAssets();
                this.isWorking = false;
            }));
        }));
    }


    private void TFDetect()
    {
        if (this.isWorking)
        {
            return;
        }

        this.isWorking = true;
        StartCoroutine(ProcessImage(Detector.IMAGE_SIZE, result =>
        {
            StartCoroutine(this.detector.Detect(result, boxes =>
            {
                this.boxOutlines = boxes;
                Resources.UnloadUnusedAssets();
                this.isWorking = false;
            }));
        }));
    }


    private IEnumerator ProcessImage(int inputSize, System.Action<Color32[]> callback)
    {
        yield return StartCoroutine(TextureTools.CropSquare(backCamera,
            TextureTools.RectOptions.Center, snap =>
            {
                var scaled = Scale(snap, inputSize);
                var rotated = Rotate(scaled.GetPixels32(), scaled.width, scaled.height);
                callback(rotated);
            }));
    }


    private void DrawBoxOutline(BoundingBox outline, float scaleFactor, float shiftX, float shiftY)
    {
        var x = outline.Dimensions.X * scaleFactor + shiftX;
        var width = outline.Dimensions.Width * scaleFactor;
        var y = outline.Dimensions.Y * scaleFactor + shiftY;
        var height = outline.Dimensions.Height * scaleFactor;

        DrawRectangle(new Rect(x, y, width, height), 4, Color.red);
        DrawLabel(new Rect(x + 10, y + 10, 200, 20), $"{outline.Label}: {(int)(outline.Confidence * 100)}%");
    }


    public static void DrawRectangle(Rect area, int frameWidth, Color color)
    {
        Rect lineArea = area;
        lineArea.height = frameWidth;
        GUI.DrawTexture(lineArea, boxOutlineTexture); // Top line

        lineArea.y = area.yMax - frameWidth;
        GUI.DrawTexture(lineArea, boxOutlineTexture); // Bottom line

        lineArea = area;
        lineArea.width = frameWidth;
        GUI.DrawTexture(lineArea, boxOutlineTexture); // Left line

        lineArea.x = area.xMax - frameWidth;
        GUI.DrawTexture(lineArea, boxOutlineTexture); // Right line
    }


    private static void DrawLabel(Rect position, string text)
    {
        GUI.Label(position, text, labelStyle);
    }


    private Texture2D Scale(Texture2D texture, int imageSize)
    {
        var scaled = TextureTools.scaled(texture, imageSize, imageSize, FilterMode.Bilinear);

        return scaled;
    }


    private Color32[] Rotate(Color32[] pixels, int width, int height)
    {
        return TextureTools.RotateImageMatrix(
                pixels, width, height, -90);
    }

    private Task<Texture2D> RotateAsync(Texture2D texture)
    {
        return Task.Run(() =>
        {
            return TextureTools.RotateTexture(texture, -90);
        });
    }


    private void SaveToFile(Texture2D texture)
    {
        var filePath = Application.persistentDataPath + "/" + "snap.png";
        File.WriteAllBytes(
            filePath, texture.EncodeToPNG());
    }
}
