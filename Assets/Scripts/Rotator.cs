using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class Rotator : MonoBehaviour
{
    public List<GameObject> gameObjects = new List<GameObject>();
    public float rotationDuration;
    public float scaleSmall;
    public float scaleBig;

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

    [ContextMenu("Resize Small")]
    public void ResizeAllSmall()
    {
        for (int i = 0; i < gameObjects.Count; i++)
        {
            GameObject go = gameObjects[i];
            Resize(go, scaleSmall);
        }
    }

    [ContextMenu("Resize Big")]
    public void ResizeAllBig()
    {
        for (int i = 0; i < gameObjects.Count; i++)
        {
            GameObject go = gameObjects[i];
            Resize(go, scaleBig);
        }
    }

    public void Resize(GameObject go, float scale)
    {
        LeanTween.scale(go, Vector3.one * scale, rotationDuration)
            .setEaseInOutCubic();
    }
}
