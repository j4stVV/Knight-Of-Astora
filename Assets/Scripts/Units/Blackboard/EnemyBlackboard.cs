using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyBlackboard
{
    public Transform targetPlayer;
    public Transform targetNPC;
    public Transform targetStructure;
    public List<Transform> nearbyAllies = new();

    public int hp;
    public int hpMax;
    public int hpLowThreshold;

    public bool waveActive;
    public bool commandIssued;
    public bool isEnraged;

    private Dictionary<string, object> data = new Dictionary<string, object>();
public void SetData(string key, object value)
    {
        data[key] = value;
    }
    public void Set(string key, object value)
    {
        data[key] = value;
    }

    public T Get<T>(string key)
    {
        if (data.TryGetValue(key, out object value))
        {
            return (T)value;
        }
        return default;
    }
}
