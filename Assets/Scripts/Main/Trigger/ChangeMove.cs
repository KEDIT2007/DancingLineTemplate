using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeMove : TriggerScripts
{

    public enum Turn
    {
        RIGHT, LEFT
    }

    public Turn turn;

    public void OnTriggerEnter(Collider other)
    {
        if (line != null)
        {
            if (other.gameObject.Equals(line.gameObject))
            {
                MainLine.AddTurnEvent(line, Change);
            }
        }
        else if (other.GetComponent<MainLine>())
        {
            MainLine.AddTurnEvent(other.GetComponent<MainLine>(), Change);
        }
    }

    public void Change(object sender, LineEventArgs e)
    {
        e.Line.NewLineTail(e.Line.isGrounded);
        int dir = (turn == Turn.LEFT) ? 1 : -1;
        MainLine.AddExtraDirection(e.Line, dir);
        e.ChangeMove = true;
        MainLine.RemoveTurnEvent(e.Line, Change);
    }
}
