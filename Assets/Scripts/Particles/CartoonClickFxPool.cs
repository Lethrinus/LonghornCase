using System.Collections.Generic;
using UnityEngine;

namespace Particles
{
    [DisallowMultipleComponent]
    public class CartoonClickFxPool : MonoBehaviour
    {
        [System.Serializable]
        public struct PrefabEntry
        {
            public ParticleSystem prefab;
            public int prewarmCount;
        }

        [Header("Prefabs & Pre-warm")]
        [Tooltip("Random Prefab")]
        public PrefabEntry[] prefabs;

        [Header("Random Scale (uniform)")]
        public float minScale = .8f;
        public float maxScale = 1.2f;

        class Pool
        {
            private readonly Queue<ParticleSystem> _q = new();
            private readonly ParticleSystem _prefab;
            private readonly Transform _parent;

            public Pool(ParticleSystem prefab, int warm, Transform parent)
            {
                this._prefab = prefab;
                this._parent = parent;

                for (int i = 0; i < warm; ++i)
                    _q.Enqueue(Create());
            }

            private ParticleSystem Create()
            {
                var ps = Object.Instantiate(_prefab, _parent);
                var main = ps.main;
                main.playOnAwake = false;
                main.stopAction = ParticleSystemStopAction.Callback;
                ps.gameObject.SetActive(false);

                var hook = ps.gameObject.AddComponent<ReturnHook>();
                hook.Configure(this);
                return ps;
            }

            public ParticleSystem Pop() => _q.Count > 0 ? _q.Dequeue() : Create();
            private void Push(ParticleSystem ps) => _q.Enqueue(ps);

            private sealed class ReturnHook : MonoBehaviour
            {
                private Pool _pool;
                public void Configure(Pool p) => _pool = p;

                private void OnParticleSystemStopped()
                {
                    var ps = GetComponent<ParticleSystem>();
                    ps.gameObject.SetActive(false);
                    _pool.Push(ps);
                }
            }
        }

        private readonly List<Pool> _pools = new();
        private readonly System.Random _rng = new();

        private void Awake()
        {
            foreach (var e in prefabs)
                if (e.prefab != null)
                    _pools.Add(new Pool(e.prefab, e.prewarmCount, transform));
        }

        public void Play(Vector3 pos, Quaternion baseRot)
        {
            if (_pools.Count == 0) return;

            var pool = _pools.Count == 1 ? _pools[0] : _pools[_rng.Next(_pools.Count)];
            var ps = pool.Pop();
            ps.Clear(true);

            ps.transform.SetPositionAndRotation(pos, baseRot);

            
            if (minScale != maxScale)
            {
                var s = Random.Range(minScale, maxScale);
                ps.transform.localScale = Vector3.one * s;
            }

            var main = ps.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            ps.gameObject.layer = 0;

            ps.gameObject.SetActive(true);
            ps.Play(true);
        }
    }
}