using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    public List<GameObject> gameObjects = new List<GameObject>();
    public List<GameObject> sprites = new List<GameObject>();
    public List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
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
    public float delayDuration = 0.1f;
    public Color flashColor;

    void Start()
    {
        meshRenderers = new List<MeshRenderer>();
        for (int i = 0; i < gameObjects.Count; i++)
        {
            GameObject go = gameObjects[i];
            go.transform.localScale = Vector3.one * scaleSmall;
            GameObject sprite = sprites[i];
            meshRenderers.Add(sprite.GetComponent<MeshRenderer>());
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
            BarrelRoll(go, i);
        }
    }

    public void BarrelRoll(GameObject go, int i)
    {
        float randomDirection = UnityEngine.Random.value;
        float direction = randomDirection > 0.5f ? 360f : -360f;
        LeanTween.rotateAroundLocal(go, Vector3.right, direction, rotationDuration)
            .setEaseInOutCubic()
            .setOnComplete(() => isBarrelRolling = false)
            .setDelay(i * delayDuration);
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
                Resize(go, scaleSmall, i);
            }
            return;
        }

        for (int i = 0; i < gameObjects.Count; i++)
        {
            resizedBig = true;
            GameObject go = gameObjects[i];
            Resize(go, scaleBig, i);
        }
    }

    public void Resize(GameObject go, float scale, int i)
    {
        LeanTween.scale(go, Vector3.one * scale, rotationDuration)
            .setEaseInOutCubic()
            .setDelay(i * delayDuration);
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
                .setEaseInOutCubic()
                .setDelay(i * delayDuration);
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
                .setEaseInOutCubic()
                .setDelay(i * delayDuration);
            position.x++;
            if (position.x > gridStartingPosition.x + 1f)
            {
                position.x = gridStartingPosition.x;
                position.y -= 0.75f;
            }
        }
    }

    public void FlashAll()
    {
        for (int i = 0; i < meshRenderers.Count; i++)
        {
            MeshRenderer meshRenderer = meshRenderers[i];
            LeanTween.value(meshRenderer.gameObject, Color.black, flashColor, rotationDuration / 4f)
                .setOnUpdate((Color color) => UpdateColor(meshRenderer, color))
                .setLoopPingPong(1)
                .setDelay(i * delayDuration);
        }
    }

    private void UpdateColor(MeshRenderer meshRenderer, Color color)
    {
        meshRenderer.material.SetColor("_Outline_Color", color);
    }
}
