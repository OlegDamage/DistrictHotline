using TMPro;
using Unity.Mathematics;
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

    private bool _locked; // защита от двойного клика

    private void Awake()
    {
        // ѕр€м панель на старте
        if (root != null) root.SetActive(false);

        protocolAButton.onClick.AddListener(() => Choose(ProtocolId.Intervene));
        protocolBButton.onClick.AddListener(() => Choose(ProtocolId.Wait));
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
        protocolAButton.interactable = true;
        protocolBButton.interactable = true;
        _locked = false;
        root.SetActive(true);
    }

    private void Hide()
    {
        root.SetActive(false);
    }

    private void Choose(ProtocolId protocol)
    {
        if(_locked) return; _locked = true;

        protocolAButton.interactable = false;
        protocolBButton.interactable = false;

        incidentSystem.ResolveIncident(protocol);
    }
}
