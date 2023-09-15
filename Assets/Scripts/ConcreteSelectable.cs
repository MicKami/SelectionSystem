using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConcreteSelectable : SelectableBase
{
    public override void Deselect()
    {
        print(gameObject.name + " deselected");
    }

    public override void Select()
    {
        print(gameObject.name + " selected");
    }
}
