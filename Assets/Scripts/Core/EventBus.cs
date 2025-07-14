using System;
using System.Collections.Generic;

namespace Core {
    public static class EventBus {
        private static readonly Dictionary<Type, List<Delegate>> Subs = new();

        public static void Subscribe<T>(Action<T> cb) {
            var t = typeof(T);
            if (!Subs.ContainsKey(t)) Subs[t] = new List<Delegate>();
            Subs[t].Add(cb);
        }

        public static void Unsubscribe<T>(Action<T> cb) {
            var t = typeof(T);
            if (Subs.TryGetValue(t, out var l)) l.Remove(cb);
        }

        public static void Publish<T>(T e) {
            var t = typeof(T);
            if (!Subs.TryGetValue(t, out var l)) return;
            for (var i = 0; i < l.Count; i++) {
                ((Action<T>)l[i])(e);
            }
        }
    }
}