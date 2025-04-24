using UnityEngine;

public class ObjectInfo : MonoBehaviour
{
    [TextArea(3, 10)]
    public string description;

    [Tooltip("Prefab, який замінить поточну модель після взаємодії")]
    public GameObject replacementPrefab;

    [HideInInspector]
    public bool isReplaced = false;

    [HideInInspector]
    public PointType pointType;

    [HideInInspector]
    public int pointIndex; // нове поле: індекс у списку _latLongs
}