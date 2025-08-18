using UnityEngine;
using UnityEngine.UI;

namespace Game {
    public class PreviewController : MonoBehaviour {
        [SerializeField] private Preview[] _previews;
        [SerializeField] private Button _next;
        [SerializeField] private Button _prev;
        private Preview _activePreview;

        private int _previewIndex = 0;
        private bool _previewHiding;

        private void Awake() {
            _next.onClick.AddListener(SetNextPreview);
            _prev.onClick.AddListener(SetPrevPreview);

            UpdatePreview();
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) {
                SetNextPreview();
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) {
                SetPrevPreview();
            }
        }

        private void SetNextPreview() {
            if (_previewHiding == false) _previewIndex++;
            UpdatePreview();
        }

        private void SetPrevPreview() {
            if (_previewHiding == false) _previewIndex--;
            UpdatePreview();
        }

        private void UpdatePreview() {
            if (_previewIndex >= _previews.Length) _previewIndex = 0;
            else if (_previewIndex < 0) _previewIndex = _previews.Length - 1;
            
            if (_activePreview != null) {
                if (_previewHiding == false) {
                    _previewHiding = true;
                    _activePreview.Hide(() => {
                        ShowNextPreview();
                    });
                }
                else {
                    _activePreview.gameObject.SetActive(false);
                    ShowNextPreview();
                }
            }
            else {
                ShowNextPreview();
            }
        }

        private void ShowNextPreview() {
            _previewHiding = false;
            _activePreview = _previews[_previewIndex];
            _activePreview.gameObject.SetActive(true);
        }
    }
}