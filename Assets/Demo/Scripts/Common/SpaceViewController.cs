using Cinemachine;
using UnityEngine;
using UnityFunction;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

/// <summary>
/// 스페이스 확대 축소 카메라 액션
///
/// 화면 스와이프시 액션
/// </summary>
namespace dduR {

public class SpaceViewController
{
    private readonly float ZOOM_RATE = 100;

    private CinemachineVirtualCamera _zoomCamera;

    private CinemachineCameraOffset _camOffset;
    private InputDetector _inputDetector;

    [SerializeField]
    private float _minZoomValue = 2.5f;
    [SerializeField]
    private float _maxZoomValue = 8.0f;

    private Vector3 _dragStartPos;
    private float _multiDelta;
    private bool _isDrag;
    private Vector3 _initCamPosition = Vector3.zero;

    public SpaceViewController(CinemachineVirtualCamera zoomCamera, InputDetector inputDetector)
    {
        _zoomCamera = zoomCamera;
        _inputDetector = inputDetector;

        _initCamPosition = _zoomCamera.transform.position;
    }

    public void Update()
    {
        if (_inputDetector == null)
            return;

        if (_inputDetector.PrimaryPhase == TouchPhase.Moved)
        {
            switch (_inputDetector.PressedState)
            {
                case PressedState.Single when _isDrag == false:
                    {
                        _dragStartPos = Camera.main.ScreenToWorldPoint(_inputDetector.PrimaryPosition);
                        _isDrag = true;
                    }
                    break;
                case PressedState.Single:
                    {
                        var mousePos = Camera.main.ScreenToWorldPoint(_inputDetector.PrimaryPosition);
                        var diff = mousePos - _zoomCamera.transform.position;
                        _zoomCamera.transform.position = _dragStartPos - diff;

                        break;
                    }
                case PressedState.Multiple:
                    {
                        _multiDelta = _inputDetector.MultipleDelta();
                        OnMultiTouchDrag(_multiDelta);
                    }
                    break;
            }
        }
        else if (_inputDetector.PrimaryPhase == TouchPhase.Ended)
        {
            _isDrag = false;
        }

        if (_inputDetector.PrimaryScroll != Vector2.zero)
        {
            OnMultiTouchDrag(_inputDetector.PrimaryScroll.y);
        }
    }

    private void OnMultiTouchDrag(float direct)
    {
        var flucate = direct / ZOOM_RATE;
        var ortho = _zoomCamera.m_Lens.OrthographicSize + flucate;

        _zoomCamera.m_Lens.OrthographicSize = Mathf.Clamp(ortho, _minZoomValue, _maxZoomValue);
        if (_zoomCamera.m_Lens.OrthographicSize == _maxZoomValue)
        {
            _zoomCamera.transform.position = _initCamPosition;
        }
    }
}

}