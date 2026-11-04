
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameStateData
{
    public int day = 1;
    public int mentalState = 100;
    public int dayStartMentalState = 100;

    public string storyNodeId = "day1_start";

    public Vector3 playerPosition;
    public Quaternion playerRotation = Quaternion.identity;

    public List<string> storyFlags = new();
    public List<string> completedInteractions = new();

    public List<ChoiceRecord> choiceHistory = new();
}

[Serializable]
public class ChoiceRecord
{
    public string choiceId;
    public string selectedOptionId;
}
