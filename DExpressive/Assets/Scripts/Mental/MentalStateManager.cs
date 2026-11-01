
using System;
using UnityEngine;

public class MentalStateManager : MonoBehaviour
{
    public event Action OnMentalStateDepleted;

    private GameStateManager gameState;

    private void Awake()
    {
        gameState = GameStateManager.Instance;
    }

    public void ChangeMentalState(int amount)
    {
        if (gameState == null) return;

        GameStateData data = gameState.CurrentState;

        if (data.mentalState <= 0) return;

        data.mentalState = Mathf.Clamp(
            data.mentalState + amount, 0, 100
        );

        Debug.Log("현재 정신력: " + data.mentalState);

        if (data.mentalState == 0)
        {
            OnMentalStateDepleted?.Invoke();
        }
    }
}
