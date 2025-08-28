using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SortingOrderByY : MonoBehaviour
{
    private SpriteRenderer _renderer;

    [Header("Sorting Settings")]
    [SerializeField] private int _offset = 0;
    [SerializeField] private float _precision = 100f;

    [Header("Optional Target")]
    [SerializeField] private Transform _target; // If null, uses self

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        Transform t = _target != null ? _target : transform;
        _renderer.sortingOrder = (int)(-t.position.y * _precision) + _offset;
    }
}