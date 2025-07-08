using UnityEngine;
using TMPro;
using DG.Tweening;

public class TextRevealController : MonoBehaviour
{
    [SerializeField] private TextMeshPro text;
    [SerializeField] private float revealDuration = 1f;
    [SerializeField] private Ease  revealEase     = Ease.Linear;

    void Awake()
    {
        var c = text.color;
        text.color = new Color(c.r, c.g, c.b, 0f);
    }

    public void Reveal(string msg)
    {
        text.text = msg;
        text.DOFade(1f, revealDuration).SetEase(revealEase);
    }
}