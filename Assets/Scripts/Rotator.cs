using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    public List<GameObject> gameObjects = new List<GameObject>();
    public List<GameObject> sprites = new List<GameObject>();
    public float rotationDuration;
    public float scaleSmall;
    public float scaleBig;
    public Vector3 defaultRotation;
    public Vector3 turntableStartRotation;
    private bool turntableRunning;
    private bool resizedBig;
    private bool isBarrelRolling;
    private bool isPositionedInRow;
    public Vector3 rowStartingPosition;
    public Vector3 gridStartingPosition;

    void Start()
    {
        for (int i = 0; i < gameObjects.Count; i++)
        {
            GameObject go = gameObjects[i];
            go.transform.localScale = Vector3.one * scaleSmall;
        }
    }

    [ContextMenu("Barrel Roll")]
    public void BarrelRollAll()
    {
        if (isBarrelRolling)
        {
            return;
        }

        isBarrelRolling = true;
        for (int i = 0; i < gameObjects.Count; i++)
        {
            GameObject go = gameObjects[i];
            BarrelRoll(go);
        }
    }

    public void BarrelRoll(GameObject go)
    {
        LeanTween.rotateAroundLocal(go, Vector3.right, 360f, rotationDuration)
            .setEaseInOutCubic()
            .setOnComplete(() => isBarrelRolling = false);
    }

    [ContextMenu("Resize")]
    public void ResizeAll()
    {
        if (resizedBig)
        {
            for (int i = 0; i < gameObjects.Count; i++)
            {
                resizedBig = false;
                GameObject go = gameObjects[i];
                Resize(go, scaleSmall);
            }
            return;
        }

        for (int i = 0; i < gameObjects.Count; i++)
        {
            resizedBig = true;
            GameObject go = gameObjects[i];
            Resize(go, scaleBig);
        }
    }

    public void Resize(GameObject go, float scale)
    {
        LeanTween.scale(go, Vector3.one * scale, rotationDuration)
            .setEaseInOutCubic();
    }

    [ContextMenu("Turntable")]
    public void TurntableAll()
    {
        LeanTween.cancelAll();
        if (turntableRunning)
        {
            turntableRunning = false;
            for (int i = 0; i < gameObjects.Count; i++)
            {
                GameObject go = gameObjects[i];
                DefaultPosition(go);
            }
            return;
        }

        for (int i = 0; i < gameObjects.Count; i++)
        {
            turntableRunning = true;
            GameObject go = gameObjects[i];
            TurntablePre(go);
        }
    }

    private void DefaultPosition(GameObject go)
    {
        LeanTween.rotateLocal(go, defaultRotation, rotationDuration / 2f)
            .setEaseOutCubic();
    }

    public void TurntablePre(GameObject go)
    {
        LeanTween.rotateLocal(go, turntableStartRotation, rotationDuration / 2f)
            .setEaseInCubic()
            .setOnComplete(() => TurntableStart(go));
    }

    private void TurntableStart(GameObject go)
    {
        LeanTween.rotateAround(go, Vector3.up, 360f, rotationDuration * 2f)
            .setOnComplete(() => TurntableLoop(go));
    }

    private void TurntableLoop(GameObject go)
    {
        LeanTween.rotateAround(go, Vector3.up, 360f, rotationDuration * 2f)
            .setLoopCount(0);
    }

    public void Positioning()
    {
        if (isPositionedInRow)
        {
            isPositionedInRow = false;
            PositionInGrid();
            return;
        }

        isPositionedInRow = true;
        PositionInRow();
    }

    private void PositionInRow()
    {
        Vector3 position = rowStartingPosition;
        for (int i = 0; i < sprites.Count; i++)
        {
            GameObject sprite = sprites[i];
            LeanTween.moveLocal(sprite, position, rotationDuration)
                .setEaseInOutCubic();
            position.x++;
        }
    }

    private void PositionInGrid()
    {
        Vector3 position = gridStartingPosition;
        for (int i = 0; i < sprites.Count; i++)
        {
            GameObject sprite = sprites[i];

            LeanTween.moveLocal(sprite, position, rotationDuration)
                .setEaseInOutCubic();
            position.x++;
            if (position.x > gridStartingPosition.x + 1f)
            {
                position.x = gridStartingPosition.x;
                position.y -= 0.75f;
            }
        }
    }
}
