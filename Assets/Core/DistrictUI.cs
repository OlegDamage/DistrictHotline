using TMPro;
using UnityEngine;

public class DistrictUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private IncidentSystem incidentSystem;

    [Header("UI")]
    [SerializeField] private GameObject incidentMarker; // обозначение инцидента
    [SerializeField] private GameObject messgaeRoot; // объект сообщения
    [SerializeField] private TextMeshProUGUI messageText; // текст сообщения

    [Header("Config")]
    [SerializeField] private string districtName = "District-01";
    private void OnEnable()
    {
        if(incidentSystem == null)
        {
            Debug.LogError("[DistrictUI] IncidentSystem is not assigned.");
            return;
        }

        incidentSystem.OnIncidentRaised += OnIncidentRaised;
        incidentSystem.OnIncidentCleared += OnIncidentCleared;

        ApplyState(incidentSystem.CurrentIncident);
    }

    private void OnDisable()
    {
        if (incidentSystem == null) return;

        incidentSystem.OnIncidentRaised -= OnIncidentRaised;
        incidentSystem.OnIncidentCleared -= OnIncidentCleared;
    }

    private void OnIncidentRaised(Incident inc)
    {
        ApplyState(inc);
    }

    private void OnIncidentCleared()
    {
        ApplyState(null);
    }

    private void ApplyState(Incident inc)
    {
        bool active = inc != null;

        if(incidentMarker != null) incidentMarker.SetActive(active);
        if(messgaeRoot != null) messgaeRoot.SetActive(active);

        if(messageText != null)
        {
            if (active) messageText.text = $"АКТИВНО: {inc.Id} ({districtName})";
            else messageText.text = "";
        }
    }
}
