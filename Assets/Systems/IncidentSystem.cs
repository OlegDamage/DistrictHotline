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

    // Сис-ма хранения инцидента. ВРЕМЕННОЕ РЕШЕНИЕ
    private int _currentIncidentType = -1;
    private bool _hasActiveIncident = false;

    private GameStateManager _gameState;
    private CityMetrics _metrics;

    private void Awake()
    {
        // Ищем зависимости в сцене (позже сделаем аккуратнее через ссылки/DI).
        _gameState = FindFirstObjectByType<GameStateManager>();
        _metrics = FindFirstObjectByType<CityMetrics>();
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
        int roll = Random.Range(0, 2);

        _currentIncidentType = roll;
        _hasActiveIncident = true;

        if(roll == 0)
            Debug.Log("[Incident] FIRE reported in District-01.");
        else
            Debug.Log("[Incident] TRAFFIC ACCIDENT on Main Ave.");

        // Переводим игру в режим активного инцидента:
        // дальше генерация стопается, пока игрок не "разрулит".
        _gameState.SetState(GameState.IncidentActive);
    }

    public void ResolveIncident(bool success)
    {
        if (!_hasActiveIncident)
            return;

        Debug.Log("[Incident] Resolving... Success: " + success);

        if (_currentIncidentType == 0) // FIRE
        {
            if (success)
                _metrics.ApplyChange(+2, +3, -5);
            else
                _metrics.ApplyChange(-5, -3, +10);
        }
        else if (_currentIncidentType == 1) // ACCIDENT
        {
            if (success)
                _metrics.ApplyChange(+1, +2, -3);
            else
                _metrics.ApplyChange(-3, -2, +6);
        }

        _currentIncidentType = -1;
        _hasActiveIncident = false;

        _gameState.SetState(GameState.Running);
    }
}
