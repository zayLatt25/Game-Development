using UnityEngine;

namespace Demo {
    public class SpriteColorHueChanger : MonoBehaviour {
        [SerializeField] private float _speed = 360f;
        [SerializeField, Range(0f, 360f)] private float _hue;
        [SerializeField, Range(0f, 1f)] private float _saturation = 0.8f;
        [SerializeField, Range(0f, 1f)] private float _value = 1f;
        
        private SpriteRenderer _spriteRenderer;

        private void Awake() {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update() {
            _hue += Time.deltaTime * _speed;
            _hue %= 360f;
            SetHue(_hue);
        }

        private void SetHue(float value) {
            float normalizedHue = value / 360f;
            _spriteRenderer.color = Color.HSVToRGB(normalizedHue, _saturation, _value); ;
        }

        private void OnValidate() {
            if (_spriteRenderer == null) {
                _spriteRenderer = GetComponent<SpriteRenderer>();
                if (_spriteRenderer == null) {
                    return;
                }
            }
            SetHue(_hue);
        }
    }
}