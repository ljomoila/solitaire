using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PoolSystem : MonoBehaviour
{
    static public GameObject GetNextObject(GameObject sourceObj, bool activateObject = true)
    {
        int uniqueId = sourceObj.GetInstanceID();

        if (!instance.poolCursors.ContainsKey(uniqueId))
        {
            Debug.LogError(
                "[CFX_SpawnSystem.GetNextPoolObject()] Object hasn't been preloaded: "
                    + sourceObj.name
                    + " (ID:"
                    + uniqueId
                    + ")"
            );
            return null;
        }

        int cursor = instance.poolCursors[uniqueId];
        instance.poolCursors[uniqueId]++;

        if (instance.poolCursors[uniqueId] >= instance.instantiatedObjects[uniqueId].Count)
        {
            instance.poolCursors[uniqueId] = 0;
        }

        GameObject returnObj = instance.instantiatedObjects[uniqueId][cursor];

        //Debug.Log(activateObject+" "+uniqueId+" "+cursor);

        if (activateObject && returnObj != null)
            returnObj.SetActive(true);

        return returnObj;
    }

    static public void PreloadObject(GameObject sourceObj, int poolSize = 1)
    {
        instance.addObjectToPool(sourceObj, poolSize);
    }

    static public void UnloadObjects(GameObject sourceObj)
    {
        instance.removeObjectsFromPool(sourceObj);
    }

    static public bool AllObjectsLoaded
    {
        get { return instance.allObjectsLoaded; }
    }

    // INTERNAL SYSTEM ----------------------------------------------------------------------------------------------------------------------------------------

    static public PoolSystem instance;

    public GameObject[] objectsToPreload = new GameObject[0];
    public int[] objectsToPreloadTimes = new int[0];
    public bool hideObjectsInHierarchy;

    private bool allObjectsLoaded;
    private Dictionary<int, List<GameObject>> instantiatedObjects =
        new Dictionary<int, List<GameObject>>();
    private Dictionary<int, int> poolCursors = new Dictionary<int, int>();

    private void addObjectToPool(GameObject sourceObject, int number)
    {
        int uniqueId = sourceObject.GetInstanceID();

        // Add new entry if it doesn't exist
        if (!instantiatedObjects.ContainsKey(uniqueId))
        {
            instantiatedObjects.Add(uniqueId, new List<GameObject>());
            poolCursors.Add(uniqueId, 0);
        }

        // Add the new objects
        GameObject newObj;
        for (int i = 0; i < number; i++)
        {
            newObj = (GameObject)Instantiate(sourceObject);
            newObj.transform.parent = poolParent.transform;
            newObj.SetActive(false);

            // Set flag to not destruct object
            CFX_AutoDestructShuriken[] autoDestruct =
                newObj.GetComponentsInChildren<CFX_AutoDestructShuriken>(true);
            foreach (CFX_AutoDestructShuriken ad in autoDestruct)
            {
                ad.OnlyDeactivate = true;
            }
            // Set flag to not destruct light
            CFX_LightIntensityFade[] lightIntensity =
                newObj.GetComponentsInChildren<CFX_LightIntensityFade>(true);
            foreach (CFX_LightIntensityFade li in lightIntensity)
            {
                li.autodestruct = false;
            }

            instantiatedObjects[uniqueId].Add(newObj);

            if (hideObjectsInHierarchy)
                newObj.hideFlags = HideFlags.HideInHierarchy;
        }
    }

    private void removeObjectsFromPool(GameObject sourceObject)
    {
        int uniqueId = sourceObject.GetInstanceID();

        if (!instantiatedObjects.ContainsKey(uniqueId))
        {
            Debug.LogWarning(
                "[CFX_SpawnSystem.removeObjectsFromPool()] There aren't any preloaded object for: "
                    + sourceObject.name
                    + " (ID:"
                    + uniqueId
                    + ")"
            );
            return;
        }

        // Destroy all objects
        for (int i = instantiatedObjects[uniqueId].Count - 1; i >= 0; i--)
        {
            GameObject obj = instantiatedObjects[uniqueId][i];
            instantiatedObjects[uniqueId].RemoveAt(i);
            GameObject.Destroy(obj);
        }

        // Remove pool entry
        instantiatedObjects.Remove(uniqueId);
        poolCursors.Remove(uniqueId);
    }

    void Awake()
    {
        if (instance != null)
            Debug.LogWarning(
                "CFX_SpawnSystem: There should only be one instance of CFX_SpawnSystem per Scene!"
            );

        instance = this;
    }

    GameObject poolParent = null;

    void Start()
    {
        StartCoroutine(Initialize());
    }

    public IEnumerator Initialize()
    {
        allObjectsLoaded = false;

        if (poolParent != null)
        {
            for (int i = 0; i < objectsToPreload.Length; i++)
            {
                UnloadObjects(objectsToPreload[i]);
            }

            Destroy(poolParent);

            instantiatedObjects.Clear();
        }

        yield return null;

        poolParent = new GameObject("PoolParent");

        for (int i = 0; i < objectsToPreload.Length; i++)
        {
            PreloadObject(objectsToPreload[i], objectsToPreloadTimes[i]);
        }

        yield return null;

        allObjectsLoaded = true;
    }
}
