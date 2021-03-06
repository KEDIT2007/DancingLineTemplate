using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Collider))]
public class TriggerScripts : MonoBehaviour
{
    [Tooltip("允许触发脚本的线\n若不选则允许所有线触发脚本")]
    public MainLine line;

    public void Reset()
    {
        if (GetComponents<Collider>().All(col => !col.isTrigger))
        {
            Debug.LogError(name + "物体的碰撞箱没有勾选 Is Trigger 可能导致异常");
        }
    }

}
