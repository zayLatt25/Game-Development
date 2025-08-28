using System;
using System.Collections;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    [field: SerializeField] public int TimeLeft { get; private set; } = 600;
    [SerializeField, Range(1, 10)] private int _timeMultiplier = 1;

    public event Action<int> TimeLeftChanged;

    private void Start()
    {
        StartCoroutine(TimerCoroutine());
    }

    private IEnumerator TimerCoroutine()
    {
        while (TimeLeft > 0)
        {
            yield return new WaitForSeconds(1);
            TimeLeft -= _timeMultiplier;
            TimeLeftChanged?.Invoke(TimeLeft);
        }
    }
}
