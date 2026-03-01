using System;
using UnityEngine;

/// <summary>
/// IncidentSystem отвечает за появление новых инцидентов
/// </summary>
public class IncidentSystem : MonoBehaviour
{
    [Header("Spawn settings")]
    [SerializeField] private float incidentIntervalSeconds = 10f;

    private float _timer;

    // Сис-ма хранения инцидента
    private Incident _currentIncident;
    public bool HasActiveIncident => _currentIncident != null;
    public Incident CurrentIncident => _currentIncident;
    private Incident[] _incidentCatalog;

    private GameStateManager _gameState;
    private CityMetrics _metrics;

    public event Action<Incident> OnIncidentRaised;
    public event Action OnIncidentCleared;

    private void Awake()
    {
        // Ищем зависимости в сцене (позже сделаем аккуратнее через ссылки/DI).
        _gameState = FindFirstObjectByType<GameStateManager>();
        _metrics = FindFirstObjectByType<CityMetrics>();

        _incidentCatalog = new[]
        {
            new Incident("FIRE", "Пожар в Distric-01", 7),
            new Incident("ACC", "ДТП на Main Ave", 4),
        };
    }

    private void Update()
    {
        // Генерируем инциденты только когда игра "идёт"
        if (_gameState == null || _metrics == null)
            return;

        // Тест системы решений инцидентов
        if (_gameState.CurrentState == GameState.IncidentActive)
        {
            if (Input.GetKeyDown(KeyCode.Y))
                ResolveIncident(ProtocolId.Intervene);
            if (Input.GetKeyDown(KeyCode.N))
                ResolveIncident(ProtocolId.Wait);
        }
        //-----

        if (_gameState.CurrentState != GameState.Running)
            return;

        _timer += Time.deltaTime;

        if(_timer >= incidentIntervalSeconds)
        {
            _timer = 0;
            SpawnTestIncident();
        }
    }

    private void SpawnTestIncident()
    {
        // Пока тупо: случайно выбираем тип из двух
        int roll = UnityEngine.Random.Range(0, _incidentCatalog.Length);
        _currentIncident = _incidentCatalog[roll];
        OnIncidentRaised?.Invoke(_currentIncident);

        Debug.Log($"[Incident] {_currentIncident.Id}: {_currentIncident.Title}. Тяжесть: {_currentIncident.BaseSeverity}");

        // Переводим игру в режим активного инцидента:
        // дальше генерация стопается, пока игрок не "разрулит".
        _gameState.SetState(GameState.IncidentActive);
    }

    public void ResolveIncident(ProtocolId protocol)
    {
        if (_currentIncident == null)
            return;

        Debug.Log($"[Incident] Resolving {_currentIncident.Id}. Protocol: " + protocol.ToString());

        // Простейшая логика последствий (для FIRE/ACC).
        switch (_currentIncident.Id)
        {
            case "FIRE":
                ApplyFire(protocol, _currentIncident.BaseSeverity);
                break;
            case "ACC":
                ApplyAccident(protocol, _currentIncident.BaseSeverity);
                break;
            default:
                break;
        }

        _currentIncident = null;
        OnIncidentCleared?.Invoke();
        _gameState.SetState(GameState.Running);
    }

    private void ApplyFire(ProtocolId protocol, int severity)
    {
        // Чем выше тяжесть, тем сильнее эффекты
        // Вмешаться: доверие растёт, нагрузка растёт, контроль слегка растёт
        // Выждать: доврие падает, контроль падает, нагрузка растёт ещё сильнее
        if (protocol == ProtocolId.Intervene)
            _metrics.ApplyChange(+2 + severity / 3, +1, +3 + severity); // пример
        else
            _metrics.ApplyChange(-3 - severity / 2, -2, +5 + severity * 2);
    }

    private void ApplyAccident(ProtocolId protocol, int severity)
    {
        // ДТП: контроль важнее, доверие менее чувствительно, нагрузка тоже растёт
        if (protocol == ProtocolId.Intervene)
            _metrics.ApplyChange(+1, +2 + severity / 2, +2 + severity); // пример
        else
            _metrics.ApplyChange(-2, -3 - severity / 2, +3 + severity * 2);
    }
}
