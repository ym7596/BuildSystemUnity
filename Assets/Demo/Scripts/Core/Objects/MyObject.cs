using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace dduR
{
    public abstract class MyObject : MonoBehaviour, IMyObject
    {
        protected bool _canPlaced = true;
        protected BoxCollider _boxCollider;
        protected Coroutine _rotationCo;

        public bool CanPlaced { get => _canPlaced;
            set {
                _canPlaced = value;
                SetBaseColor(_canPlaced ? Color.green : Color.red);
            }
        }
        public bool Placed { get; set; }

        public ObjectPlacedType placedType = ObjectPlacedType.Floor;
        public ItemCategoryType itemCategoryType = ItemCategoryType.Bed;

        public List<int> childUniqueID = new List<int>();

        public bool isOnObject = false;
        public string networkId = "";

        private Rigidbody _rigidBody;

        public int Index { get; private set; } = SpaceConfig.newObject;
        public string Name { get; private set; } = "";
        public string ObjectId { get; private set; }
        public int ParentIndex { get; private set; } = SpaceConfig.newObject;
        public Material material { get; private set; }

        protected abstract void OnTriggerEnter(Collider other);
        protected abstract void OnTriggerStay(Collider other);
        protected abstract void OnTriggerExit(Collider other);

        protected void Awake()
        {
            InitObject();
        }

        private void Start()
        {
            _rigidBody = gameObject.AddComponent<Rigidbody>();
            _rigidBody.useGravity = false;
        }


        protected virtual void OnDestroy()
        {
            if (_rotationCo != null)
                StopCoroutine(_rotationCo);
        }

        public virtual void InitObject()
        {
            _boxCollider = GetComponent<BoxCollider>();
            material = transform.GetComponentInChildren<MeshRenderer>().material;

            //Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
            //material.shader = urpShader;
        }

        public void SetRigidbody(bool isOn)
        {
            if (_rigidBody == null)
                return;

            _rigidBody.isKinematic = !isOn;
        }

        public void SetBaseColor(Color color)
        {
            material.SetColor("_BaseColor", color);
        }

        public void SetMove(Vector3 pos)
        {
            transform.position = pos;
        }

        public void SetName(string name)
        {

            Name = name;
        }

        public void SetPlaced(bool isOn)
        {
            Placed = isOn;
        }

        public virtual void SetRotate(int value)
        {
            if (_rotationCo != null)
                return;

            _rotationCo = StartCoroutine(TriggerCheckCo(value));
        }

        public void SetUniqueID(int id)
        {
            Index = id;
        }

        public void SetParentId(int index)
        {
            ParentIndex = index;
        }

        public void SetAddChildObject(int uniqueID)
        {
            if (childUniqueID.Contains(uniqueID) == true)
                return;

            childUniqueID.Add(uniqueID);
        }

        public void SetRemoveChildObject(int uniqueID)
        {
            if (childUniqueID.Contains(uniqueID))
            {
                childUniqueID.Remove(uniqueID);
            }

        }

        protected virtual IEnumerator TriggerCheckCo(int value)
        {
            SetRigidbody(true);

            transform.Rotate(Vector3.up, value);
            yield return new WaitForSeconds(0.1f);

            SetRigidbody(false);
            yield return new WaitForSeconds(0.1f);

            if (CanPlaced == true)
                Placed = true;

            _rotationCo = null;
        }

        public virtual void SettingAnim(Vector3 pos)
        {
            transform.DOMove(pos, 0.5f).SetEase(Ease.OutBounce).SetLoops(1).
                From(new Vector3(pos.x, pos.y + .5f, pos.z));
        }


    }

}