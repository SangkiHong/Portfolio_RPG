using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SK
{
    public class IntroMenuHandler : MonoBehaviour
    {
        [SerializeField] private CanvasGroup menuCanvas;
        [SerializeField] private float menuAppearWaitTime = 1.5f;
        [SerializeField] private float menuAppearTime = 1.5f;

        [Space]
        [SerializeField] private CanvasGroup loadingCanvas;
        [SerializeField] private Image image_BlackCover;
        [SerializeField] private Slider slider_Progress;
        [SerializeField] private float loadingScreenAppearTime = 1.5f;

        private AsyncOperation _async;

        private Color _defaultColor;
        private bool _menuAppeared, _showLoadScreen;
        private bool _isOnLoaded, _isOnSliderMax;
        private float _elapsed;

        private void Start()
        {
            AudioManager.Instance.PlayBackGroundMusic("BGM_Meadow", 0);
        }

        private void Update()
        {
            // 메뉴 화면 표시
            if (!_menuAppeared)
            {
                _elapsed += Time.deltaTime;

                if (_elapsed > menuAppearWaitTime)
                {
                    menuCanvas.alpha = (_elapsed - menuAppearWaitTime) / menuAppearTime;
                    
                    if (menuCanvas.alpha >= 1) 
                        _menuAppeared = true;
                }
            }

            // 씬 로딩 화면 표시
            if (_showLoadScreen)
            {
                if (loadingCanvas.alpha < 1)
                {
                    _elapsed += Time.deltaTime;
                    float alpha = _elapsed / menuAppearTime;
                    loadingCanvas.alpha = alpha;

                    // 화면 표시가 완료된 경우
                    if (alpha >= 1)
                    {
                        _showLoadScreen = false;

                        // 게임 씬 로딩
                        StartCoroutine(ChangeScene());
                    }
                }
            }

            // 씬 로드가 되었다면
            if (_isOnLoaded)
            {
                // 슬라이더 값을 최대로
                if (!_isOnSliderMax)
                {
                    slider_Progress.value += Time.deltaTime * 0.7f;

                    if (slider_Progress.value >= 1)
                        _isOnSliderMax = true;
                }
                // 슬라이더가 최대 값인 경우 검은 화면 변경
                else
                {
                    _defaultColor.a += Time.deltaTime;

                    image_BlackCover.color = _defaultColor;

                    if (_defaultColor.a >= 1)
                        _async.allowSceneActivation = true;
                }
            }
        }

        public void LoadGame()
        {
            _elapsed = 0;
            menuCanvas.alpha = 0;
            menuCanvas.blocksRaycasts = false;
            _showLoadScreen = true;
        }

        IEnumerator ChangeScene()
        {
            _async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(1);
            _async.allowSceneActivation = false;

            while (!_async.isDone)
            {
                if (_async.progress >= 0.9f)
                {
                    _isOnLoaded = true;
                }
                yield return new WaitForEndOfFrame();
            }
        }
    }
}
