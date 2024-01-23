using System.Collections;
using System.Collections.Generic;
using JfranMora.Inspector;
using UnityEngine;

public class PlaceOnCircle : MonoBehaviour
{
    [SerializeField] private float radius = 1f;
    [SerializeField] private int numberOfObjects = 5;
    [SerializeField] private GameObject prefab;

    [Button]
    public void UpdateObjects()
    {
        //delete children
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }

        for (int i = 0; i < numberOfObjects; i++)
        {
            float angle = i * Mathf.PI * 2 / numberOfObjects;
            Vector3 positionOnCircle = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
            positionOnCircle = transform.TransformPoint(positionOnCircle);

            Instantiate(prefab, positionOnCircle, Quaternion.identity, transform);
        }
    }
}
