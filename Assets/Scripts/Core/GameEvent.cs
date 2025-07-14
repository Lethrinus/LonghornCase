// Assets/Scripts/Events/GameEvent.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/GameEvent")]
public class GameEvent : ScriptableObject {
    // no per-raise allocations, just raw delegates
    readonly List<Action> listeners = new List<Action>();

    public void Raise() {
        for (int i = listeners.Count - 1; i >= 0; --i)
            listeners[i]?.Invoke();
    }

    public void RegisterListener(Action callback) {
        if (!listeners.Contains(callback))
            listeners.Add(callback);
    }

    public void UnregisterListener(Action callback) {
        listeners.Remove(callback);
    }
}