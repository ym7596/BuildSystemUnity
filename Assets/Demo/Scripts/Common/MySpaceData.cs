using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace dduR{

   
        [Serializable]
        public class MySpaceData
        {
            public List<SpaceObjectData> spaceObjects = new List<SpaceObjectData>();

            public MySpaceData Copy()
            {
                var clone = new MySpaceData();
                clone.spaceObjects = spaceObjects.ToList();
                return clone;
            }
        }

        [Serializable]
        public class SpaceObjectData
        {
            public int index = 0;
            public int partType = 0;
            public int category = 0;
            public int objectParentId = 0;
            public string objectId = "";
            public string name = "";
            public string id = "";

            public Vector3 position = Vector3.zero;
            public Vector3 rotation = Vector3.zero;
            public Vector3 scale = Vector3.zero;

            public SpaceObjectData(string objectId, int partType, int category, int index, Vector3 pos, Vector3 rot,
                string name, int parent)
            {
                this.objectId = objectId;
                this.partType = partType;
                this.category = category;
                this.index = index;
                this.position = pos;
                this.rotation = rot;
                this.name = name;
                this.objectParentId = parent;
            }

            public void UpdateTransform(Vector3 pos, Vector3 rot, Vector3 sc)
            {
                position = pos;
                rotation = rot;
                scale = sc;
            }

            public void UpdateParent(int parentId)
            {
                this.objectParentId = parentId;
            }
        }
}

