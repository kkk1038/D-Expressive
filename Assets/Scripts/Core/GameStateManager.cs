
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance
    {
        get;
        private set;
    }

    public GameStateData CurrentState
    {
        get;
        private set;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CurrentState = new GameStateData();
    }

    public void SetState(GameStateData newState)
    {
        if (newState == null)
        {
            Debug.LogError("게임 상태 데이터가 없습니다.");
            return;
        }

        CurrentState = newState;
    }

    public void ChangeDay(int newDay)
    {
        CurrentState.day = newDay;
        Debug.Log("현재 날짜: " + newDay);
    }

    public void ChangeMentalState(int amount)
    {
        CurrentState.mentalState = Mathf.Clamp(
            CurrentState.mentalState + amount,
            0,
            100
        );

        Debug.Log(
            "현재 정신력: " + CurrentState.mentalState
        );
    }
}
