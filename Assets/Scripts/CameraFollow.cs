using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _followSmoothTime = 0.25f;
    [SerializeField] private float _cursorInfluence = 5f; // how much cursor affects camera
    [SerializeField] private float _maxOffset = 3f;      // clamp distance from player

    private Camera _cam;
    private Vector3 _velocity;

    private void Awake()
    {
        _cam = Camera.main;
    }

    private void LateUpdate()
    {
        if (_target == null) return;

        // Get mouse world position
        Vector3 mouseWorldPos = _cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        // Calculate offset toward cursor
        Vector3 offset = (mouseWorldPos - _target.position) / _cursorInfluence;

        // Clamp offset magnitude so it doesn't go too far
        if (offset.magnitude > _maxOffset)
            offset = offset.normalized * _maxOffset;

        // Final target position
        Vector3 targetPos = _target.position + offset;
        targetPos.z = transform.position.z;

        // Smooth move
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _velocity, _followSmoothTime);
    }
}