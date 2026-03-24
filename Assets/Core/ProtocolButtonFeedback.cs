using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ProtocolButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Refs")]
    [SerializeField] private RectTransform target;
    [SerializeField] private Graphic targetGraphic;

    [Header("Scale")]
    [SerializeField, Range(0.9f, 1.2f)] private float normalScale = 1f;
    [SerializeField, Range(0.9f, 1.2f)] private float hoverScale = 1.04f;
    [SerializeField, Range(0.8f, 1.1f)] private float pressedScale = 0.97f;

    [Header("Color")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private Color disabledColor = new Color(0.7f, 0.7f, 0.7f, 0.8f);

    [Header("Timing")]
    [SerializeField, Range(0.02f, 0.3f)] private float animDuration = 0.08f;

    [Header("Confirm")]
    [SerializeField, Range(1f, 1.2f)] private float confirmScale = 1.08f;
    [SerializeField] private Color confirmColor = Color.white;
    [SerializeField, Range(0.02f, 0.3f)] private float confirmDuration = 0.12f;

    [Header("Rejected")]
    [SerializeField, Range(0.8f, 1.05f)] private float rejectedScale = 0.98f;
    [SerializeField] private Color rejectedColor = new Color(0.6f, 0.6f, 0.6f, 0.85f);

    private Coroutine scaleRoutine;
    private bool isHovered;
    private bool isPressed;
    private bool isLocked;
    private bool isConfirmed;
    private bool isRejected;

    private void Awake()
    {
        if (target == null)
            target = transform as RectTransform;

        isLocked = false;
        isConfirmed = false;
        isRejected = false;

        ApplyImmediate(normalScale, normalColor);
    }

    public void SetLocked(bool locked)
    {
        isLocked = locked;

        if (isLocked)
        {
            StopCurrentAnimation();
            return;
        }

        isConfirmed = false;
        isRejected = false;
        isHovered = false;
        isPressed = false;

        ApplyImmediate(normalScale, normalColor);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isLocked) return;
        isHovered = true;
        ApplyVisualState();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isLocked) return;
        isHovered = false;
        isPressed = false;
        ApplyVisualState();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isLocked) return;
        isPressed = true;
        ApplyVisualState();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isLocked) return;
        isPressed = false;
        ApplyVisualState();
    }

    private void ApplyVisualState()
    {
        if (isConfirmed)
        {
            StartScaleAnimation(confirmScale);

            if (targetGraphic != null)
                targetGraphic.color = confirmColor;

            return;
        }

        if (isRejected)
        {
            StartScaleAnimation(rejectedScale);

            if (targetGraphic != null)
                targetGraphic.color = rejectedColor;

            return;
        }

        float targetScale = normalScale;
        Color targetColor = normalColor;

        if (isPressed)
        {
            targetScale = pressedScale;
            targetColor = hoverColor;
        }
        else if (isHovered)
        {
            targetScale = hoverScale;
            targetColor = hoverColor;
        }

        StartScaleAnimation(targetScale);

        if (targetGraphic != null)
            targetGraphic.color = targetColor;
    }

    private void StartScaleAnimation(float targetScale)
    {
        StopCurrentAnimation();

        if (!gameObject.activeInHierarchy)
        {
            if (target != null)
                target.localScale = new Vector3(targetScale, targetScale, targetScale);
            return;
        }

        scaleRoutine = StartCoroutine(ScaleRoutine(targetScale));
    }

    private IEnumerator ScaleRoutine(float targetScale)
    {
        Vector3 start = target.localScale;
        Vector3 end = new Vector3(targetScale, targetScale, targetScale);

        float elapsed = 0f;
        while (elapsed < animDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animDuration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            target.localScale = Vector3.Lerp(start, end, eased);
            yield return null;
        }

        target.localScale = end;
        scaleRoutine = null;
    }

    private void ApplyImmediate(float scale, Color color)
    {
        if (target != null)
            target.localScale = new Vector3(scale, scale, scale);

        if (targetGraphic != null)
            targetGraphic.color = color;
    }

    private void StopCurrentAnimation()
    {
        if (scaleRoutine != null)
        {
            StopCoroutine(scaleRoutine);
            scaleRoutine = null;
        }
    }

    public void PlayConfirm()
    {
        if (isLocked && isConfirmed) return;

        isLocked = true;
        isConfirmed = true;
        isRejected = false;

        StopCurrentAnimation();
        StartScaleAnimation(confirmScale);

        if (targetGraphic != null)
            targetGraphic.color = confirmColor;
    }

    public void PlayRejected()
    {
        if (isLocked && isRejected) return;

        isLocked = true;
        isRejected = true;
        isConfirmed = false;

        StopCurrentAnimation();
        StartScaleAnimation(rejectedScale);

        if (targetGraphic != null)
            targetGraphic.color = rejectedColor;
    }
}
