using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class Rotator : MonoBehaviour
{
    public List<GameObject> gameObjects = new List<GameObject>();
    public float rotationDuration;
    void Start()
    {

    }

    [ContextMenu("Barrel Roll")]
    public void BarrelRollAll()
    {
        for (int i = 0; i < gameObjects.Count; i++)
        {
            GameObject go = gameObjects[i];
            BarrelRoll(go);
        }
    }

    public void BarrelRoll(GameObject go)
    {
        LeanTween.rotateAroundLocal(go, Vector3.right, 360f, rotationDuration)
            .setEaseInOutCubic();
    }
}
