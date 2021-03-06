using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ChangeFog : TriggerScripts
{
    public bool enableFog = true;
    public Color fogColor;
    public float fogStart, fogEnd;
    public float needTime;
    public List<Tween> tweening = new List<Tween>();

    void OnTriggerEnter(Collider other)
    {
        if (line != null)
        {
            if (other.gameObject.Equals(line.gameObject))
            {
                RenderSettings.fog = enableFog;
                DOTween.To(() => RenderSettings.fogColor, x => RenderSettings.fogColor = x, fogColor, needTime).SetId<Tween>("FogChanger");
                DOTween.To(() => RenderSettings.fogStartDistance, x => RenderSettings.fogStartDistance = x, fogStart, needTime).SetId<Tween>("FogChanger");
                DOTween.To(() => RenderSettings.fogEndDistance, x => RenderSettings.fogEndDistance = x, fogEnd, needTime).SetId<Tween>("FogChanger");
            }
        }
        else if (other.GetComponent<MainLine>())
        {
            RenderSettings.fog = enableFog;
            DOTween.To(() => RenderSettings.fogColor, x => RenderSettings.fogColor = x, fogColor, needTime).SetId<Tween>("FogChanger");
            DOTween.To(() => RenderSettings.fogStartDistance, x => RenderSettings.fogStartDistance = x, fogStart, needTime).SetId<Tween>("FogChanger");
            DOTween.To(() => RenderSettings.fogEndDistance, x => RenderSettings.fogEndDistance = x, fogEnd, needTime).SetId<Tween>("FogChanger");
        }
    }        
}
