using UnityEngine;

public class AverageFPSCounter : MonoBehaviour
{
    public float updateInterval = 0.5f; // Update FPS every 0.5 seconds
    private float accum = 0.0f; // FPS accumulated over the interval
    private int frames = 0; // Frames drawn over the interval
    private float timeleft; // Left time for current interval

    private float averageFPS = 0.0f; // Average FPS

    void Start()
    {
        timeleft = updateInterval;
    }

    void Update()
    {
        timeleft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        frames++;

        // Interval ended - update GUI text and start new interval
        if (timeleft <= 0.0)
        {
            // Calculate average FPS
            averageFPS = accum / frames;

            // Reset variables for next interval
            timeleft = updateInterval;
            accum = 0.0f;
            frames = 0;
        }
    }

    void OnGUI()
    {
        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(0, 0, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 100;
        style.normal.textColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);

        string text = string.Format("FPS: {0:F2}", averageFPS);
        GUI.Label(rect, text, style);
    }
}
