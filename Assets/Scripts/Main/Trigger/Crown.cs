using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class Crown : TriggerScripts, IResetable, IRevivable
{

    [Tooltip("皇冠模型")]public GameObject CrownModel;

    [Tooltip("第几个皇冠")]public int count;

    public void Start()
    {
        if (CrownModel == null)
        {
            Debug.LogError("皇冠的模型未选中");
            enabled = false;
        }
    }

    public void Update()
    {
        CrownModel.transform.rotation = Quaternion.Euler(CrownModel.transform.eulerAngles + Vector3.up * 60f * Time.deltaTime);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (line != null)
        {
            if (other.Equals(line))
            {
                RecordValue(line);
            }
        }
        else if (other.GetComponent<MainLine>())
        {
            RecordValue(other.GetComponent<MainLine>());
        }
    }

    public void ObjectReset()
    {
        CrownModel.GetComponent<MeshRenderer>().gameObject.SetActive(true);
        GetComponent<Collider>().enabled = true;
    }

    #region 静态方法
    public static void RecordValue(MainLine line)
    {
        var objs =
                from obj in FindObjectsOfType<GameObject>()
                where obj.GetComponent<IRevivable>() != null
                from comp in obj.GetComponents<IRevivable>()
                select comp;
        objs.ToList().ForEach(obj => obj.SetReviveValues(line));
    }

    public static void CallRevive(int count)
    {
        var objs =
                from obj in FindObjectsOfType<GameObject>()
                where obj.GetComponent<IRevivable>() != null
                from comp in obj.GetComponents<IRevivable>()
                select comp;
        var first = objs.Where(obj => obj.GetType().GetMethod("ObjectRevive").GetCustomAttributes(false).Any(
            attri => attri.GetType() == typeof(FirstCallWhenResetAttribute)));
        var last = objs.Where(obj => obj.GetType().GetMethod("ObjectRevive").GetCustomAttributes(false).Any(
            attri => attri.GetType() == typeof(LastCallWhenResetAttribute)));
        var rest = objs.Where(obj => obj.GetType().GetMethod("ObjectRevive").GetCustomAttributes(false).All(
            attri => attri.GetType() != typeof(LastCallWhenResetAttribute) &&
                attri.GetType() != typeof(FirstCallWhenResetAttribute)));
        first.ToList().ForEach(obj => obj.ObjectRevive(count));
        rest.ToList().ForEach(obj => obj.ObjectRevive(count));
        last.ToList().ForEach(obj => obj.ObjectRevive(count));
    }
    #endregion


    public virtual void SetReviveValues(MainLine line)
    {
        CrownModel.GetComponent<MeshRenderer>().gameObject.SetActive(false);
        GetComponent<Collider>().enabled = false;
    }

    public virtual void ObjectRevive(int count)
    {

    }
}

#region 接口
//可在复活时执行
public interface IRevivable
{
    void SetReviveValues(MainLine line);
    void ObjectRevive(int count);
}
#endregion

#region 标签
/// <summary>
/// 在重置时最后被调用
/// </summary>
sealed class LastCallWhenReviveAttribute : Attribute
{

}
/// <summary>
/// 在重置时最先被调用
/// </summary>
sealed class FirstCallWhenReviveAttribute : Attribute
{

}
#endregion