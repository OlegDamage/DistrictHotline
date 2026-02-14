using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public GameState CurrentState { get; private set; }

    private static GameStateManager _instance;

    private void Awake()
    {
        // Проверка на дубликат GameStateManager
        if(_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        //
    }

    private void Start()
    {
        SetState(GameState.Running);
    }

    public void SetState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log("Game State changed to: " + CurrentState.ToString());
    }
}
