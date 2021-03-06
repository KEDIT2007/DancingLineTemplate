using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class LineMusic : MonoBehaviour, IResetable
{
    public AudioClip BackgroundMusic;

    private AudioSource source;

    void Start()
    {
        MainLine line = FindObjectOfType<MainLine>();
        line.LineStart += Play;
        line.LineDie += Stop;
        source = GetComponent<AudioSource>();
    }

    public void Play(object sender, LineEventArgs e)
    {
        if (BackgroundMusic != null)
        {
            source.clip = BackgroundMusic;
            source.Play();
            source.time = e.Line.time;
        }
    }

    public void Stop(object sender, LineEventArgs e)
    {
        if (BackgroundMusic != null)
        {
            source.Stop();
        }
    }

    public void ObjectReset()
    {
        Start();
        if (BackgroundMusic != null)
        {
            source.Stop();
        }
    }
}
