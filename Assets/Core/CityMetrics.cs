using UnityEngine;

public class CityMetrics : MonoBehaviour
{
    // Диапозон  метрик
    public const int MinValue = 0;
    public const int MaxValue = 100;

    [Header("Current metrics (0...100)")]
    [SerializeField] private int trust = 50;
    [SerializeField] private int control = 50;
    [SerializeField] private int load = 0;

    // Публичное чтение, но менять можно только через ApplyChange
    public int Trust => trust;
    public int Control => control;
    public int Load => load;

    /// <summary>
    /// Применяет изменения к метрикам.
    /// Все значения после изменения зажимаются в пределах MinValue..MaxValue.
    /// </summary>
    public void ApplyChange(int trustDelta, int controlDelta, int loadDelta)
    {
        // 1) Считаем "сырые" новые значения
        int newTrust = trust + trustDelta;
        int newControl = control + controlDelta;
        int newLoad = load + loadDelta;

        // 2) КЛАМП: зажимаем в допустимые значения
        trust = Mathf.Clamp(newTrust, MinValue, MaxValue);
        control = Mathf.Clamp(newControl, MinValue, MaxValue);
        load = Mathf.Clamp(newLoad, MinValue, MaxValue);

        // 3) Лог для проверки
        Debug.Log($"[CityMetrics] Applied Δ(T:{trustDelta}, C:{controlDelta}, L:{loadDelta}) -> " +
                  $"Now (T:{trust}, C:{control}, L:{load})");
    }
}
