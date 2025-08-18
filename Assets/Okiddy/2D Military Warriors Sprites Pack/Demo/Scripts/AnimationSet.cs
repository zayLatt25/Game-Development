using UnityEngine;

namespace Game {
    public class AnimationSet : MonoBehaviour {
        [SerializeField] private string _animationName;
        [SerializeField, Range(0f, 1f)] private float _normalizedTime = 0f;

        private void OnEnable() {
            GetComponent<Animator>().Play(_animationName, -1, _normalizedTime);
        }
    }
}