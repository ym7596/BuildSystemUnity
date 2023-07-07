using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityUtility;
using UnityFunction;
using Cinemachine;
using UnityEngine.InputSystem;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace dduR
{

    public class RoomManager : Singleton<RoomManager>
    {
        [SerializeField]
        private InputDetector _inputDetector;

        [SerializeField]
        private UICanvas _uiCanvas;
        [SerializeField]
        private Transform _centerTransform;
        [SerializeField]
        private CinemachineVirtualCamera _cinemachineCamera;

        private MyObject _myObject;
        private MyObject _currentRayObject;
        private MySpaceData _mySpaceData;
        private SpaceViewController _spaceViewController;

        private Dictionary<int, MyObject> _objDict = new Dictionary<int, MyObject>();


        private RaycastHit _hit;
        private LayerMask _layerMask;

        private Coroutine _loadCoroutine;

        private float _holdDuration = 0.2f;
        private float _touchTime = 0f;
        private bool _hasObject;
        private bool _isCreating = false;

        public int rotateAmount = 90;
        public float gridsize = 0.25f;

        public bool editMode = false;
        public bool enterMode = false;

        public Ray myRay { get; private set; }

        public Vector3 pos { get; private set; } = Vector3.zero;

        public MyObject MyObjectPr
        {
            get
            {
                return _myObject;
            }
            set
            {
                _myObject = value;

                bool isObject = value == null ? false : true;

                editMode = isObject;
            }
        }



        protected override void Awake()
        {
            _holdDuration = 0.1f;
            base.Awake();
        }

        protected override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
        }
        private void Start()
        {

            _spaceViewController = new SpaceViewController(_cinemachineCamera, _inputDetector);
            StartCoroutine(InitializeCo());

        }
        public IEnumerator InitializeCo()
        {
            //yield return new WaitUntil(() => _bundleLoader != null);
            yield return null;
            _objDict = new Dictionary<int, MyObject>();

            _mySpaceData = new MySpaceData();
        }


        private void OnEnable()
        {
            
        }

        private void OnDisable()
        {
            
        }


        private void Update()
        {
           // if (enterMode == false)
            //    return;

            var phase = _inputDetector.PrimaryPhase;
            var position = _inputDetector.PrimaryPosition;

#if !UNITY_EDITOR && UNITY_ANDROID
            var touch = _inputDetector.GetTouch(0);

            if(touch != null)
			{
                phase = touch.phase;
                position = touch.position;
			}
#endif

            if (phase == TouchPhase.None || _inputDetector.IsUILayerDetected == true)
                return;

            if (_inputDetector.IsUILayerDetected == false)
            {
                _uiCanvas.OnButton_TogglePanel(false);
            }

            if (editMode == false)
            {
                if (_spaceViewController != null)
                    _spaceViewController.Update();
            }

            myRay = Camera.main.ScreenPointToRay(position);

            UpdateFunction(phase, myRay);
        }
        #region Update Func
        public void UpdateFunction(TouchPhase phase, Ray ray)
        {
            RaycastHit[] hits = Physics.RaycastAll(ray, 100);

            if (Physics.Raycast(ray, out _hit, 100))
            {
                var rayObj = GetCurrentObject(_hit.collider.gameObject);

                switch (phase)
                {
                    case TouchPhase.Began:
                        {
                        }
                        break;
                    case TouchPhase.Moved:
                        {
                            DragTouch(ray);
                        }
                        break;
                    case TouchPhase.Stationary:
                        {
                            if (IsHolding() == true)
                            {
                                HoldTouch(hits);
                                _touchTime = 0;
                            }
                        }
                        break;
                    case TouchPhase.Canceled:
                    case TouchPhase.Ended:
                        {
                            TabTouchEnded(rayObj);
                        }
                        break;
                }
            }
        }

        public void SetSpaceObject(MyObject spaceObj)
        {
            MyObjectPr = spaceObj;
        }
        public void InputClear()
        {
            _hasObject = false;
            pos = Vector3.zero;
        }

        public void PlaceObject()
        {
            if (_myObject == null || MyObjectPr.CanPlaced == false)
                return;


            MyObjectPr.SetPlaced(true);
            MyObjectPr.SetBaseColor(Color.white);
            MyObjectPr.SettingAnim(MyObjectPr.transform.position);

            InputClear();
            SetCurrentObjectState(MyObjectPr);
        }
        public void SetPanelSlide(bool UIDetect)
        {
           // onPanelClose?.Invoke(UIDetect);
        }
        #endregion
        #region Touch Drag Info

        public bool IsHolding()
        {
            _touchTime += Time.time;
            if (_touchTime > _holdDuration)
                return true;

            return false;
        }

        public void DragTouch(Ray ray)
        {
            if (_hasObject == true && MyObjectPr == true)
            {
                _layerMask = SetLayerMask(MyObjectPr.placedType);
                if (Physics.Raycast(ray, out _hit, 1000, _layerMask))
                {
                    pos = SetPositionToState(_hit, MyObjectPr);
                    ObjectMoving(pos);
                }
            }
        }

        public void HoldTouch(RaycastHit[] hits)
        {
            if (IsCurrentObject(hits, MyObjectPr) == false)
                return;

            SetObjectRigidBody(true);

            _hasObject = true;
        }

        public void TabTouchEnded(MyObject curRayObj)
        {
            if (curRayObj == null)
                return;

            //Debug.Log($"TapCheck : {curRayObj}");
            if (_hasObject == false && MyObjectPr == null) //Click
            {
                if (curRayObj == null)
                    return;

                SetCurrentObjectState(curRayObj);
            }
            else // Draging
            {
                if (MyObjectPr == null)
                {
                    InputClear();
                    return;
                }

                SetObjectRigidBody(false);

                if (_hasObject == true)
                    PlaceObject();
            }
            InputClear();
        }

        public void DragObject(Ray ray)
        {
            _layerMask = SetLayerMask(MyObjectPr.placedType);
            if (Physics.Raycast(ray, out _hit, 1000, _layerMask))
            {
                pos = SetPositionToState(_hit, MyObjectPr);
                ObjectMoving(pos);
            }
        }


        public LayerMask SetLayerMask(ObjectPlacedType placedType)
        {
            LayerMask layerMask;

            switch (placedType)
            {
                case ObjectPlacedType.Prop:
                    {
                        layerMask = (1 << LayerMask.NameToLayer(SpaceConfig.GroundTag))
                            | (1 << LayerMask.NameToLayer(SpaceConfig.OnObjectLayer));
                    }
                    break;
                case ObjectPlacedType.Wall:
                    {
                        layerMask = 1 << LayerMask.NameToLayer(SpaceConfig.WallTag);
                    }
                    break;
                default:
                    {
                        layerMask = 1 << LayerMask.NameToLayer(SpaceConfig.GroundTag);
                    }
                    break;
            }
            return layerMask;
        }

        public Vector3 SetPositionToState(RaycastHit _hit, MyObject myObject)
        {
            Vector3 pos = _hit.point;

            switch (myObject.placedType)
            {
                case ObjectPlacedType.Prop:
                    {
                        //MySpaceObject rayObject = _hit.collider.GetComponent<MySpaceObject>();
                        if (IsTopSideCheck(_hit) == false)
                        {
                            pos = new Vector3(pos.x, 0, pos.z);
                            myObject.transform.parent = _centerTransform;

                            if (myObject.ParentIndex != SpaceConfig.newObject)
                            {
                                _objDict[myObject.ParentIndex].SetRemoveChildObject(myObject.Index);
                            }
                            myObject.SetParentId(SpaceConfig.newObject);
                        }
                        else
                        {
                            myObject.transform.parent = _hit.transform;
                            myObject.SetParentId(_hit.collider.GetComponent<MyObject>().Index);
                        }
                    }
                    break;
                case ObjectPlacedType.Wall:
                    {
                    }
                    break;
                default:
                    {
                        pos = new Vector3(pos.x, 0, pos.z);
                    }
                    break;
            }
            return pos;
        }

        public bool IsTopSideCheck(RaycastHit hitPoint)
        {
            Vector3 localPoint = hitPoint.transform.InverseTransformPoint(hitPoint.point);
            Vector3 localDir = localPoint.normalized;

            float upDot = Vector3.Dot(localDir, Vector3.up);
            float fwdDot = Vector3.Dot(localDir, Vector3.forward);
            float rightDot = Vector3.Dot(localDir, Vector3.right);

            float upPower = Mathf.Abs(upDot);
            float fwdPower = Mathf.Abs(fwdDot);
            float rightPower = Mathf.Abs(rightDot);

            float maxVal = Mathf.Max(upPower, fwdPower, rightPower);

            return maxVal == upPower ? true : false;
        }

        public bool IsCurrentObject(RaycastHit[] hits, MyObject myObject)
        {
            var curObj = GetCurrentObject(hits, myObject) ?? null;

            if (myObject != curObj || curObj == null)
                return false;

            return true;
        }

        private MyObject GetCurrentObject(RaycastHit[] hits, MyObject myObject)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider.GetComponent<MyObject>() == myObject)
                {
                    return hits[i].collider.GetComponent<MyObject>();
                }
            }
            return null;
        }

        public MyObject GetCurrentObject(GameObject currentObj)
        {
            return currentObj.GetComponent<MyObject>();
        }

        #endregion
        #region Object Move
        public void ObjectMoving(Vector3 pos)
        {
            Vector3 oPos = new Vector3(RoundToNearestGrid(pos.x),
             pos.y,  // RoundToNearestGrid(pos.y),
                  RoundToNearestGrid(pos.z));
            MyObjectPr.SetMove(oPos);
           _uiCanvas.SetEditPanelPos(MyObjectPr.transform);
        }

        private float RoundToNearestGrid(float pos)
        {
            float xDiff = pos % gridsize;
            pos -= xDiff;
            if (xDiff > (gridsize / 2))
            {
                pos += gridsize;
            }
            return pos;
        }

        #endregion
        

        #region Object Data Set Function
        public void SetListData(MyObject mySpaceObject)
        {
            _objDict.Add(mySpaceObject.Index, mySpaceObject);
            AddObjectData(mySpaceObject.ObjectId, mySpaceObject.placedType, mySpaceObject.itemCategoryType,
                    mySpaceObject.Index, mySpaceObject.transform.localPosition,
                    mySpaceObject.transform.localRotation.eulerAngles, mySpaceObject.Name,
                    mySpaceObject.ParentIndex);
        }

        public void SetObjectInit(GameObject obj, ObjectPlacedType types)
        {
            obj.transform.localPosition = _centerTransform.position;
            obj.transform.rotation = Quaternion.identity;
            obj.tag = SpaceConfig.ItemTag;

            MyObjectPr = SetObjectTypes(obj, types);

            MyObjectPr.placedType = types;
        }

        private MyObject SetObjectTypes(GameObject spaceObject, ObjectPlacedType placeType)
        {
            MyObject loadObject = null;

            switch (placeType)
            {
                case ObjectPlacedType.Floor:
                    {
                        loadObject = spaceObject.AddComponent<MyFloorObject>();
                        loadObject.placedType = ObjectPlacedType.Floor;
                    }
                    break;
                case ObjectPlacedType.Prop:
                    {
                        loadObject = spaceObject.AddComponent<MyFloorObject>();
                        loadObject.placedType = ObjectPlacedType.Prop;
                    }
                    break;
                case ObjectPlacedType.Putable:
                    {
                        loadObject = spaceObject.AddComponent<MyFloorObject>();
                        loadObject.placedType = ObjectPlacedType.Putable;
                        loadObject.gameObject.SetLayer(LayerMask.NameToLayer(SpaceConfig.OnObjectLayer));
                    }
                    break;
                case ObjectPlacedType.Wall:
                    {
                        loadObject = spaceObject.AddComponent<MyWallObject>();
                        loadObject.placedType = ObjectPlacedType.Wall;
                    }
                    break;
            }
            return loadObject;
        }

        public void SetCurrentObjectState(MyObject obj)
        {
            if (obj == null)
            {
                if (MyObjectPr == true && MyObjectPr.CanPlaced == true)
                {
                    MyObjectPr.SetPlaced(true);
                    MyObjectPr.SetBaseColor(Color.white);
                    MyObjectPr = null;
                }
            }
            else
            {
                if (MyObjectPr == null)
                {
                    MyObjectPr = obj;
                    MyObjectPr.SetBaseColor(Color.green);
                    MyObjectPr.SetPlaced(false);
                }
                else
                {
                    MyObjectPr.SetPlaced(true);
                    MyObjectPr.SetBaseColor(Color.white);
                    MyObjectPr = null;
                    MyObjectPr = obj;
                    MyObjectPr.SetPlaced(false);
                    MyObjectPr.SetBaseColor(Color.green);
                }
                _uiCanvas.SetEditPanelPos(MyObjectPr.transform);
            }
        }

        public void SetObjectRigidBody(bool isOn)
        {
            if (MyObjectPr == null)
                return;

            MyObjectPr.SetRigidbody(isOn);
        }
        private int GetUniqueID(List<int> uniqueArray)
        {
            int result = 1;

            while (true)
            {
                if (result == SpaceConfig.newObject)
                {
                    ++result;
                    continue;
                }

                if (uniqueArray.Contains(result) == false)
                    return result;

                ++result;
            }

        }

        private void SetParentObject(GameObject g, int pId)
        {
            if (pId == SpaceConfig.newObject)
                return;

            foreach (KeyValuePair<int, MyObject> item in _objDict)
            {
                if (item.Key == pId)
                {
                    g.transform.parent = item.Value.transform;
                    return;
                }
            }

        }
        public void AddObjectData(string id, ObjectPlacedType partType, ItemCategoryType category,
        int uniqueID, Vector3 pos, Vector3 rot, string name, int parentid)
        {
            SpaceObjectData itemObj = new SpaceObjectData(id, (int)partType, (int)category, uniqueID, pos, rot, name, parentid);
            _mySpaceData.spaceObjects.Add(itemObj);
        }

        public void UpdateParentID(int pId)
        {
            int itemindex = _mySpaceData.spaceObjects.FindIndex(x => x.index == MyObjectPr.Index);
            _mySpaceData.spaceObjects[itemindex].UpdateParent(pId);
        }

        public void UpdateTransformObject(Vector3 pos, Vector3 rot)
        {
            int itemindex = _mySpaceData.spaceObjects.FindIndex(x => x.index == MyObjectPr.Index);
            _mySpaceData.spaceObjects[itemindex].UpdateTransform(pos, rot, Vector3.one);
        }

        public void RemoveObject(MySpaceData mySpaceItemClass)
        {
            //todo : 삭제할 때 parent 이하 오브젝트들도 데이터에서 지워야함.
            //toto complete
            if (MyObjectPr == null)
                return;

            if (MyObjectPr.childUniqueID.Count > 0)
            {
                int cnt = MyObjectPr.childUniqueID.Count;
                int objectindex = 0;
                for (int i = 0; i < cnt; i++)
                {
                    objectindex = mySpaceItemClass.spaceObjects.
                        FindIndex(x => x.index == MyObjectPr.childUniqueID[i]);

                    mySpaceItemClass.spaceObjects.RemoveAt(objectindex);

                    _objDict.Remove(MyObjectPr.childUniqueID[i]);
                }
            }

            int itemindex = mySpaceItemClass.spaceObjects.FindIndex(x => x.index == MyObjectPr.Index);

            mySpaceItemClass.spaceObjects.RemoveAt(itemindex);

            _objDict.Remove(MyObjectPr.Index);
        }

        #endregion



    }

}
