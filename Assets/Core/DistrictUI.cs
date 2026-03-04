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

    [Header("Pulse")]
    [SerializeField, Range(0.1f, 3f)] private float pulseSpeed = 2f;
    [SerializeField, Range(0f, 1f)] private float minAlpha = 0.25f;

    private Coroutine pulseRoutine;
    private DistrictState currentState = DistrictState.Normal;

    private void Awake()
    {
        ApplyStateInstant(DistrictState.Normal);
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
        SetDistrictState(DistrictState.Incident);
    }
    private void HandleIncidentCleared()
    {
        SetDistrictState(DistrictState.Normal);
    }

    public void SetDistrictState(DistrictState state)
    {
        if(currentState == state) return;
        currentState = state;

        StopPulse();

        switch (state)
        {
            case DistrictState.Normal:
                ApplyStateInstant(DistrictState.Normal);
                break;

            case DistrictState.Incident:
                ApplyStateInstant(DistrictState.Incident);
                StartPulse();
                break;

            case DistrictState.Critical:
                ApplyStateInstant(DistrictState.Critical);
                StartPulse();// позже надо усилить
                break;
        }
    }

    private void ApplyStateInstant(DistrictState state)
    {
        bool alert = state != DistrictState.Normal;

        if (dangerIcon) dangerIcon.gameObject.SetActive(alert);
        if (signalDot) signalDot.gameObject.SetActive(alert);

        if(statusLabel)
        {
            statusLabel.gameObject.SetActive(true);
            statusLabel.text = state switch
            {
                DistrictState.Normal => "СИСТЕМА СТАБИЛЬНА",
                DistrictState.Incident => "СИГНАЛ ПОЛУЧЕН",
                DistrictState.Critical => "КРИТИЧЕСКИЙ УРОВЕНЬ",
                _ => ""
            };
        }

        // Сброс альфы, чтобы после пульса всё было предсказуемо
        if (dangerIcon) SetAlpha(dangerIcon, 1f);
        if (signalDot) SetAlpha(signalDot, 1f);
    }

    private void StartPulse()
    {
        if(pulseRoutine != null) StopCoroutine(pulseRoutine);
        pulseRoutine = StartCoroutine(PulseLoop());
    }

    private void StopPulse()
    {
        if (pulseRoutine == null) return;
        StopCoroutine(pulseRoutine);
        pulseRoutine = null;
    }

    private IEnumerator PulseLoop()
    {
        float t = 0f;

        while (true)
        {
            t += Time.deltaTime * pulseSpeed;
            float a = Mathf.Lerp(minAlpha, 1f, (Mathf.Sin(t) + 1f) * 0.5f);

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
}
