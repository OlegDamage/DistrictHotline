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

        if(roll == 0)
        {
            Debug.Log("[Incident] FIRE reported in District-01. Suggested: PROTOCOL_A (Containment).");
            // Для теста увеличим нагрузку и немного снизим доверие, если "горит"
            _metrics.ApplyChange(trustDelta: -3, controlDelta: 0, loadDelta: +10);
        }
        else
        {
            Debug.Log("[Incident] TRAFFIC ACCIDENT on Main Ave. Suggested: PROTOCOL_B (Reroute + EMS).");
            _metrics.ApplyChange(trustDelta: -1, controlDelta: +1, loadDelta: +6);
        }

        // Переводим игру в режим активного инцидента:
        // дальше генерация стопается, пока игрок не "разрулит".
        _gameState.SetState(GameState.IncidentActive);
    }
}
