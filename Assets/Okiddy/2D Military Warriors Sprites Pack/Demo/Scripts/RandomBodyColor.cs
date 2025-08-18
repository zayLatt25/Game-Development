using Alchemy.Inspector;
using UnityEngine;

namespace Demo {
    public class RandomBodyColor : MonoBehaviour {
        [SerializeField] private Vector2 _randomHue = new(0f, 1f);
        [SerializeField] private Vector2 _randomSaturation = new(0.5f, 1f);
        [SerializeField] private Vector2 _randomValue = new(0.5f, 0.8f);
        [SerializeField] private bool _randomOnEnable = false;

        private SpriteRenderer _spriteRenderer;

        private void OnEnable() {
            if (_randomOnEnable == false) {
                return;
            }

            RandomColor();
        }

        [Button]
        public void RandomColor() {
            if (_spriteRenderer == null) {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }
            _spriteRenderer.color = Color.HSVToRGB(Random.Range(_randomHue.x, _randomHue.y),
                                                   Random.Range(_randomSaturation.x, _randomSaturation.y),
                                                   Random.Range(_randomValue.x, _randomValue.y));
        }
    }
}