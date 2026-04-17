using System.Collections;
using UnityEngine;

public class CombinedSliderAnimator : MonoBehaviour
{
    private RectTransform _rect;
    private CanvasGroup _canvasGroup;
    private Coroutine _currentAnimation;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void SetVisible(bool visible)
    {
        _canvasGroup.alpha = visible ? 1f : 0f;
    }

    public void AnimateTo(Vector2 targetPosition, Vector2 targetScale, float duration)
    {
        if (_currentAnimation != null)
            StopCoroutine(_currentAnimation);
        _currentAnimation = StartCoroutine(AnimateCoroutine(targetPosition, targetScale, duration));
    }

    public void SnapTo(Vector2 position, Vector2 scale)
    {
        if (_currentAnimation != null)
            StopCoroutine(_currentAnimation);
        _rect.anchoredPosition = position;
        _rect.localScale = new Vector3(scale.x, scale.y, 1f);
    }

    private IEnumerator AnimateCoroutine(Vector2 targetPosition, Vector2 targetScale, float duration)
    {
        Vector2 startPosition = _rect.anchoredPosition;
        Vector2 startScale = _rect.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            _rect.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
            _rect.localScale = new Vector3(
                Mathf.Lerp(startScale.x, targetScale.x, t),
                Mathf.Lerp(startScale.y, targetScale.y, t),
                1f);
            yield return null;
        }

        // Snap exactly to target to avoid float drift
        _rect.anchoredPosition = targetPosition;
        _rect.localScale = new Vector3(targetScale.x, targetScale.y, 1f);
        _currentAnimation = null;
    }
}