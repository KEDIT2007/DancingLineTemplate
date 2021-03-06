using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class EventTrigger : TriggerScripts
{
    public UnityEvent TriggerEvents;
    public MainLine.LINE_EVENT eventType;

    void OnTriggerEnter(Collider other)
    {
        if (line != null)
        {
            if (other.gameObject.Equals(line.gameObject))
            {
                InvokeEvents(line);
            }
        }
        else if (other.GetComponent<MainLine>())
        {
            InvokeEvents(other.GetComponent<MainLine>());
        }
    }

    public void InvokeEvents(MainLine line)
    {
        switch(eventType)
        {
            case MainLine.LINE_EVENT.IMMEDIATE:
                DoInvoke(this, new LineEventArgs(line, line.transform.position, line.nowForward));
                break;
            case MainLine.LINE_EVENT.START:
                MainLine.AddStartEvent(line, DoInvoke);
                break;
            case MainLine.LINE_EVENT.DIE:
                MainLine.AddDieEvent(line, DoInvoke);
                break;
            case MainLine.LINE_EVENT.TURN:
                MainLine.AddTurnEvent(line, DoInvoke);
                break;
            case MainLine.LINE_EVENT.GROUND:
                MainLine.AddGroundEvent(line, DoInvoke);
                break;
            case MainLine.LINE_EVENT.OFFGROUND:
                MainLine.AddOffGroundEvent(line, DoInvoke);
                break;
        }
    }

    public void DoInvoke(object sender, LineEventArgs e)
    {
        TriggerEvents.Invoke();
    }

}
