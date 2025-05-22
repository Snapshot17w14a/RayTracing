using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BenchmarkManager : MonoBehaviour
{
    [SerializeField] private List<string> scenesToBenchmark;
    [SerializeField] private float sceneWarmupTime = 2f;
    [SerializeField] private float testDuration = 10f;

    [SerializeField] private RectTransform progressBarTransform;

    [SerializeField] private int sceneCount;

    [SerializeField] private List<string> validSceneNames;

    private WaitForSeconds warmupTime;
    private int currentBenchmarkIndex = -1;

    private string graphicsAPI;

    private readonly List<float> frameTimes = new();

    private void Start()
    {
        //Read the command line args
        string[] args = System.Environment.GetCommandLineArgs();
        foreach (string s in args)
        {
            Debug.Log(s);
            if (!validSceneNames.Contains(s)) continue;
            scenesToBenchmark.Add(s);
        }

        //Log error if there are no scenes provided
        if (scenesToBenchmark.Count == 0)
        {
            Debug.LogError("[Benchmark] Scene array was empty");
            Destroy(gameObject);
            Application.Quit();
            return;
        }

        //Initialize stuff
        Application.targetFrameRate = -1;
        QualitySettings.vSyncCount = 0;
        Screen.fullScreen = true;
        graphicsAPI = SystemInfo.graphicsDeviceType.ToString();
        warmupTime = new WaitForSeconds(sceneWarmupTime);

        //Run first benchmark
        RunNextBenchmark();
    }

    private void RunNextBenchmark()
    {
        if (currentBenchmarkIndex < scenesToBenchmark.Count - 1)
        {
            StartCoroutine(RunBenchmarkOnScene(scenesToBenchmark[++currentBenchmarkIndex]));
            return;
        }

        Debug.Log("[Benchmark] All benchmarks completed.");
        Application.Quit();
    }

    private IEnumerator RunBenchmarkOnScene(string sceneName)
    {
        //Load the scene to benchmark
        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        //Wait for the scene to warm up
        yield return warmupTime;

        Debug.Log("[Benchmark] Recording started");

        //Start recording frame times
        float elapsed = 0f;
        frameTimes.Clear();
        while (elapsed < testDuration)
        {
            float dt = Time.unscaledDeltaTime;
            frameTimes.Add(dt);
            elapsed += dt;
            progressBarTransform.sizeDelta = new Vector2(elapsed / testDuration * 1920f, 25f);
            yield return null;
        }

        //Calculate stats for the run
        var rtRenderer = FindFirstObjectByType<CameraRayTraceRender>();
        var currentTime = System.DateTime.Now;

        float avgFPS = frameTimes.Count / frameTimes.Sum();
        float minFPS = 1f / Mathf.Max(frameTimes.Max(), 0.0001f);
        float maxFPS = 1f / Mathf.Max(frameTimes.Min(), 0.0001f);
        float medianFPS = 1f / frameTimes.OrderBy(t => t).ElementAt(frameTimes.Count / 2);

        string log = $"[{currentTime}] Test: {sceneName} | Graphics API: {graphicsAPI}" + 
                     (rtRenderer != null ? $"\nRT Settings: Rays per pixel {rtRenderer.RaysPerPixel} | Max bounces: {rtRenderer.MaxBounces}\n" : "\n") +
                     $"Duration: {testDuration}s | Warmup: {sceneWarmupTime}s\n" +
                     $"Avg FPS: {avgFPS:F2} | Min FPS: {minFPS:F2} | Max FPS: {maxFPS:F2} | Median FPS: {medianFPS:F2}\n\n" +
                     "Raw data:\n";

        //Append the individual frame times
        foreach (var t in frameTimes) log += t.ToString() + "\n";

        Debug.Log(log);

        //Create the file
        var file = File.Create(Application.persistentDataPath + $"/Results/{sceneName}-Benchmark.txt");

        //Using UTF8 encoding get the byte array and write it to the file
        var byteLog = System.Text.Encoding.UTF8.GetBytes(log);
        file.Write(byteLog, 0, byteLog.Length);
        file.Close();

        //Unload the scene
        yield return SceneManager.UnloadSceneAsync(sceneName);

        //Run the next benchmark
        RunNextBenchmark();
        yield return null;
    }
}
