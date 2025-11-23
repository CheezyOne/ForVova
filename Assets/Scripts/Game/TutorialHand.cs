using DG.Tweening;
using UnityEngine;

public class TutorialHand : MonoBehaviour
{
    [SerializeField] private float _animationScale;
    [SerializeField] private float _animationTime;
    [SerializeField] private RectTransform _tutorialHand;

    public float HandWidth => _tutorialHand.sizeDelta.x;
    public float HandHeight => _tutorialHand.sizeDelta.y;

    private void Awake()
    {
        _tutorialHand.DOScale(_animationScale, _animationTime).SetLoops(-1, LoopType.Yoyo);
    }

    private void OnDestroy()
    {
        _tutorialHand.DOKill();
    }
}