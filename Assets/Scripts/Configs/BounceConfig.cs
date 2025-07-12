// Assets/Scripts/Config/BounceConfig.cs
using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "BounceConfig",
    menuName = "Assets/Configs/Bounce Config")]
public class BounceConfig : ScriptableObject
{
    [Header("Jump")]
    public float jumpPower     = 0.08f;
    public float jumpDuration  = 0.35f;
    public Ease  jumpEase      = Ease.OutCubic;
    
    
    
    [Header("Sound")]
    public AudioClip clip;
    [Range(0f,1f)] public float volume = 1f;
    public float pitch = 1f;
    
    
    [Header("Position Shake")]
    public Vector3 posShake      = new(.01f, .01f, .01f);
    public int     posVibrato    = 10;
    public float   posRandomness = 60f;

    [Header("Rotation Shake")]
    public Vector3 rotShake      = new(4f, 2f, 4f);
    public int     rotVibrato    = 12;
    public float   rotRandomness = 45f;

    public Ease commonEase = Ease.InOutSine;
}