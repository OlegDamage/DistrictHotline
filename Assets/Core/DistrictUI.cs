using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DistrictUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private IncidentSystem incidentSystem;

    [Header("UI")]
    [SerializeField] private Image dangerIcon; // обозначение инцидента
    [SerializeField] private Image signalDot; // обозначение инцидента
    [SerializeField] private TextMeshProUGUI statusLabel; // текст сообщения
    [SerializeField] private RectTransform districtRoot;// контейнер правой панели
    [SerializeField] CanvasGroup flashGroup; // полупрозрачный img поверх панели

    [Header("Pulse")]
    [SerializeField, Range(0.1f, 3f)] private float pulseSpeed = 2f;
    [SerializeField, Range(0f, 1f)] private float minAlpha = 0.25f;

    [Header("Signal FX")]
    //[SerializeField, Range(0.02f, 0.3f)] private float flashDuration = 0.08f;
    [SerializeField, Range(0.5f, 1f)] private float flashAlpha = 0.6f;
    [SerializeField, Range(0.02f, 0.2f)] private float glitchDuration = 0.08f;
    [SerializeField, Range(1f, 10f)] private float glitchStrength = 4f;

    [Header("Critical")]
    [SerializeField, Range(0.1f, 5f)] private float criticalPulseSpeed = 3.5f;
    [SerializeField, Range(0f, 1f)] private float criticalMinAlpha = 0.45f;
    [SerializeField] private Color normalAlertColor = Color.white;
    [SerializeField] private Color criticalAlertColor = new Color(1f, 0.35f, 0.35f, 1f);
    [SerializeField] private Image criticalTint;
    [SerializeField, Range(0f, 1f)] private float criticalTintAlpha = 0.18f;

    [SerializeField, Range(0.5f, 2f)] private float criticalFlashMultiplier = 1.25f;
    [SerializeField, Range(1f, 3f)] private float criticalGlitchMultiplier = 1.5f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip pingClip;

    private Coroutine pulseRoutine;
    private Coroutine signalFxRoutine;
    private DistrictState currentState = DistrictState.Normal;

    private void Awake()
    {
        ApplyNormalState();
    }

    private void OnEnable()
    {
        if(incidentSystem == null)
        {
            Debug.LogError("[DistrictUI] IncidentSystem is not assigned.");
            return;
        }

        incidentSystem.OnIncidentRaised += HandleIncidentRaised;
        incidentSystem.OnIncidentCleared += HandleIncidentCleared;
    }

    private void OnDisable()
    {
        if (incidentSystem == null) return;

        incidentSystem.OnIncidentRaised -= HandleIncidentRaised;
        incidentSystem.OnIncidentCleared -= HandleIncidentCleared;
    }

    private void HandleIncidentRaised(Incident incident)
    {
        PlaySignalFx(false);
    }

    private void HandleIncidentCleared()
    {
        // ничего
    }

    public void SetDistrictState(DistrictState state)
    {
        if (currentState == state) return;
        currentState = state;

        StopPulse();

        switch (state)
        {
            case DistrictState.Normal:
                ApplyNormalState();
                break;

            case DistrictState.Incident:
                ApplyIncidentState();
                StartPulse(pulseSpeed, minAlpha);
                break;

            case DistrictState.Critical:
                ApplyCriticalState();
                StartPulse(criticalPulseSpeed, criticalMinAlpha);
                PlaySignalFx(true);
                break;
        }
    }

    private void ApplyNormalState()
    {
        if (dangerIcon)
        {
            dangerIcon.gameObject.SetActive(false);
            dangerIcon.color = normalAlertColor;
            SetAlpha(dangerIcon, 1f);
        }

        if (signalDot)
        {
            signalDot.gameObject.SetActive(false);
            signalDot.color = normalAlertColor;
            SetAlpha(signalDot, 1f);
        }

        if (criticalTint)
        {
            criticalTint.gameObject.SetActive(false);
            SetAlpha(criticalTint, 0f);
        }

        if (statusLabel)
        {
            statusLabel.gameObject.SetActive(true);
            statusLabel.text = "СИСТЕМА СТАБИЛЬНА";
        }
    }

    private void ApplyIncidentState()
    {
        if (dangerIcon)
        {
            dangerIcon.gameObject.SetActive(true);
            dangerIcon.color = normalAlertColor;
            SetAlpha(dangerIcon, 1f);
        }

        if (signalDot)
        {
            signalDot.gameObject.SetActive(true);
            signalDot.color = normalAlertColor;
            SetAlpha(signalDot, 1f);
        }

        if (criticalTint)
        {
            criticalTint.gameObject.SetActive(false);
            SetAlpha(criticalTint, 0f);
        }

        if (statusLabel)
        {
            statusLabel.gameObject.SetActive(true);
            statusLabel.text = "СИГНАЛ ПОЛУЧЕН";
        }
    }

    private void ApplyCriticalState()
    {
        if (dangerIcon)
        {
            dangerIcon.gameObject.SetActive(true);
            dangerIcon.color = criticalAlertColor;
            SetAlpha(dangerIcon, 1f);
        }

        if (signalDot)
        {
            signalDot.gameObject.SetActive(true);
            signalDot.color = criticalAlertColor;
            SetAlpha(signalDot, 1f);
        }

        if (criticalTint)
        {
            criticalTint.gameObject.SetActive(true);
            criticalTint.color = new Color(
                criticalTint.color.r,
                criticalTint.color.g,
                criticalTint.color.b,
                1f
            );
            SetAlpha(criticalTint, criticalTintAlpha);
        }

        if (statusLabel)
        {
            statusLabel.gameObject.SetActive(true);
            statusLabel.text = "КРИТИЧЕСКИЙ УРОВЕНЬ";
        }
    }

    private void StartPulse(float speed, float minPulseAlpha)
    {
        if (pulseRoutine != null) StopCoroutine(pulseRoutine);
        pulseRoutine = StartCoroutine(PulseLoop(speed, minPulseAlpha));
    }

    private void StopPulse()
    {
        if (pulseRoutine == null) return;
        StopCoroutine(pulseRoutine);
        pulseRoutine = null;
    }

    private IEnumerator PulseLoop(float speed, float minPulseAlpha)
    {
        float t = 0f;

        while (true)
        {
            t += Time.deltaTime * speed;
            float a = Mathf.Lerp(minPulseAlpha, 1f, (Mathf.Sin(t) + 1f) * 0.5f);

            if (dangerIcon) SetAlpha(dangerIcon, a);
            if (signalDot) SetAlpha(signalDot, a);

            yield return null;
        }
    }

    private static void SetAlpha(Graphic g, float a)
    {
        var c = g.color;
        c.a = a;
        g.color = c;
    }

    private void PlaySignalFx(bool isCritical = false)
    {
        if (signalFxRoutine != null) StopCoroutine(signalFxRoutine);
        signalFxRoutine = StartCoroutine(SignalFxRoutine(isCritical));
    }

    private IEnumerator SignalFxRoutine(bool isCritical)
    {
        if (audioSource && pingClip) audioSource.PlayOneShot(pingClip);

        Vector2 originalPos = districtRoot ? districtRoot.anchoredPosition : Vector2.zero;

        float fxGlitchDuration = isCritical ? glitchDuration * criticalGlitchMultiplier : glitchDuration;
        float fxGlitchStrength = isCritical ? glitchStrength * criticalGlitchMultiplier : glitchStrength;
        float fxFlashAlpha = isCritical ? flashAlpha * criticalFlashMultiplier : flashAlpha;

        float t = 0f;
        while (t < fxGlitchDuration)
        {
            t += Time.deltaTime;

            if (districtRoot)
            {
                float x = Random.Range(-fxGlitchStrength, fxGlitchStrength);
                float y = Random.Range(-fxGlitchStrength, fxGlitchStrength);
                districtRoot.anchoredPosition = originalPos + new Vector2(x, y);
            }

            if (flashGroup)
            {
                float k = t / fxGlitchDuration;
                float tri = 1f - Mathf.Abs(2f * k - 1f);
                flashGroup.alpha = tri * fxFlashAlpha;
            }

            yield return null;
        }

        if (districtRoot) districtRoot.anchoredPosition = originalPos;
        if (flashGroup) flashGroup.alpha = 0f;

        signalFxRoutine = null;
    }
}
