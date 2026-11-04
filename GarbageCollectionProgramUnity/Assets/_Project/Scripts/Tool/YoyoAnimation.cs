using DG.Tweening;
using UnityEngine;

public class YoyoAnimation : MonoBehaviour
{
    [SerializeField, Min(0f)]
    private float breathingScale = 1.03f;

    [SerializeField, Min(0f)]
    private float duration = 0.8f;

    private Vector3 originalScale;
    private Tween breathingTween;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        breathingTween?.Kill();
        transform.localScale = originalScale;
        breathingTween = transform
            .DOScale(originalScale * breathingScale, duration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true);
    }

    private void OnDisable()
    {
        breathingTween?.Kill();
        breathingTween = null;
        transform.localScale = originalScale;
    }
}
