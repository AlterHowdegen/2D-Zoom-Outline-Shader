using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class Rotator : MonoBehaviour
{
    public List<Ship> ships = new List<Ship>();
    internal List<Ship> shipInstances = new List<Ship>();

    public List<GameObject> gameObjects = new List<GameObject>();
    public float rotationDuration;
    public float scaleSmall;
    public float scaleBig;
    public Vector3 defaultRotation;
    public Vector3 turntableStartRotation;
    private bool turntableRunning;
    private bool resizedBig;
    private bool isBarrelRolling;
    private bool isPositionedInRow = true;
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
    public MeshRenderer meshRendererFlagship;
    public Color flashColorEnemy;
    public RenderTexture renderTextureColor;
    public RenderTexture renderTextureShading;
    private int renderTextureStep = 64;
    private Vector2Int renderTextureResolution;
    private Vector2 tiling = Vector2.one;
    private Vector2 offset = Vector2.zero;
    public Vector3 spritesStartingPosition;
    private int previousRandomIndex = -1;
    public Vector3 shipSpawnPosition;

    private void Start()
    {
        Application.targetFrameRate = 60;
        cameraColor.enabled = false;
        cameraShading.enabled = false;
        RequestManualRender(cameraColor);
        RequestManualRender(cameraShading);

        targetFrameCountForCurrentAnimationFramerate = Application.targetFrameRate / initialAnimationFramerates[0];
        for (int i = 0; i < gameObjects.Count; i++)
        {
            GameObject go = gameObjects[i];
            go.transform.localScale = Vector3.one * scaleSmall;
            previousPositions.Add(go.transform.localPosition.y);
            interpolatedVelocities.Add(0f);
        }

        PositionInRow(false);
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

        MoveShips();
        Bank();
    }

    private void MoveShips()
    {
        for (int i = 0; i < shipInstances.Count; i++)
        {
            Ship ship = shipInstances[i];
            ship.sprite.transform.localPosition = Vector3.MoveTowards(ship.sprite.transform.localPosition, ship.targetPosition, Time.deltaTime * ship.speed);
        }
    }

    public void SpawnShip()
    {
        // Instantiate objects

        List<int> randomIndices = new List<int>();

        for (int i = 0; i < ships.Count; i++)
        {
            if (i == previousRandomIndex)
            {
                continue;
            }
            randomIndices.Add(i);
        }

        int randomIndex = UnityEngine.Random.Range(0, randomIndices.Count);
        randomIndex = randomIndices[randomIndex];

        previousRandomIndex = randomIndex;

        Ship ship = Instantiate(ships[randomIndex]);
        GameObject model = Instantiate(ship.modelPrefab.gameObject, Vector3.zero, Quaternion.identity);
        GameObject sprite = Instantiate(ship.spritePrefab.gameObject, Vector3.zero, Quaternion.identity);
        ship.model = model.GetComponent<Transform>();
        ship.sprite = sprite.GetComponent<MeshRenderer>();
        gameObjects.Add(model);
        shipInstances.Add(ship);
        previousPositions.Add(sprite.transform.localPosition.y);
        interpolatedVelocities.Add(0f);
        ship.sprite.transform.localPosition = shipSpawnPosition;

        ResizeRenderTextures();
        PositionModels();
        PositionCameras();
        SetSpritesTilingAndOffset();
        PositioningAgain();
    }

    private void PositionCameras()
    {
        PositionCamera(cameraColor);
        PositionCamera(cameraShading);
    }

    private void PositionCamera(Camera camera)
    {
        Vector3 position = camera.transform.localPosition;
        position.x = renderTextureResolution.x * 0.5f - 0.5f;
        camera.transform.localPosition = position;
    }

    private void PositionModels()
    {
        for (int i = 0; i < shipInstances.Count; i++)
        {
            Ship ship = shipInstances[i];
            Vector3 position = Vector3.zero;
            position.x = i * 1f;
            ship.model.transform.localPosition = position;
        }
    }

    private void SetSpritesTilingAndOffset()
    {
        tiling.x = 1f / renderTextureResolution.x;
        offset = Vector3.zero;
        for (int i = 0; i < shipInstances.Count; i++)
        {
            Ship ship = shipInstances[i];
            offset.x = tiling.x * i;
            MeshRenderer meshRenderer = ship.sprite.GetComponent<MeshRenderer>();
            meshRenderer.material.SetVector("_Tiling", tiling);
            meshRenderer.material.SetVector("_Offset", offset);

            meshRenderer.material.SetTexture("_MainTex", renderTextureColor);
            meshRenderer.material.SetTexture("_Color_Tex", renderTextureShading);
        }
    }

    private void ResizeRenderTextures()
    {
        renderTextureResolution.x = shipInstances.Count;
        renderTextureResolution.y = 1;

        renderTextureResolution.x = Mathf.Clamp(renderTextureResolution.x, 1, 64);
        renderTextureResolution.y = Mathf.Clamp(renderTextureResolution.y, 1, 64);
        ResizeRenderTexture(renderTextureColor);
        ResizeRenderTexture(renderTextureShading);
        // ResizeCamera(cameraColor);
        // ResizeCamera(cameraShading);
    }

    // private void ResizeCamera(Camera cameraColor)
    // {
    //     cameraColor.orthographicSize = 0.5f * renderTextureResolution.x;
    // }

    private void ResizeRenderTexture(RenderTexture renderTexture)
    {
        renderTexture.Release();
        renderTexture.width = renderTextureResolution.x * 64;
        renderTexture.height = renderTextureResolution.y * 64;
        renderTexture.Create();
    }

    private void Bank()
    {
        for (int i = 0; i < shipInstances.Count; i++)
        {
            Ship ship = shipInstances[i];
            float previousPosition = previousPositions[i];

            float velocity = ship.sprite.transform.localPosition.y - previousPosition;

            interpolatedVelocities[i] = Mathf.Lerp(interpolatedVelocities[i], velocity, Time.deltaTime);

            Vector3 localEulerAngles = ship.model.transform.localEulerAngles;
            localEulerAngles.x = interpolatedVelocities[i] * bankingSpeed;

            ship.model.transform.localEulerAngles = localEulerAngles;
            // Debug.Log(localEulerAngles.x);

            previousPositions[i] = ship.sprite.transform.localPosition.y;
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

        // LeanTween.rotateAroundLocal(flagship, Vector3.right, -15f, rotationDuration / 2f)
        //     .setEaseInOutCubic();

        // LeanTween.rotateAroundLocal(flagship, Vector3.right, 30f, rotationDuration)
        //     .setEaseInOutCubic()
        //     .setDelay(rotationDuration / 2f);

        // LeanTween.rotateAroundLocal(flagship, Vector3.right, -15f, rotationDuration / 2f)
        //     .setEaseInOutCubic()
        //     .setDelay(rotationDuration + (rotationDuration / 2f));
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
            // Resize(flagship, 0.5f, 0);
            return;
        }

        SetNotification("Resize:", "Big");
        for (int i = 0; i < gameObjects.Count; i++)
        {
            resizedBig = true;
            GameObject go = gameObjects[i];
            Resize(go, scaleBig, i);
        }
        // Resize(flagship, 1f, 0);
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
            .setLoopCount(-1);
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

    public void PositioningAgain()
    {
        if (isPositionedInRow)
        {
            PositionInRow(false);
            return;
        }

        PositionInGrid(false);
    }

    private void PositionInRow(bool doNotification = true)
    {
        if (doNotification)
        {
            SetNotification("Position:", "Row");
        }
        Vector3 position = rowStartingPosition;
        for (int i = 0; i < shipInstances.Count; i++)
        {
            Ship ship = shipInstances[i];
            ship.targetPosition = position;
            position.x += 0.9f;
        }
    }

    private void PositionInGrid(bool doNotification = true)
    {
        if (doNotification)
        {
            SetNotification("Position:", "Grid");
        }

        Vector3 position = gridStartingPosition;
        for (int i = 0; i < shipInstances.Count; i++)
        {
            Ship ship = shipInstances[i];
            ship.targetPosition = position;

            position.x++;
            if (position.x > gridStartingPosition.x + 2f)
            {
                position.x = gridStartingPosition.x;
                position.y -= 0.75f;
            }
        }
    }

    public void FlashAll()
    {
        SetNotification("Flash", "Flash");
        for (int i = 0; i < ships.Count; i++)
        {
            Ship ship = shipInstances[i];
            MeshRenderer meshRenderer = ship.sprite;
            Flash(meshRenderer, flashColor, i);
        }

        // Flash(meshRendererFlagship, flashColorEnemy, 0);
    }

    public void Flash(MeshRenderer meshRenderer, Color flashColor, int i)
    {
        LeanTween.value(meshRenderer.gameObject, Color.black, flashColor, rotationDuration / 4f)
            .setOnUpdate((Color color) => UpdateColor(meshRenderer, color))
            .setLoopPingPong(1)
            .setDelay(i * delayDuration);
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
