using UnityEngine;

namespace dduR{

    public class MyFloorObject : MyObject
    {
        private bool IsPutable(GameObject g)
        {

            return false;
        }

        protected override void OnTriggerEnter(Collider other)
        {
            if (Placed == true)
                return;

            if (other.CompareTag(SpaceConfig.ItemTag) || other.CompareTag(SpaceConfig.WallTag))
            {
                CanPlaced = false;
            }
            else
            {
                CanPlaced = true;
            }
        }

        protected override void OnTriggerStay(Collider other)
        {
            if (Placed == true)
                return;

            if (other.CompareTag(SpaceConfig.ItemTag) || other.CompareTag(SpaceConfig.WallTag))
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
            else if (other.CompareTag(SpaceConfig.WallTag))
            {
                CanPlaced = true;
            }
        }
    }

}