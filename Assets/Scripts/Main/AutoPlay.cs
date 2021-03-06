using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;

public class AutoPlay : MonoBehaviour, IResetable
{
    public bool useBpm;
    public int bpm;
    public bool doPlay;
    public MainLine line;
    public List<float> time = new List<float>();
    int index = -1;
    Coroutine autoCoroutine;

    private void Start()
    {
        if (doPlay)
        {
            line.LineStart += Line_LineStart;
            line.LineTurn += Line_LineTurn;
        }
    }

    private void Line_LineTurn(object sender, LineEventArgs e)
    {
        e.AllowTurn = false;
    }

    private void Line_LineStart(object sender, LineEventArgs e)
    {
        autoCoroutine = StartCoroutine(Auto());
    }

    public IEnumerator Auto()
    {
        while (++index < time.Count)
        {
            float nextTime;
            if (useBpm)
            {
                nextTime = (60f / bpm) * time[index];
            }
            else
            {
                nextTime = time[index];
            }
            yield return new WaitForSecondsRealtime(nextTime);
            line.Turn();
        }
    }

    public void ObjectReset()
    {
        if (doPlay)
        {
            index = -1;
            if (autoCoroutine != null)
            {
                StopCoroutine(autoCoroutine);
            }
            line.LineStart += Line_LineStart;
        }
    }
}
