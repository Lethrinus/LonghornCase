using System.Collections.Generic;
using UnityEngine;

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
    [Tooltip("Eklediğiniz her prefab rastgele seçilebilir.")]
    public PrefabEntry[] prefabs;

    [Header("Random Scale (uniform)")]
    public float minScale = .8f;
    public float maxScale = 1.2f;

    [Header("Random Tilt (deg)")]
    [Tooltip("Örneğin 20 = -20…+20 derece")]
    public float maxRandomTilt = 20f;
    [Tooltip("Hangi eksen etrafında dönsün? (Z, Y, X vs.)")]
    public Vector3 tiltAxis = Vector3.forward;                 // Z ekseni

    
    class Pool
    {
        readonly Queue<ParticleSystem> q = new();
        readonly ParticleSystem prefab;
        readonly Transform parent;

        public Pool(ParticleSystem prefab, int warm, Transform parent)
        {
            this.prefab = prefab;
            this.parent = parent;

            for (int i = 0; i < warm; ++i)
                q.Enqueue(Create());
        }

        ParticleSystem Create()
        {
            var ps   = Object.Instantiate(prefab, parent);
            var main = ps.main;
            main.playOnAwake = false;
            main.stopAction  = ParticleSystemStopAction.Callback;
            ps.gameObject.SetActive(false);

            var hook = ps.gameObject.AddComponent<ReturnHook>();
            hook.Configure(this);
            return ps;
        }

        public ParticleSystem Pop()  => q.Count > 0 ? q.Dequeue() : Create();
        public void Push(ParticleSystem ps) => q.Enqueue(ps);

       
        sealed class ReturnHook : MonoBehaviour
        {
            Pool pool;
            public void Configure(Pool p) => pool = p;
            void OnParticleSystemStopped()
            {
                var ps = GetComponent<ParticleSystem>();
                ps.gameObject.SetActive(false);
                pool.Push(ps);
            }
        }
    }

    
    readonly List<Pool> pools = new();
    readonly System.Random rng = new();

    void Awake()
    {
        foreach (var e in prefabs)
            if (e.prefab != null)
                pools.Add(new Pool(e.prefab, e.prewarmCount, transform));
    }

   
    public void Play(Vector3 pos, Quaternion baseRot)
    {
        if (pools.Count == 0) return;

        
        var pool = pools[rng.Next(pools.Count)];
        var ps   = pool.Pop();
        ps.Clear(true);
        
        ps.transform.SetPositionAndRotation(pos, baseRot);
        
        float tilt = Random.Range(-maxRandomTilt, maxRandomTilt);
        ps.transform.rotation =
            Quaternion.AngleAxis(tilt, ps.transform.forward) * ps.transform.rotation;
        
        float s = Random.Range(minScale, maxScale);
        ps.transform.localScale = Vector3.one * s;
        
        var main = ps.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        ps.gameObject.layer  = 0;                 

        ps.gameObject.SetActive(true);
        ps.Play(true);
    }
}
