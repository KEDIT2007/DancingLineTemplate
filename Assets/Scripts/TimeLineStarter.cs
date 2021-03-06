using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class TimeLineStarter : MonoBehaviour
{
    public PlayableDirector[] TimeLines;

    public void Start()
    {
        FindObjectOfType<MainLine>().LineStart += StartTimeLine;
        foreach (PlayableDirector tl in TimeLines)
        {
            tl.Pause();
        }
    }

    public void StartTimeLine(object sender, LineEventArgs e)
    {
        foreach (PlayableDirector tl in TimeLines)
        {
            tl.Play();
        }
    }
}
