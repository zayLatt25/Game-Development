using System;
using System.Collections;
using UnityEngine;

namespace Game {
    public class Preview : MonoBehaviour {
        [SerializeField] private Transform[] _appearTransforms;
        [SerializeField] private float _appearDelayBetween = 0.2f;
        [SerializeField] private float _appearDuration = 0.5f;
        [SerializeField] private AnimationCurve _appearCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private float _disappearDelayBetween = 0.2f;
        [SerializeField] private float _disappearDuration = 0.5f;
        [SerializeField] private AnimationCurve _diappearCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        private Vector3[] _initScales;
        private Action _onDisappearComplete;

        public void OnEnable() {
            if (_initScales == null) {
                _initScales = new Vector3[_appearTransforms.Length];
                for (int i = 0; i < _appearTransforms.Length; i++) {
                    _initScales[i] = _appearTransforms[i].localScale;
                }
            }

            StopAllCoroutines();

            foreach (var transform in _appearTransforms) {
                transform.localScale = Vector3.zero;
            }

            StartCoroutine(AppearAll());
        }

        private void OnDisable() {
            StopAllCoroutines();
        }

        public void Hide(Action callback) {
            _onDisappearComplete = callback;
            StartCoroutine(DisappearAll());
        }

        IEnumerator AppearAll() {
            WaitForSeconds delay = new WaitForSeconds(_appearDelayBetween);

            int index = 0;
            foreach (var transform in _appearTransforms) {
                StartCoroutine(Appear(index, transform));
                yield return delay;
                index++;
            }
        }

        IEnumerator Appear(int index, Transform transform) {
            transform.gameObject.SetActive(false);
            transform.gameObject.SetActive(true);   // reenable so animations will reset

            Vector3 targetScale = _initScales[index];
            for (float i = 0f; i < 1f; i += Time.deltaTime / _appearDuration) {
                transform.localScale = Vector3.LerpUnclamped(Vector3.zero, targetScale, _appearCurve.Evaluate(i));
                yield return null;
            }
        }

        IEnumerator DisappearAll() {
            WaitForSeconds delay = new WaitForSeconds(_disappearDelayBetween);

            int index = 0;
            foreach (var transform in _appearTransforms) {
                StartCoroutine(Disappear(index, transform));
                yield return delay;
                index++;
            }

            yield return new WaitForSeconds(_disappearDuration - _disappearDelayBetween + 0.05f);
            gameObject.SetActive(false);
            _onDisappearComplete?.Invoke();
        }

        IEnumerator Disappear(int index, Transform transform) {
            Vector3 startScale = transform.localScale;
			Vector3 targetScale = 0.1f * _initScales[index];
            for (float i = 0f; i < 1f; i += Time.deltaTime / _appearDuration) {
                transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, _diappearCurve.Evaluate(i));
                yield return null;
            }
			transform.localScale = Vector3.zero;
        }
    }
}