using System.Collections.Generic;
using UnityEngine;

public class ParticlePool : MonoBehaviour
{
    [SerializeField] ParticleSystem prefab;
    [SerializeField] int size = 6;

    readonly Queue<ParticleSystem> q = new();

    void Awake() {
        for (int i = 0; i < size; i++)
            q.Enqueue(Instantiate(prefab, transform));
    }

    public ParticleSystem Play(Vector3 pos, Quaternion rot)
    {
        var ps = q.Dequeue();
        ps.transform.SetPositionAndRotation(pos, rot);
        ps.gameObject.SetActive(true);
        ps.Play(true);
        q.Enqueue(ps);
        return ps;
    }
}