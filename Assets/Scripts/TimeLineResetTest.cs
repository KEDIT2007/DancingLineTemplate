using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class TimeLineResetTest : MonoBehaviour, IResetable
{
    public PlayableDirector timeLine;

    private double time = 0;

    // Start is called before the first frame update
    void Start()
    {
        FindObjectOfType<MainLine>().LineDie += LineDie;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LineDie(object sender, LineEventArgs e)
    {
        timeLine.Pause();
    }

    public void ObjectReset()
    {
        if (timeLine != null)
        {
            StartCoroutine(TimeLineReset());
        }
    }

    IEnumerator TimeLineReset()
    {
        timeLine.time = time;
        timeLine.Play();
        yield return null;
        timeLine.Pause();
    }
}
