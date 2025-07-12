using UnityEngine;
using System.Collections.Generic;


public class ParticlePool : MonoBehaviour
{
    [SerializeField] ParticleSystem prefab;
    [SerializeField] int            size = 8;

    Queue<ParticleSystem> pool = new();

    void Awake()
    {
        for (int i = 0; i < size; i++)
        {
            var ps = Instantiate(prefab, transform);
            pool.Enqueue(ps);
        }
    }

    public ParticleSystem Play(Vector3 pos, Quaternion rot)
    {
        if (pool.Count == 0) pool.Enqueue(Instantiate(prefab, transform));

        var ps = pool.Dequeue();
        ps.transform.SetPositionAndRotation(pos, rot);
        ps.gameObject.SetActive(true);
        ps.Play(true);
        pool.Enqueue(ps);              // back to queue for next call
        return ps;
    }
}