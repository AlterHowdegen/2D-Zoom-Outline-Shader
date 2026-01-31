using System.Collections.Generic;
using UnityEngine;

public class RandomRotator : MonoBehaviour
{
    public List<GameObject> gameObjects = new List<GameObject>();
    public float rotationDuration;
    private List<Vector3> rotations = new List<Vector3>();
    void Start()
    {
        for (int i = 0; i < gameObjects.Count; i++)
        {
            rotations.Add(gameObjects[i].transform.localRotation.eulerAngles);
        }
        
        Rotate();
    }

    private void Rotate()
    {
        int randomIndex = Random.Range(0, gameObjects.Count);
        GameObject randomGameObject = gameObjects[randomIndex];

        Vector3 randomRotation = rotations[randomIndex];

        int randomAxis = Random.Range(0, 2);

        float direction;
        switch (randomAxis)
        {
            case 0:
                direction = Random.value;
                randomRotation.x += direction > 0.5f ? -90f : 90f;
                randomRotation.x = ClampRotation(randomRotation.x);
                break;
            case 1:
                direction = Random.value;
                randomRotation.y += direction > 0.5f ? -90f : 90f;
                randomRotation.y = ClampRotation(randomRotation.y);
                break;
            case 2:
                direction = Random.value;
                randomRotation.z += direction > 0.5f ? -90f : 90f;
                randomRotation.z = ClampRotation(randomRotation.z);
                break;
            default:
                break;
        }

        rotations[randomIndex] = randomRotation;

        LeanTween.rotateLocal(randomGameObject, rotations[randomIndex], rotationDuration)
            .setEaseInOutCubic()
            .setOnComplete(Rotate);
    }

    private float ClampRotation(float value)
    {
        if (value > 360f)
        {
            return -270f;
        }

        if (value < -360f)
        {
            return 270f;
        }

        return value;
    }
}
