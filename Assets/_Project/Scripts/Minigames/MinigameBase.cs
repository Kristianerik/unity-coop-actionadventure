using UnityEngine;

public abstract class MinigameBase : MonoBehaviour
{
    
    public System.Action<int> OnMinigameComplete;
    protected Coroutine _activeCoroutine;

    public abstract void StartMinigame();
    public abstract void StopMinigame();
    public abstract void OnHit();

    protected void StartMinigameCoroutine(System.Collections.IEnumerator routine)
    {
        if (_activeCoroutine != null)
            StopCoroutine(_activeCoroutine);
        _activeCoroutine = StartCoroutine(routine);
    }

    protected void StopMinigameCoroutine()
    {
        if (_activeCoroutine != null)
        {
            StopCoroutine(_activeCoroutine);
            _activeCoroutine = null;
        }
    }
}
