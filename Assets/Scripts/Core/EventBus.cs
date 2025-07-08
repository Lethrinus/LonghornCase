using System;
using System.Collections.Generic;

namespace Core
{
    // publish subscribe
    public static class EventBus
    {
        static readonly Dictionary<Type, List<Delegate>> _subs = new();

        public static void Subscribe<T>(Action<T> callback)
        {
            var t = typeof(T);
            if (!_subs.ContainsKey(t)) _subs[t] = new List<Delegate>();
            _subs[t].Add(callback);
        }

        public static void Unsubscribe<T>(Action<T> callback)
        {
            var t = typeof(T);
            if (_subs.TryGetValue(t, out var list)) list.Remove(callback);
        }

        public static void Publish<T>(T evt)
        {
            var t = typeof(T);
            if (!_subs.TryGetValue(t, out var list)) return;
            foreach (var d in list) ((Action<T>)d)?.Invoke(evt);
        }
    }
}