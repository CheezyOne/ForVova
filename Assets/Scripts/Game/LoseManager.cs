using UnityEngine;

public class LoseManager : Singleton<LoseManager>
{
    [SerializeField] private LoseWindow _loseWindow;
    [SerializeField] private LosingTimerWindow _losingTimerWindow;

    private bool _lostOnce;

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
            Lose();
    }
#endif

    public void Lose()
    {
        WindowsManager.Instance.CloseAllWindows();

        if (!_lostOnce)
        {
            WindowsManager.Instance.OpenWindow(_losingTimerWindow);
            _lostOnce = true;
            return;
        }

        SoundsManager.Instance.PlaySound(SoundType.Lose);
        WindowsManager.Instance.OpenWindow(_loseWindow);
    } 
}