using System.Collections; 
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DecisionUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject root;// Объект с самой панелью (WindowRoot)
    [SerializeField] private TextMeshProUGUI idText;// Текст типа инцидента
    [SerializeField] private TextMeshProUGUI severityText;// Текст тяжести инцидента
    [SerializeField] private TextMeshProUGUI titleText;// Текст описания инцидента
    [SerializeField] private Button protocolAButton;// Кнопка А протокола
    [SerializeField] private Button protocolBButton;// Кнопка B протокола
    [SerializeField] private ProtocolButtonFeedback protocolAFeedback;
    [SerializeField] private ProtocolButtonFeedback protocolBFeedback;

    [Header("Refs")]
    [SerializeField] private IncidentSystem incidentSystem;

    [Header("Animation")]
    [SerializeField] private CanvasGroup dimBackground;
    [SerializeField] private CanvasGroup windowCanvasGroup;
    [SerializeField] private RectTransform windowRoot;
    [SerializeField, Range(0.05f, 0.5f)] private float showDuration = 0.18f;
    [SerializeField, Range(0.05f, 0.5f)] private float hideDuration = 0.15f;
    [SerializeField, Range(0.8f, 1f)] private float startScale = 0.95f;
    [SerializeField, Range(0f, 1f)] private float dimTargetAlpha = 0.4f;
    [SerializeField, Range(0.05f, 0.3f)] private float confirmDelay = 0.12f;

    private Coroutine confirmRoutine;

    private bool _locked;
    private Coroutine transitionRoutine;

    private void Awake()
    {
        SetVisualStateHiddenInstant();

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
        if(inc == null) return;

        idText.text = inc.Id;
        severityText.text = inc.BaseSeverity.ToString();
        titleText.text = inc.Title;

        protocolAButton.interactable = true;
        protocolBButton.interactable = true;

        if (protocolAFeedback != null) protocolAFeedback.SetLocked(false);
        if (protocolBFeedback != null) protocolBFeedback.SetLocked(false);

        _locked = false;

        if (root != null) root.SetActive(true);

        StartTransition(true);
    }

    private void Hide()
    {
        if (root == null || !root.activeSelf)
            return;

        StartTransition(false);
    }

    private void Choose(ProtocolId protocol)
    {
        if (_locked) return;
        _locked = true;

        protocolAButton.interactable = false;
        protocolBButton.interactable = false;

        if (confirmRoutine != null)
            StopCoroutine(confirmRoutine);

        confirmRoutine = StartCoroutine(ChooseRoutine(protocol));
    }

    private void StartTransition(bool show)
    {
        if (transitionRoutine != null) StopCoroutine(transitionRoutine);

        transitionRoutine = StartCoroutine(TransitionRoutine(show));
    }

    private IEnumerator TransitionRoutine(bool show)
    {
        float duration = show ? showDuration : hideDuration;
        float elapsed = 0f;

        float fromDim = show ? 0f : dimTargetAlpha;
        float toDim = show ? dimTargetAlpha : 0f;

        float fromAlpha = show ? 0f : 1f;
        float toAlpha = show ? 1f : 0f;

        float fromScale = show ? startScale : 1f;
        float toScale = show ? 1f : startScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = EaseOutCubic(t);

            if (dimBackground != null)
                dimBackground.alpha = Mathf.Lerp(fromDim, toDim, eased);

            if (windowCanvasGroup != null)
                windowCanvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, eased);

            if (windowRoot != null)
            {
                float scale = Mathf.Lerp(fromScale, toScale, eased);
                windowRoot.localScale = new Vector3(scale, scale, scale);
            }

            yield return null;
        }

        if (dimBackground != null)
            dimBackground.alpha = toDim;

        if (windowCanvasGroup != null)
            windowCanvasGroup.alpha = toAlpha;

        if (windowRoot != null)
            windowRoot.localScale = new Vector3(toScale, toScale, toScale);

        transitionRoutine = null;

        if (!show && root != null)
            root.SetActive(false);

        if (dimBackground != null)
        {
            dimBackground.alpha = toDim;
            dimBackground.blocksRaycasts = show;
            dimBackground.interactable = show;
        }
    }

    private IEnumerator ChooseRoutine(ProtocolId protocol)
    {
        bool isProtocolA = protocol == ProtocolId.Intervene;

        if (isProtocolA)
        {
            if (protocolAFeedback != null) protocolAFeedback.PlayConfirm();
            if (protocolBFeedback != null) protocolBFeedback.PlayRejected();
        }
        else
        {
            if (protocolBFeedback != null) protocolBFeedback.PlayConfirm();
            if (protocolAFeedback != null) protocolAFeedback.PlayRejected();
        }

        yield return new WaitForSeconds(confirmDelay);

        incidentSystem.ResolveIncident(protocol);
        confirmRoutine = null;
    }

    private void SetVisualStateHiddenInstant()
    {
        if (dimBackground != null)
            dimBackground.alpha = 0f;

        if (windowCanvasGroup != null)
            windowCanvasGroup.alpha = 0f;

        if (windowRoot != null)
            windowRoot.localScale = new Vector3(startScale, startScale, startScale);

        if (dimBackground != null)
        {
            dimBackground.alpha = 0f;
            dimBackground.blocksRaycasts = false;
            dimBackground.interactable = false;
        }
    }

    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }
}
