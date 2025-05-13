using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Monotone.Utility
{
    public static class PrefabLibrary
    {
        public static Dictionary<string, GameObject> library;

        public static GameObject Get(string name)
        {
            CheckLibraryLoaded();
            if (library.ContainsKey(name)) return library[name];
            Debug.LogError($"Prefab not found:{name}");
            return null;
        }

        public static void LoadLibrary()
        {
            library = new();
            GameObject[] allPrefabs = Resources.LoadAll<GameObject>("");
            foreach (GameObject prefab in allPrefabs)
            {
                if (!library.ContainsKey(prefab.name))
                {
                    library.Add(prefab.name, prefab);
                }
                else
                {
                    Debug.LogWarning($"Duplicate prefab name found: {prefab.name}");
                }
            }

            Debug.Log($"Loaded {library.Keys.Count} prefabs");
        }

        private static void CheckLibraryLoaded()
        {
            if (library == null) { LoadLibrary(); return; }
            if (library.Count >= 0) return;
            LoadLibrary();
        }
    }

    public class PrefabPool<T> where T : Component
    {
        private Queue<T> pool;

        private GameObject prefab;
        private string prefabName;

        public PrefabPool(string inPrefabName)
        {
            pool = new();
            prefabName = inPrefabName;
        }

        public PrefabPool(GameObject prefab)
        {
            pool = new();
            this.prefab = prefab;
        }

        public T Get()
        {
            if (pool.Count == 0) CreateElement();

            T instance = pool.Dequeue();

            if (instance == null)
            {
                RemoveNull();
                return Get();
            }

            instance.gameObject.SetActive(true);
            return instance;
        }
        public void Release(T item)
        {
            if (item == null) return;

            item.transform.SetParent(null);
            item.gameObject.SetActive(false);

            pool.Enqueue(item);
        }
        private void CreateElement()
        {
            if (prefab == null)
            {
                prefab = PrefabLibrary.Get(prefabName);
            }
            if (UnityEngine.Object.Instantiate(prefab).TryGetComponent(out T component))
            {
                Release(component);
            }
            else
            {
                Debug.LogError($"Prefab {prefab.name} does not have component:{nameof(T)}");
            }
        }

        private void RemoveNull()
        {
            pool = new(pool.Where(item => item != null));
        }
    }

    public class PrefabPool
    {
        private Queue<GameObject> pool;
        private GameObject poolElement;
        private string prefabName;

        public PrefabPool(string inPrefabName)
        {
            pool = new();
            prefabName = inPrefabName;
        }

        public PrefabPool(GameObject prefab)
        {
            pool = new();
            poolElement = prefab;
        }

        public GameObject Get()
        {
            if (pool.Count == 0) CreateElement();
            GameObject element = pool.Dequeue();
            if (element == null)
            {
                RemoveNull();
                return Get();
            }
            element.SetActive(true);
            return element;
        }
        public void Release(GameObject item)
        {
            if (item == null) return;
            item.transform.SetParent(null);
            item.SetActive(false);
            pool.Enqueue(item);
        }
        private void CreateElement()
        {
            if (poolElement == null) poolElement = PrefabLibrary.Get(prefabName);
            Release(UnityEngine.Object.Instantiate(poolElement));
        }
        private void RemoveNull() => pool = new(pool.Where(item => item != null));
    }
}