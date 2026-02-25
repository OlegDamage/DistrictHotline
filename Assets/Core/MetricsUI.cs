using TMPro;
using UnityEngine;

public class MetricsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI trustText;
    [SerializeField] private TextMeshProUGUI controlText;
    [SerializeField] private TextMeshProUGUI loadText;

    private CityMetrics metrics;

    private void Start()
    {
        metrics = FindAnyObjectByType<CityMetrics>();

        if(metrics != null)
        {
            metrics.OnMetricsChanged += UpdateUI;
            UpdateUI(metrics.Trust, metrics.Control, metrics.Load);
        }
    }

    private void OnDestroy()
    {
        if (metrics != null)
            metrics.OnMetricsChanged -= UpdateUI;
    }

    private void UpdateUI(int trust, int control, int load)
    {
        trustText.text = trust.ToString();
        controlText.text = control.ToString();
        loadText.text = load.ToString();
    }
}
