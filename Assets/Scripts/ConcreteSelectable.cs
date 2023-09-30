using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConcreteSelectable : SelectableBase
{
    [Layer]
    public int outlinesLayer;
    private int previousLayer;


    public override void Deselect()
    {
        gameObject.layer = previousLayer;
        //print(gameObject.name + " deselected");
    }

    public override void Select()
    {
        previousLayer = gameObject.layer;
        gameObject.layer = outlinesLayer;
        //print(gameObject.name + " selected");
    }
}
