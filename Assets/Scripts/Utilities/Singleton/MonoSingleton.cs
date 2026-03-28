using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    private static T _instance;
    private static readonly object _lock = new object();
    public static T Instance
    {
        get
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<T>() as T;
                    if (_instance == null)
                    {
                        GameObject go = new GameObject(typeof(T).Name);
                        _instance = go.AddComponent<T>();
                    }
                }
                return _instance;
            }
        }
    }
    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = (T)this;
            DontDestroyOnLoad(this.gameObject);
        }
        else Destroy(this.gameObject);
    }
}
