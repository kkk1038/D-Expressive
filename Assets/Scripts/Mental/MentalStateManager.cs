
using System;
using UnityEngine;

public class MentalStateManager : MonoBehaviour
{
    public event Action OnMentalStateDepleted;
    public event Action<int> OnMentalStateChanged;

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
        if (amount < 0 && data.storyFlags.Contains("day7_contact_recovery")) return;

        data.mentalState = Mathf.Clamp(
            data.mentalState + amount, 0, 100
        );

        Debug.Log("현재 정신력: " + data.mentalState);
        OnMentalStateChanged?.Invoke(data.mentalState);

        if (data.mentalState == 0)
        {
            OnMentalStateDepleted?.Invoke();
        }
    }
}
