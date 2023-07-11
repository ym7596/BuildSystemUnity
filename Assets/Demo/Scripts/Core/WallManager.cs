using UnityEngine;
using DG.Tweening;
using Cinemachine;
using System.Collections;
using UnityUtility;


namespace dduR
{
    public class WallManager : MonoBehaviour
    {
        private int _rotValue = 90;

        private bool _isRotateLock = false;
        private bool _isStartAnimReady = true;

        [SerializeField]
        private Transform[] _wallTransform;
        [SerializeField]
        private CameraViewDir _currentCamView;

        private WallAnimPreset wallAnimpreset = new WallAnimPreset();

        public float _dotAnimDelay = 0.4f;

        public CameraViewDir currentCamView => _currentCamView;

        private void OnEnable()
        {
            StartCoroutine(StartAnimCo());
        }

        private IEnumerator StartAnimCo()
        {
            transform.DOMove(new Vector3(0, 3, 0), 0);
            _isStartAnimReady = false;
            for (int i = 0; i < _wallTransform.Length; i++)
            {

                _wallTransform[i].DOScale(new Vector3(1, 1, 1), 0);
                if (i == _wallTransform.Length - 1)
                    _isStartAnimReady = true;
            }

            yield return new WaitUntil(() => _isStartAnimReady == true);


            transform.DOMove(Vector3.zero, .5f).SetEase(Ease.OutBounce).SetLoops(1).From(new Vector3(0, 3, 0))
                .OnComplete(() => SetCameraViewDir(_currentCamView, true));
        }

        public void OnButton_RoomRotate(bool isLeft)
        {
            if (_isRotateLock == true)
                return;

            StartCoroutine(RoomRotationCo(isLeft));
        }

        public void OnButton_RoomRotate(float detail)
        {
            if (_isRotateLock == true)
                return;

            if (detail < 10)
                RoomRotationCo(detail > 0).CStart(this);
            else
                MoveCamera(detail);
        }

        private void MoveCamera(float detail)
        {

        }

        private IEnumerator RoomRotationCo(bool isLeft)
        {
            if (_isRotateLock == true)
                yield break;

            _isRotateLock = true;

            int camDir = (int) _currentCamView;

            if (isLeft == true)
            {
                camDir++;
            }
            else
            {
                camDir--;
                if (camDir < 0)
                    camDir = 3;
            }

            camDir = camDir % 4;

            transform.DORotate(new Vector3(0, camDir * -_rotValue, 0), 0.3f)
                .OnComplete(() => SetCameraViewDir((CameraViewDir) camDir, isLeft));

            yield return new WaitForSeconds(0.5f);
            _isRotateLock = false;
        }

        private void SetAnimation(CameraViewDir camView, bool isLeft)
        {
            int camint = (int) camView;
            for (int i = 0; i < _wallTransform.Length; i++)
            {
                if (isLeft == true)
                {
                    if (i == camint) //기준 내려가야함
                    {
                        wallAnimpreset.DownAnim(_wallTransform[i], _dotAnimDelay);
                    }
                    else if (i == GetNextNum(camint, isLeft)) // 기준+1 없어지면서 움직임
                    {
                        wallAnimpreset.SetScaleZero(_wallTransform[i]);
                    }
                    else if (i == GetNextNum(camint, !isLeft)) //기준-1 서있는 상태에서 그대로 이동
                    {
                    }
                    else //기준 +2 대각선 새로 올라와야함
                    {
                        wallAnimpreset.UpAnim(_wallTransform[i], _dotAnimDelay);
                    }
                }
                else
                {
                    if (i == camint) //좌하단
                    {
                        wallAnimpreset.SetScaleZero(_wallTransform[i]);
                    }
                    else if (i == GetNextNum(camint, isLeft)) //기준 -1 내려가야함
                    {
                        wallAnimpreset.UpAnim(_wallTransform[i], _dotAnimDelay);
                    }
                    else if (i == GetNextNum(camint, !isLeft)) //기준 +1
                    {
                        wallAnimpreset.DownAnim(_wallTransform[i], _dotAnimDelay);
                    }
                    else //우상단
                    {
                    }
                }

            }
        }

        private void SetCameraViewDir(CameraViewDir dir, bool isLeft)
        {
            _currentCamView = dir;
            SetAnimation(_currentCamView, isLeft);
        }

        private int GetNextNum(int val, bool isLeft)
        {
            if (isLeft == true)
            {
                val -= 1;
                if (val < 0)
                    val = 3;
            }
            else
            {
                val += 1;
                if (val > 3)
                    val = 0;
            }

            return val;
        }
    }


    public class WallAnimPreset
    {
        public void DownAnim(Transform target, float delay)
        {
            target.DOScale(new Vector3(1, 0, 1), delay).SetEase(Ease.InBack).
                OnComplete(() => target.DOScale(new Vector3(0, 0, 0), 0.1f));
        }

        public void UpAnim(Transform target, float dealy)
        {
            target.DOScale(new Vector3(1, 1, 1), dealy).SetEase(Ease.OutBack).From(new Vector3(1, 0, 1));
        }

        public void SetScaleZero(Transform target)
        {
            target.DOScale(Vector3.zero, 0);
        }


    }

}


