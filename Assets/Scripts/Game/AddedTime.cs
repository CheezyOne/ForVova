using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class AddedTime : MonoBehaviour
{
    [SerializeField] private float _movementDistance;
    [SerializeField] private float _movementTime;
    [SerializeField] private Text _text;

    public void Init(int addedTime)
    {
        _text.text = "+" + addedTime;
        transform.DOMoveY(transform.position.y + _movementDistance, _movementTime);
        _text.DOFade(0, _movementTime).OnComplete(()=>Destroy(gameObject));
    }
}