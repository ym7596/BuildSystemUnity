using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICanvas : MonoBehaviour
{
    [SerializeField]
    private GameObject _editBar;

    [SerializeField]
    private RectTransform _canvasRect;
    [SerializeField]
    private RectTransform _uiEditBarRect;

    [SerializeField]
    private Camera _uicamera;

    [SerializeField]
    private GameObject _togglePanel;

    public GameObject resetPanel;

    public BundleInfos bundleInfos;

   
    public event Action onButton_Editdone;
    public event Action onButton_RotateObject;
    public event Action onButton_RemoveObject;
    public event Action onButton_Save;
    public event Action onButton_Reset;
    public event Action onButton_Revert;
    public event Action<bool> onButton_RoomRotate;
   

   

    public void OnButton_TogglePanel(bool isOn)
    {
        _togglePanel.SetActive(isOn);
    }

    public void SetEditPanelPos(Transform currentPos)
    {
        Vector3 pos = Camera.main.WorldToScreenPoint(currentPos.position);

        float editWidth = _uiEditBarRect.rect.width;
        float editLeftPos = pos.x - (editWidth / 2f);
        float editRightPos = pos.x + (editWidth / 2f);

        if (editLeftPos < 30 || editRightPos > _uicamera.pixelWidth - 30f)
        {
            return;
        }

        _uiEditBarRect.position
        = new Vector3(pos.x, pos.y + 150f, _editBar.transform.position.z);
    }

    public void EditBarOn(bool isOn)
    {
        _editBar.SetActive(isOn);
    }

    public void OnButton_ResetPanel(bool isOn)
    {
        resetPanel.SetActive(isOn);
    }

    public void OnButton_Save()
    {
        onButton_Save?.Invoke();
    }

    public void OnButton_Revert()
    {
        onButton_Revert?.Invoke();

      //  resetPanel.SetActive(false);
    }

    public void OnButton_Reset()
    {
        onButton_Reset?.Invoke();

      //  resetPanel.SetActive(false);
    }

    public void OnButton_EditDone()
    {
        onButton_Editdone?.Invoke();
    }

    public void OnButton_RotateObject()
    {
        onButton_RotateObject?.Invoke();
    }

    public void OnButton_RoomRotate(bool isLeft)
    {
        onButton_RoomRotate?.Invoke(isLeft);
    }

    public void OnButton_removeObject()
    {
        onButton_RemoveObject?.Invoke();
    }
}
