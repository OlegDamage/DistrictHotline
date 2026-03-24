using UnityEngine;

public class DistrictStateController : MonoBehaviour
{
    [SerializeField] private CityMetrics metrics;
    [SerializeField] private IncidentSystem incidentSystem;
    [SerializeField] private DistrictUI districtUI;

    [Header("Thresholds")]
    [SerializeField] private int criticalLoadThreshold = 80;

    private bool hasActiveIncident;

    private void OnEnable()
    {
        incidentSystem.OnIncidentRaised += HandleIncidentRaised;
        incidentSystem.OnIncidentCleared += HandleIncidentCleared;
        metrics.OnMetricsChanged += HandleMetricsChanged;
    }

    private void OnDisable()
    {
        incidentSystem.OnIncidentRaised -= HandleIncidentRaised;
        incidentSystem.OnIncidentCleared -= HandleIncidentCleared;
        metrics.OnMetricsChanged -= HandleMetricsChanged;
    }

    private void HandleIncidentRaised(Incident incident)
    {
        hasActiveIncident = true;
        UpdateState();
    }

    private void HandleIncidentCleared()
    {
        hasActiveIncident = false;
        UpdateState();
    }

    private void HandleMetricsChanged()
    {
        UpdateState();
    }

    private void UpdateState()
    {
        if (metrics.Load >= criticalLoadThreshold)
        {
            districtUI.SetDistrictState(DistrictState.Critical);
        }
        else if (hasActiveIncident)
        {
            districtUI.SetDistrictState(DistrictState.Incident);
        }
        else
        {
            districtUI.SetDistrictState(DistrictState.Normal);
        }
    }
}
