using System;
using UnityEngine;

/// <summary>
/// IncidentSystem отвечает за появление новых инцидентов.
/// Пока без UI: генерим тестовый инцидент по таймеру и переводим игру в IncidentActive.
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
                ResolveIncident(true);
            if (Input.GetKeyDown(KeyCode.N))
                ResolveIncident(false);
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

    public void ResolveIncident(bool success)
    {
        if (_currentIncident == null)
            return;

        Debug.Log($"[Incident] Resolving {_currentIncident.Id}. Success: " + success);

        switch (_currentIncident.Id)
        {
            case "FIRE":
                if (success)
                    _metrics.ApplyChange(+2, +3, -5);
                else
                    _metrics.ApplyChange(-5, -3, +10);
                break;
            case "ACC":
                if (success)
                    _metrics.ApplyChange(+2, +3, -5);
                else
                    _metrics.ApplyChange(-5, -3, +10);
                break;
            default:
                break;
        }

        _currentIncident = null;
        OnIncidentCleared?.Invoke();
        _gameState.SetState(GameState.Running);
    }
}
