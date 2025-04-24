using UnityEngine;

public class ObjectInfo : MonoBehaviour
{
    [TextArea(3, 10)]
    public string description;

    [Tooltip("Prefab, ���� ������� ������� ������ ���� �����䳿")]
    public GameObject replacementPrefab;

    [HideInInspector]
    public bool isReplaced = false;

    [HideInInspector]
    public PointType pointType;

    [HideInInspector]
    public int pointIndex; // ���� ����: ������ � ������ _latLongs
}