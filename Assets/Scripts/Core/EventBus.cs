using System;
using System.Collections.Generic;

namespace Core {
    public static class EventBus {
        static readonly Dictionary<Type, List<Delegate>> _subs = new();
        public static void Subscribe<T>(Action<T> cb) {
            var t = typeof(T);
            if (!_subs.ContainsKey(t)) _subs[t] = new List<Delegate>();
            _subs[t].Add(cb);
        }
        public static void Unsubscribe<T>(Action<T> cb) {
            var t = typeof(T);
            if (_subs.TryGetValue(t, out var L)) L.Remove(cb);
        }
        public static void Publish<T>(T e) {
            var t = typeof(T);
            if (!_subs.TryGetValue(t, out var L)) return;
            foreach (var d in L) (d as Action<T>)?.Invoke(e);
        }
    }
}