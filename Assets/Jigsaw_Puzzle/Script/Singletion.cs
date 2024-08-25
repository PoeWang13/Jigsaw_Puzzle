using UnityEngine;

public class Singletion<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    private static object _lock = new object();
    private void Awake()
    {
        lock (_lock)
        {
            if (instance == null)
            {
                instance = this as T;
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }
        OnAwake();
    }
    public static T Instance
    {
        get
        {
            return instance;
        }
    }
    public virtual void OnAwake()
    {

    }
}