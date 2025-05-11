using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon;

namespace Watermelon.JellyMerge
{
    public class CellsPool : MonoBehaviour
    {
        public static CellsPool instance;
        public static Vector3 defaultPosition = new Vector3(-30f, 0f, 0f);


        private List<GameObject> objects = new List<GameObject>();

        private void Awake()
        {
            instance = this;
        }

        public GameObject GetPooledObject(Vector3 position)
        {
            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i].transform.position.x == -30)
                {
                    objects[i].transform.position = position;
                    return objects[i];
                }
            }

            return objects[objects.Count - 1];
        }

        public void ReturnToPoolEverything()
        {
            Debug.Log("Returm " + objects.Count);
            for (int i = 0; i < objects.Count; i++)
            {
                objects[i].transform.position = defaultPosition;
            }
        }
    }
}