using UnityEngine;
using UnityEngine.UI;

public class GameTime : MonoBehaviour
{
    private Text timeText;

    public float Time { get; set; } = 0;

    void Awake()
    {
        timeText = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (StateManager.Instance.activeState is Game)
        {
            Time += UnityEngine.Time.deltaTime;
        }
        else
        {
            Time = 0;
        }

        timeText.text = GetTimeStr();
    }

    string GetTimeStr()
    {
        System.TimeSpan ts = System.TimeSpan.FromSeconds(Time);

        string timeStr = string.Format("{0:D2}:{1:D2}", ts.Minutes, ts.Seconds);

        if (ts.Hours > 0)
        {
            timeStr = string.Format("{0:D2}:{1:D2}:{2:D2}", ts.Hours, ts.Minutes, ts.Seconds);
        }

        return timeStr;
    }
}
