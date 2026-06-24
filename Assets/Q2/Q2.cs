using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
录屏路径：`Assets/Q2/CandyCrush.MP4`

请仔细观察根目录中提供的知名消除游戏 Candy Crush 录屏中，选关界面对话框 Play 按钮的动画效果，请复刻这一效果，使用代码实现或者 Animation 均可，动画包括：
1. 按钮出现
2. 按钮按下
3. 按钮弹起
*/

public class Q2 : MonoBehaviour
{
    [SerializeField]
    private Button button = null;

    [SerializeField]
    private AnimationCurve bounceCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.2f, 1.2f),
        new Keyframe(0.4f, 0.8f),
        new Keyframe(0.6f, 1.1f),
        new Keyframe(0.8f, 0.95f),
        new Keyframe(1f, 1f)
    );

    [SerializeField]
    private AnimationCurve jellyCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.15f, 0.3f),
        new Keyframe(0.3f, -0.2f),
        new Keyframe(0.45f, 0.15f),
        new Keyframe(0.6f, -0.1f),
        new Keyframe(0.75f, 0.05f),
        new Keyframe(1f, 0f)
    );

    private Coroutine _currentAnimation;
    private Vector3 _originalScale;
    private Vector3 _originalPosition;

    private void Awake()
    {
        _originalScale = button.transform.localScale;
        _originalPosition = button.transform.localPosition;
    }

    public void OnShowBtnClick()
    {
        if (_currentAnimation != null)
            StopCoroutine(_currentAnimation);
        _currentAnimation = StartCoroutine(ShowAnimation());
    }

    private IEnumerator ShowAnimation()
    {
        float duration = 0.6f;
        float elapsed = 0f;

        // 初始状态
        button.transform.localScale = Vector3.zero;
        button.transform.localRotation = Quaternion.identity;
        button.gameObject.SetActive(true);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // 果冻弹动效果 - scale
            float bounceValue = bounceCurve.Evaluate(t);
            float jellyValue = jellyCurve.Evaluate(t);

            // X方向拉伸，Y方向压缩（果冻效果）
            float scaleX = _originalScale.x * bounceValue * (1f + jellyValue * 0.3f);
            float scaleY = _originalScale.y * bounceValue * (1f - jellyValue * 0.2f);
            button.transform.localScale = new Vector3(scaleX, scaleY, _originalScale.z);

            // 左右两侧上下跳动效果 - 通过旋转实现
            float wobbleAngle = Mathf.Sin(t * Mathf.PI * 6) * (1f - t) * 15f;
            button.transform.localRotation = Quaternion.Euler(0f, 0f, wobbleAngle);

            yield return null;
        }

        // 恢复原始状态
        button.transform.localScale = _originalScale;
        button.transform.localRotation = Quaternion.identity;
        _currentAnimation = null;
    }

    public void OnTouchDownBtnClick()
    {
        if (_currentAnimation != null)
            StopCoroutine(_currentAnimation);
        _currentAnimation = StartCoroutine(TouchDownAnimation());
    }

    private IEnumerator TouchDownAnimation()
    {
        float duration = 0.2f;
        float elapsed = 0f;
        Vector3 targetScale = _originalScale * 0.75f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // 缩小进度，带有果冻弹跳效果
            // 先快速缩小，然后稍微弹回，再稳定到目标尺寸
            float bounceT = Mathf.Sin(t * Mathf.PI * 1.5f) * 0.15f * (1f - t);
            float scaleProgress = Mathf.SmoothStep(0f, 1f, t) + bounceT;
            scaleProgress = Mathf.Clamp01(scaleProgress);

            // 果冻效果 - X/Y差异化拉伸，增强幅度
            float jellyValue = Mathf.Sin(t * Mathf.PI * 5) * (1f - t) * 0.15f;

            float baseScaleX = Mathf.Lerp(_originalScale.x, targetScale.x, scaleProgress);
            float baseScaleY = Mathf.Lerp(_originalScale.y, targetScale.y, scaleProgress);
            float scaleX = baseScaleX * (1f + jellyValue);
            float scaleY = baseScaleY * (1f - jellyValue);
            button.transform.localScale = new Vector3(scaleX, scaleY, _originalScale.z);

            yield return null;
        }

        // 确保到达目标尺寸，无果冻效果
        button.transform.localScale = targetScale;
        _currentAnimation = null;
    }

    public void OnTouchUpBtnClick()
    {
        if (_currentAnimation != null)
            StopCoroutine(_currentAnimation);
        _currentAnimation = StartCoroutine(TouchUpAnimation());
    }

    private IEnumerator TouchUpAnimation()
    {
        float duration = 0.25f;
        float elapsed = 0f;
        Vector3 startScale = _originalScale * 0.85f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // 放大进度，带有果冻弹跳效果
            // 先快速放大，然后稍微超出，再回弹到原始尺寸
            float bounceT = Mathf.Sin(t * Mathf.PI * 1.5f) * 0.15f * (1f - t);
            float scaleProgress = Mathf.SmoothStep(0f, 1f, t) + bounceT;
            scaleProgress = Mathf.Clamp01(scaleProgress);

            // 果冻效果 - X/Y差异化拉伸，增强幅度
            float jellyValue = Mathf.Sin(t * Mathf.PI * 5) * (1f - t) * 0.15f;

            float baseScaleX = Mathf.Lerp(startScale.x, _originalScale.x, scaleProgress);
            float baseScaleY = Mathf.Lerp(startScale.y, _originalScale.y, scaleProgress);
            float scaleX = baseScaleX * (1f + jellyValue);
            float scaleY = baseScaleY * (1f - jellyValue);
            button.transform.localScale = new Vector3(scaleX, scaleY, _originalScale.z);

            yield return null;
        }

        // 确保恢复到原始尺寸，无果冻效果
        button.transform.localScale = _originalScale;
        _currentAnimation = null;
    }
}
