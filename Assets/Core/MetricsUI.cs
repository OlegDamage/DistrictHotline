using System.Collections;
using TMPro;
using UnityEngine;

public class MetricsUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI trustText;
    [SerializeField] private TextMeshProUGUI controlText;
    [SerializeField] private TextMeshProUGUI loadText;
    [SerializeField] private CityMetrics cityMetrics;

    [Header("Animation")]
    [SerializeField, Range(0.05f, 1f)] private float valueAnimDuration = 0.25f;
    [SerializeField, Range(0.05f, 1f)] private float flashDuration = 0.2f;

    [Header("Colors")]
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color positiveColor = new Color(0.6f, 1f, 0.6f);
    [SerializeField] private Color negativeColor = new Color(1f, 0.5f, 0.5f);

    [Header("Load Shake")]
    [SerializeField, Range(0.05f, 0.5f)] private float shakeDuration = 0.15f;
    [SerializeField, Range(1f, 10f)] private float shakeStrength = 2f;

    private Coroutine loadShakeRoutine;
    private Vector3 loadTextOriginalPos;

    private int currentTrust;
    private int currentControl;
    private int currentLoad;

    private Coroutine trustValueRoutine;
    private Coroutine controlValueRoutine;
    private Coroutine loadValueRoutine;

    private Coroutine trustFlashRoutine;
    private Coroutine controlFlashRoutine;
    private Coroutine loadFlashRoutine;

    private void Awake()
    {
        if(cityMetrics == null)
        {
            Debug.LogWarning("MetricsUI: CityMetrics reference is missing");
            return;
        }

        currentTrust = cityMetrics.Trust;
        currentControl = cityMetrics.Control;
        currentLoad = cityMetrics.Load;

        trustText.text = currentTrust.ToString();
        controlText.text = currentControl.ToString();
        loadText.text = currentLoad.ToString();

        trustText.color = defaultColor;
        controlText.color = defaultColor;
        loadText.color = defaultColor;

        loadTextOriginalPos = loadText.rectTransform.localPosition;
    }

    private void OnEnable()
    {
        if (cityMetrics == null) return;
        cityMetrics.OnMetricsChanged += HandleMetricsChanged;
    }

    private void OnDisable()
    {
        if (cityMetrics == null) return;
        cityMetrics.OnMetricsChanged -= HandleMetricsChanged;
    }

    private void HandleMetricsChanged()
    {
        UpdateMetric(
            trustText,
            currentTrust,
            cityMetrics.Trust,
            ref trustValueRoutine,
            ref trustFlashRoutine,
            true
        );

        UpdateMetric(
            controlText,
            currentControl,
            cityMetrics.Control,
            ref controlValueRoutine,
            ref controlFlashRoutine,
            true
        );

        UpdateMetric(
           loadText,
           currentLoad,
           cityMetrics.Load,
           ref loadValueRoutine,
           ref loadFlashRoutine,
           false
        );

        if(cityMetrics.Load > currentLoad)
        {
            if(loadShakeRoutine != null) StopCoroutine(loadShakeRoutine);
            loadShakeRoutine = StartCoroutine(ShakeLoadText());
        }

        currentTrust = cityMetrics.Trust;
        currentControl = cityMetrics.Control;
        currentLoad = cityMetrics.Load;
    }

    private void UpdateMetric(
        TMP_Text textElement,
        int oldValue,
        int newValue,
        ref Coroutine valueRoutine,
        ref Coroutine flashRoutine,
        bool higerIsBetter)
    {
        if (oldValue == newValue) return;
        if (valueRoutine != null) StopCoroutine(valueRoutine);

        valueRoutine = StartCoroutine(AnimateValue(textElement, oldValue, newValue));

        bool positive = higerIsBetter
            ? newValue > oldValue
            : newValue < oldValue;

        if(flashRoutine != null) StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashText(textElement, positive));
    }

    private IEnumerator AnimateValue(TMP_Text textElement, int from, int to)
    {
        float elapsed = 0f;

        while (elapsed < valueAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed/valueAnimDuration);
            int value = Mathf.RoundToInt(Mathf.Lerp(from, to, t));
            textElement.text = value.ToString();
            yield return null;
        }

        textElement.text = to.ToString();
    }

    private IEnumerator FlashText(TMP_Text textElement, bool positive)
    {
        Color flashColor = positive ? positiveColor : negativeColor;

        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / flashDuration);

            // 0 -> 1 -> 0
            float pulse = 1f - Mathf.Abs(2f * t - 1f);
            textElement.color = Color.Lerp(defaultColor, flashColor, pulse);

            yield return null;
        }

        textElement.color = defaultColor;
    }

    private IEnumerator ShakeLoadText()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            float offsetX = Random.Range(-shakeStrength, shakeStrength);
            float offsetY = Random.Range(-shakeStrength, shakeStrength);

            loadText.rectTransform.localPosition = loadTextOriginalPos + new Vector3(offsetX, offsetY, 0f);
            yield return null;
        }

        loadText.rectTransform.localPosition = loadTextOriginalPos;
        loadShakeRoutine = null;
    }
}
