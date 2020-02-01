using System;
using Barracuda;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class Classifier : MonoBehaviour
{
    public NNModel modelFile;
    public TextAsset labelsFile;

    public const int IMAGE_SIZE = 224;
    private const int IMAGE_MEAN = 127;
    private const float IMAGE_STD = 127.5f;
    private const string INPUT_NAME = "input";
    private const string OUTPUT_NAME = "MobilenetV2/Predictions/Reshape_1";

    private IWorker worker;
    private string[] labels;


    public void Start()
    {
        this.labels = Regex.Split(this.labelsFile.text, "\n|\r|\r\n")
            .Where(s => !String.IsNullOrEmpty(s)).ToArray();
        var model = ModelLoader.Load(this.modelFile);
        this.worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, model);
    }


    private int i = 0;
    public IEnumerator Classify(Color32[] picture, System.Action<List<KeyValuePair<string, float>>> callback)
    {
        var map = new List<KeyValuePair<string, float>>();

        using (var tensor = TransformInput(picture, IMAGE_SIZE, IMAGE_SIZE))
        {
            var inputs = new Dictionary<string, Tensor>();
            inputs.Add(INPUT_NAME, tensor);  
            var enumerator = this.worker.ExecuteAsync(inputs);

            while (enumerator.MoveNext())
            {
                i++;
                if (i >= 20)
                {
                    i = 0;
                    yield return null;
                }
            };

            // this.worker.Execute(inputs);
            // Execute() scheduled async job on GPU, waiting till completion
            // yield return new WaitForSeconds(0.5f);

            var output = worker.PeekOutput(OUTPUT_NAME);

            for (int i = 0; i < labels.Length; i++)
            {
                map.Add(new KeyValuePair<string, float>(labels[i], output[i] * 100));
            }
        }

        callback(map.OrderByDescending(x => x.Value).ToList());
    }


    public static Tensor TransformInput(Color32[] pic, int width, int height)
    {
        float[] floatValues = new float[width * height * 3];

        for (int i = 0; i < pic.Length; ++i)
        {
            var color = pic[i];

            floatValues[i * 3 + 0] = (color.r - IMAGE_MEAN) / IMAGE_STD;
            floatValues[i * 3 + 1] = (color.g - IMAGE_MEAN) / IMAGE_STD;
            floatValues[i * 3 + 2] = (color.b - IMAGE_MEAN) / IMAGE_STD;
        }

        return new Tensor(1, height, width, 3, floatValues);
    }
}