using UnityEngine;

[CreateAssetMenu(fileName = "Ship", menuName = "Scriptable Objects/Ship")]
public class Ship : ScriptableObject
{
    public Transform modelPrefab;
    public MeshRenderer spritePrefab;
    internal Transform model;
    internal MeshRenderer sprite;
}
