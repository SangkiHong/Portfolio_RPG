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
            // �޴� ȭ�� ǥ��
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

            // �� �ε� ȭ�� ǥ��
            if (_showLoadScreen)
            {
                if (loadingCanvas.alpha < 1)
                {
                    _elapsed += Time.deltaTime;
                    float alpha = _elapsed / menuAppearTime;
                    loadingCanvas.alpha = alpha;

                    // ȭ�� ǥ�ð� �Ϸ�� ���
                    if (alpha >= 1)
                    {
                        _showLoadScreen = false;

                        // ���� �� �ε�
                        StartCoroutine(ChangeScene());
                    }
                }
            }

            // �� �ε尡 �Ǿ��ٸ�
            if (_isOnLoaded)
            {
                // �����̴� ���� �ִ��
                if (!_isOnSliderMax)
                {
                    slider_Progress.value += Time.deltaTime * 0.7f;

                    if (slider_Progress.value >= 1)
                        _isOnSliderMax = true;
                }
                // �����̴��� �ִ� ���� ��� ���� ȭ�� ����
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
