using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class WindowsManager : Singleton<WindowsManager>
{
    [SerializeField] private Transform _windowsCanvas;

    private List<BaseWindow> _openedWindows = new List<BaseWindow>();

    public void OpenWindow(BaseWindow window, bool isOverrided = false)
    {
        if (IsOpened(window.GetType()))
        {
            CloseWindow(window.GetType());
            return;
        }

        if (_openedWindows.Count != 0 && !isOverrided)
            return;

        BaseWindow newWindow = Instantiate(window, _windowsCanvas);
        newWindow.Init();
        _openedWindows.Add(newWindow);
    }

    public void CloseWindow(Type type)
    {
        var window = _openedWindows.FirstOrDefault(x => x.GetType() == type);

        if (window == null || window == default)
            return;

        _openedWindows.Remove(window);
        window.OnClose();
        Destroy(window.gameObject);
    }

    public bool IsOpened(Type type)
    {
        return _openedWindows.Any(x => x.GetType() == type);
    }

    public T FindWindow<T>() where T : BaseWindow
    {
        var window = _openedWindows.FirstOrDefault(x => x.GetType() == typeof(T));

        if (window == null || window == default)
            return null;

        return (T)window;
    }

    public void CloseAllWindows()
    {
        List<BaseWindow> openedWindows = new List<BaseWindow>(_openedWindows);

        for (int i = 0; i < openedWindows.Count; i++)
        {
            if (openedWindows[i] == null || openedWindows[i] == default)
                continue;

            CloseWindow(openedWindows[i].GetType());         
        }
    }
}
