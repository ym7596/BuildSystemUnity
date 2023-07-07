using System.Collections;
using UnityEngine;
using DG.Tweening;

namespace dduR
{
    public class MyWallObject : MyObject
    {
        private Vector3[] _vertices = new Vector3[] { };
        public override void SetRotate(int value)
        {

        }

        protected override IEnumerator TriggerCheckCo(int value)
        {
            yield return null;
            
/*            MySpaceEditorManager.Instance.SetObjectRigidBody(true);
            yield return new WaitForSeconds(0.1f);

            //transform.Rotate(Vector3.up, value);
            yield return new WaitForSeconds(0.1f);

            MySpaceEditorManager.Instance.SetObjectRigidBody(false);
            yield return new WaitForSeconds(0.1f);*/
           
        }

        public override void SettingAnim(Vector3 pos)
        {
            transform.DOMove(pos, 0.5f).SetEase(Ease.OutBounce).SetLoops(1).From(new Vector3(pos.x, pos.y, pos.z - .5f));
        }

        public void GetCurrentVertices()
        {
            _vertices = GetVertexPos(this.gameObject);
        }

        protected  override void OnTriggerEnter(Collider other)
        {
            if (Placed == true)
                return;

            if (other.CompareTag(SpaceConfig.ItemTag))
            {
                CanPlaced = false;
                return;
            }
         
            GetCurrentVertices();

            if (other.CompareTag(SpaceConfig.WallTag) 
                && IsVerticesContain(other, GetVertexPos(gameObject)))
            {
                CanPlaced = true;
            }

            else
            {
                CanPlaced = false;
            }
        }
        protected override void OnTriggerStay(Collider other)
        {
            if (Placed == true)
                return;

            if (other.CompareTag(SpaceConfig.ItemTag))
            {
                CanPlaced = false;
                return;
            }

            GetCurrentVertices();



            if (other.CompareTag(SpaceConfig.WallTag)
                && IsVerticesContain(other, GetVertexPos(gameObject)))
            {
                CanPlaced = true;
            }

            else
            {
                CanPlaced = false;
            }


        }

        protected override void OnTriggerExit(Collider other)
        {
            if (Placed == true)
                return;

            if (other.CompareTag(SpaceConfig.ItemTag))
            {
                CanPlaced = true;
            }
        }



        Vector3[] GetVertexPos(GameObject g)
        {
            //vertex 위치 : z축(앞) 0 1 4 5  뒤 2 3 6 7
            Vector3[] vertices = new Vector3[8];
            Matrix4x4 thisMatrix = g.transform.localToWorldMatrix;
            Quaternion storedRotation = g.transform.rotation;
            g.transform.rotation = Quaternion.identity;

            var extents = _boxCollider.bounds.extents;
            vertices[0] = thisMatrix.MultiplyPoint3x4(extents);
            vertices[1] = thisMatrix.MultiplyPoint3x4(new Vector3(-extents.x, extents.y, extents.z));
            vertices[2] = thisMatrix.MultiplyPoint3x4(new Vector3(extents.x, extents.y, -extents.z));
            vertices[3] = thisMatrix.MultiplyPoint3x4(new Vector3(-extents.x, extents.y, -extents.z));
            vertices[4] = thisMatrix.MultiplyPoint3x4(new Vector3(extents.x, -extents.y, extents.z));
            vertices[5] = thisMatrix.MultiplyPoint3x4(new Vector3(-extents.x, -extents.y, extents.z));
            vertices[6] = thisMatrix.MultiplyPoint3x4(new Vector3(extents.x, -extents.y, -extents.z));
            vertices[7] = thisMatrix.MultiplyPoint3x4(-extents);

            g.transform.rotation = storedRotation;
            return vertices;
        }

        private bool IsVerticesContain(Collider other, Vector3[] vertices)
        {
            BoxCollider boxCol = other.GetComponent<BoxCollider>();
            /*for (int i = 0; i < vertices.Length; i++)
            {
                Debug.Log($"bound : {boxCol.bounds}");

                if (boxCol.bounds.Contains(_vertices[i]))
                {
                    Debug.Log($"{i} is Contains!");
                }


            }*/
            Debug.Log((int)other.transform.parent.localRotation.y);
            //오브젝트 콜라이더의 앞부분(닿는 부분)이 벽과 마주쳤을때..
            /*    int cnt = 0;
                bool isContains = false;
                foreach(var vtx in vertices)
                {
                    if (boxCol.bounds.Contains(vtx))
                    {
                        cnt++;
                    }

                    if (cnt == 4)
                    {
                        isContains = true;
                        Debug.Log("success");
                        break;
                    }
                }*/

            if (boxCol.bounds.Contains(_vertices[0]) &&
                boxCol.bounds.Contains(_vertices[1]) &&
                boxCol.bounds.Contains(_vertices[4]) &&
                boxCol.bounds.Contains(_vertices[5])
                )
            {
                return true;
            }
            return false;

           /* Debug.Log(cnt);
            Debug.Log(other.name);
            return isContains;*/
        }
    }

}