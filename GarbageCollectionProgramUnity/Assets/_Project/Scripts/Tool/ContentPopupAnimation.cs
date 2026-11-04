using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class ContentPopupAnimation : MonoBehaviour
{
    [SerializeField, Min(0f)]
    private float duration = 0.35f;

    [SerializeField, Min(0f)]
    private float startScale = 0.75f;

    [SerializeField, Min(0f)]
    private float endScale = 1f;

    private RectTransform content;
    private Tween popupTween;

    private void Awake()
    {
        content = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        if (content == null)
        {
            content = GetComponent<RectTransform>();
        }

        if (content == null)
        {
            CommonTool.LogError(
                "[ContentPopupAnimation] RectTransform not found.",
                this
            );
            return;
        }

        popupTween?.Kill();
        content.localScale = Vector3.one * startScale;
        popupTween = content
            .DOScale(Vector3.one * endScale, duration)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
    }

    private void OnDisable()
    {
        popupTween?.Kill();
        popupTween = null;

        if (content != null)
        {
            content.localScale = Vector3.one * endScale;
        }
    }
}
