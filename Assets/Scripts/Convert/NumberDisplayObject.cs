using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumberDisplayObject
{
    public int value { get; private set; }
    public int displayValue { get; private set; }
    public event Action onValueChangeAction;
    public event Action onDisplayChangeAction;
    
    public NumberDisplayObject(int val)
    {
        this.setValue(val, true);
    }

    public void setValue(int val, bool refreshDisplay = false)
    {
        this.value = val;
        if (refreshDisplay) {
            this.displayValue = this.value;
            this.onDisplayChangeAction?.Invoke();
        }
        this.onValueChangeAction?.Invoke();
    }

    public void refresh()
    {
        this.displayValue = this.value;
        this.onDisplayChangeAction?.Invoke();
    }
}
