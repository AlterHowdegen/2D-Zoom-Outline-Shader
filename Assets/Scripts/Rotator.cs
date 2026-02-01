using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

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
    public Camera cameraColor;
    public Camera cameraShading;
    public List<int> initialAnimationFramerates = new List<int>();
    public int targetFrameCountForCurrentAnimationFramerate;
    private int currentAnimationFrame;
    public int currentAnimationFramerateIndexIndex;
    public TextMeshPro statusLabelTop;
    public TextMeshPro statusLabelBottom;
    private List<float> previousPositions = new List<float>();
    private List<float> interpolatedVelocities = new List<float>();
    public float bankingSpeed = 1f;

    private void Start()
    {
        Application.targetFrameRate = 60;
        cameraColor.enabled = false;
        cameraShading.enabled = false;
        PositionInGridNow();
        RequestManualRender(cameraColor);
        RequestManualRender(cameraShading);

        targetFrameCountForCurrentAnimationFramerate = Application.targetFrameRate / initialAnimationFramerates[0];
        meshRenderers = new List<MeshRenderer>();
        for (int i = 0; i < gameObjects.Count; i++)
        {
            GameObject go = gameObjects[i];
            go.transform.localScale = Vector3.one * scaleBig;
            GameObject sprite = sprites[i];
            meshRenderers.Add(sprite.GetComponent<MeshRenderer>());
            previousPositions.Add(go.transform.localPosition.y);
            interpolatedVelocities.Add(0f);
        }
        UpdateFpsDisplay();
    }

    private void Update()
    {
        currentAnimationFrame++;

        if (currentAnimationFrame >= targetFrameCountForCurrentAnimationFramerate)
        {
            RequestManualRender(cameraColor);
            RequestManualRender(cameraShading);

            currentAnimationFrame = 0;
        }

        Bank();
    }

    private void Bank()
    {
        for (int i = 0; i < sprites.Count; i++)
        {
            GameObject sprite = sprites[i];
            GameObject go = gameObjects[i];
            float previousPosition = previousPositions[i];

            float velocity = sprite.transform.localPosition.y - previousPosition;

            interpolatedVelocities[i] = Mathf.Lerp(interpolatedVelocities[i], velocity, Time.fixedDeltaTime);

            Vector3 localEulerAngles = go.transform.localEulerAngles;
            localEulerAngles.x = interpolatedVelocities[i] * bankingSpeed;

            go.transform.localEulerAngles = localEulerAngles;
            // Debug.Log(localEulerAngles.x);

            previousPositions[i] = sprite.transform.localPosition.y;
        }
    }

    private void RequestManualRender(Camera camera)
    {
        // if (!camera.targetTexture.IsCreated())
        // {
        //     camera.targetTexture.Create();
        // }

        RenderPipeline.StandardRequest request = new RenderPipeline.StandardRequest();
        // Render to a 2D texture
        request.destination = camera.targetTexture;

        // Render the camera, and fill the 2D texture with its view
        camera.SubmitRenderRequest(request);
    }

    [ContextMenu("Barrel Roll")]
    public void BarrelRollAll()
    {
        if (isBarrelRolling)
        {
            return;
        }

        SetNotification("Do a", "Barrel Roll");

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
            SetNotification("Resize:", "Smol");
            for (int i = 0; i < gameObjects.Count; i++)
            {
                resizedBig = false;
                GameObject go = gameObjects[i];
                Resize(go, scaleSmall, i);
            }
            return;
        }

        SetNotification("Resize:", "Big");
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
            SetNotification("Turntable:", "Stop");
            turntableRunning = false;
            for (int i = 0; i < gameObjects.Count; i++)
            {
                GameObject go = gameObjects[i];
                DefaultPosition(go);
            }
            return;
        }

        SetNotification("Turntable:", "Activate");
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
        SetNotification("Position:", "Row");
        Vector3 position = rowStartingPosition;
        for (int i = 0; i < sprites.Count; i++)
        {
            GameObject sprite = sprites[i];
            LeanTween.moveLocal(sprite, position, rotationDuration)
                .setEaseInOutCubic()
                .setDelay(i * delayDuration);
            position.x += 0.9f;
        }
    }

    private void PositionInGrid()
    {
        SetNotification("Position:", "Grid");
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

    private void PositionInGridNow()
    {
        Vector3 position = gridStartingPosition;
        for (int i = 0; i < sprites.Count; i++)
        {
            GameObject sprite = sprites[i];
            sprite.transform.localPosition = position;
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
        SetNotification("Flash", "Flash");
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
        meshRenderer.material.SetColor("_Inner_Color", color);
    }

    public void SetRenderSkip()
    {
        currentAnimationFramerateIndexIndex++;
        if (currentAnimationFramerateIndexIndex >= initialAnimationFramerates.Count)
        {
            currentAnimationFramerateIndexIndex = 0;
        }

        targetFrameCountForCurrentAnimationFramerate = Application.targetFrameRate / initialAnimationFramerates[currentAnimationFramerateIndexIndex];

        UpdateFpsDisplay();
    }

    private void UpdateFpsDisplay()
    {
        SetNotification($"Animation FPS: {initialAnimationFramerates[currentAnimationFramerateIndexIndex]}", $"Animate every: {targetFrameCountForCurrentAnimationFramerate}");
    }

    private void SetNotification(string top, string bottom)
    {
        LeanTween.cancel(statusLabelTop.gameObject);

        SetNotificationAlpha(1f);
        statusLabelTop.SetText(top);
        statusLabelBottom.SetText(bottom);

        FadeOut();
    }

    private void FadeOut()
    {
        LeanTween.value(statusLabelTop.gameObject, statusLabelTop.alpha, 0f, rotationDuration)
            .setOnUpdate(SetNotificationAlpha)
            .setDelay(delayDuration);
    }

    private void SetNotificationAlpha(float value)
    {
        statusLabelTop.alpha = value;
        statusLabelBottom.alpha = value;
    }
}
