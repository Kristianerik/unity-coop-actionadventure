using System.Collections.Generic;
using UnityEngine;

public class GameStateSnapshot 
{
    
    private List<IResettable> _resettables = new List<IResettable>();

    public void RegisterResettable(IResettable resettable)
    {
        if (!_resettables.Contains(resettable)) _resettables.Add(resettable);
    }

    public void TakeSnapshot()
    {
        foreach (var resettable in _resettables) resettable.SaveState();

        Debug.Log($"Snapshot taken - {_resettables.Count} objects saved");
    }

    public void RestoreSnapshot()
    {
        foreach (var resettable in _resettables) resettable.RestoreState();

        Debug.Log($"Snapshot restored - {_resettables.Count} objects restored");
    }

    public void RemoveResettable(IResettable resettable)
    {
        _resettables.Remove(resettable);
    }
}
