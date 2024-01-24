using System.Collections;
using System.Collections.Generic;
using JfranMora.Inspector;
using UnityEditor;
using UnityEngine;

public class PlaceOnCircle : MonoBehaviour
{
    [SerializeField] private float radius = 1f;
    [SerializeField] private int numberOfObjects = 5;
    [SerializeField] private GameObject prefab;

    [Button]
    private void DestroyChilds()
    {
        //delete children
        for (int index = transform.childCount -1 ; index >=0 ; --index)
        {
            DestroyImmediate(transform.GetChild(index).gameObject);
        }
    }

    [Button]
    public void UpdateObjects()
    {
        for (int i = 0; i < numberOfObjects; i++)
        {
            float angle = i * Mathf.PI * 2 / numberOfObjects;
            Vector3 positionOnCircle = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);

            GameObject prefabInstance = PrefabUtility.InstantiatePrefab(prefab, transform) as GameObject;
            prefabInstance.transform.localPosition = positionOnCircle;
        }
    }
}
