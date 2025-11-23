using UnityEngine;

public class SingletonDontDestroyOnLoad<T> : MonoBehaviour where T : SingletonDontDestroyOnLoad<T>
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<T>();
            }

            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if(_instance != null && _instance != (T)this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = (T)this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
