using UnityEngine;
using TMPro;

public class Scale : MonoBehaviour
{
    [SerializeField] [Range(0.01f, 1f)] private float scale = 1.0f;
    [SerializeField] [Range(0.0f, 360f)] private float rotation = 0.0f;
    [SerializeField] [Range(0f, 1f)] private float outlineStrength = 1.0f;
    [SerializeField] [Range(0f, 1f)] private float innerLineStrength = 1.0f;
    [SerializeField] [Range(0f, 1f)] private float shadingStrength = 1.0f;
    [SerializeField] private float maxPower = 8.0f;
    [SerializeField] private Renderer renderer;
    [SerializeField] private float power;
    [SerializeField] private float logPower;
    [SerializeField] private TextMeshProUGUI scaleLabel;
    [SerializeField] private TextMeshProUGUI rotationLabel;
    [SerializeField] private TextMeshProUGUI outlineStrengthLabel;
    [SerializeField] private TextMeshProUGUI innerLinesLabel;
    [SerializeField] private TextMeshProUGUI shadingLabel;

    public void OnScale(float value)
    {
        scale = value / 100f;
        scaleLabel.SetText($"{value}%");
    }

    public void OnRotation(float value)
    {
        rotation = value;
        rotationLabel.SetText($"{value}°");
    }

    public void OnOutlinesStrength(float value)
    {
        outlineStrength = value / 100f;
        outlineStrengthLabel.SetText($"{value}%");
    }

    public void OnInnerLinesStrength(float value)
    {
        innerLineStrength = value / 100f;
        innerLinesLabel.SetText($"{value}%");
    }

    public void OnShadingStrength(float value)
    {
        shadingStrength = value / 100f;
        shadingLabel.SetText($"{value}%");
    }

    private void Update()
    {
        UpdateScaleAndRotation();
    }

    private void OnValidate()
    {
        UpdateScaleAndRotation();
    }

    private void UpdateScaleAndRotation()
    {
        transform.localScale = Vector3.one * scale;
        Vector3 euler = transform.localRotation.eulerAngles;
        euler.z = -rotation;
        transform.localRotation = Quaternion.Euler(euler);

        power = maxPower * scale;
        logPower = 8 + Mathf.Log(power, 2);
        renderer.sharedMaterial.SetFloat("_Pixelate_Multiplier", logPower);
        renderer.sharedMaterial.SetFloat("_Rotation", rotation);

        renderer.sharedMaterial.SetFloat("_Outer_Strength", outlineStrength);
        renderer.sharedMaterial.SetFloat("_Inner_Strength", innerLineStrength);
        renderer.sharedMaterial.SetFloat("_Shading_Strength", shadingStrength);
    }
}
