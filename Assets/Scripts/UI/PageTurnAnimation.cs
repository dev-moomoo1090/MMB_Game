using System;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PageTurnAnimation : MonoBehaviour
{
    /// <summary>현재 페이지 나가기.</summary>
    public void AnimateOut(bool turnRight, Action onComplete = null)
    {
        gameObject.SetActive(false);
        onComplete?.Invoke();
    }

    /// <summary>새 페이지 들어오기.</summary>
    public void AnimateIn(bool turnRight, Action onComplete = null)
    {
        gameObject.SetActive(true);
        onComplete?.Invoke();
    }
}
