using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;

public class DieCube : MonoBehaviour
{
    public float needTime;
    public float waitTime;
    public Collider sphere;
    public Transform[] cubes;

    public void Show(Vector3 position)
    {
        sphere.transform.position = position;
        foreach (Transform obj in cubes)
        {
            obj.localPosition = new Vector3(Random.Range(-0.125f, 0.125f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
            float ra = Random.Range(0.5f, 0.85f);
            obj.localScale = Vector3.one * ra;
        }
        sphere.enabled = true;
    }

    void Start()
    {
        Invoke("Fade", waitTime);
    }

    void Fade()
    {
        cubes.ToList().ForEach(obj => obj.transform.DOScale(0f, needTime));
    }
}
