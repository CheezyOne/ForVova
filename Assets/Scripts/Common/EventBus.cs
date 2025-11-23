using System;
using UnityEngine;

public static class EventBus
{
    public static Action<Bolt> OnBoltCompleted;
    public static Action OnLevelCompleted;
    public static Action OnLevelGenerated;

    public static Action OnMoneyChanged;

    public static Action<Bolt> OnScrewSelected;
    public static Action OnScrewMoved;

    public static Action OnMoneyReward;
    public static Action OnContinuePlayingReward;

    public static Action OnShuffleBoost;
    public static Action OnScrewReturnBoost;
    public static Action OnAddBoltBoost;
}