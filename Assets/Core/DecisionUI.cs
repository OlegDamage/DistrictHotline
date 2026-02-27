using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DecisionUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject root;
    [SerializeField] private TextMeshProUGUI idText;
    [SerializeField] private TextMeshProUGUI severityText;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button protocolAButton;
    [SerializeField] private Button protocolBButton;

    [Header("Refs")]
    [SerializeField] private IncidentSystem incidentSystem;

    private void Awake()
    {
        // Прям панель на старте
        if (root != null) root.SetActive(false);

        protocolAButton.onClick.AddListener(() => Resolve(true));
        protocolBButton.onClick.AddListener(() => Resolve(false));
    }

    private void OnEnable()
    {
        incidentSystem.OnIncidentRaised += ShowIncident;
        incidentSystem.OnIncidentCleared += Hide;
    }

    private void OnDisable()
    {
        incidentSystem.OnIncidentRaised -= ShowIncident;
        incidentSystem.OnIncidentCleared -= Hide;
    }

    private void ShowIncident(Incident inc)
    {
        idText.text = inc.Id;
        severityText.text = inc.BaseSeverity.ToString();
        titleText.text = inc.Title;
        root.SetActive(true);
    }

    private void Hide()
    {
        root.SetActive(false);
    }

    private void Resolve(bool success)
    {
        incidentSystem.ResolveIncident(success);
        // Hide() вызовется через событие OnIcidentCleared
    }
}
