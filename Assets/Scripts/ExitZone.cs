using System;
using UnityEngine;

public class ExitZone : MonoBehaviour
{
    public event Action ExitZoneReached;

    private Player _player;
    
    private void Start()
    {
        _player = FindObjectOfType<Player>();
        _player.IsInfectedChanged += OnPlayerIsInfectedChanged;
    }

    private void OnDestroy()
    {
        _player.IsInfectedChanged -= OnPlayerIsInfectedChanged;
    }

    private void OnPlayerIsInfectedChanged(bool isInfected)
    {
        GetComponent<SpriteRenderer>().color = isInfected ? Color.red : Color.green;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.isTrigger && other.TryGetComponent<Player>(out var player) && !player.IsInfected)
        {
            ExitZoneReached?.Invoke();
        }
    }
}