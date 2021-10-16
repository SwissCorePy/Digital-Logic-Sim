using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
public class CustomButton : Button
{
    public List<Action> events = new List<Action>();
    public event Action ONPointerDown;

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        ONPointerDown?.Invoke();
    }

    public void AddListener(Action action)
    {
        ONPointerDown += action;
        events.Add(action);
    }

    public void ClearEvents()
    {
        foreach (var a in events) ONPointerDown -= a;
        events.Clear();
    }
}
}