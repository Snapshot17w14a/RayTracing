using UnityEngine;
using UnityEngine.UI;

public class FPSTracker : MonoBehaviour
{
    [SerializeField] private int fpsBufferSize = 64;

    private Text fpsText;
    private float[] fpsReadings;
    private int currentIndex = -1;

    private float AvgFps
    {
        get
        {
            float sum = 0;
            for (int i = 0; i < fpsBufferSize; i++)
            {
                sum += fpsReadings[i];
            }
            return System.MathF.Round(sum / (float)fpsBufferSize, 2);
        }
    }

    private void Start()
    {
        fpsText = GetComponent<Text>();
        fpsReadings = new float[fpsBufferSize];
    }

    private void Update()
    {
        fpsReadings[++currentIndex % fpsBufferSize] = 1 / Time.deltaTime;
        fpsText.text = "FPS: " + AvgFps.ToString();
    }
}
