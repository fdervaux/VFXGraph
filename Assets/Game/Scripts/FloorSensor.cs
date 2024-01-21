using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct FloorDetection
{
    public bool detectGround;
    public float hitDistance;
    public Vector3 hitNormal;
    public float floorDistance;
    public Collider collider;
}

[System.Serializable]
public class FloorSensor
{

    [SerializeField]
    private LayerMask _layermask;

    [SerializeField]
    private bool _debugMode = false;


    private float _castLength = 1.0f;
    private Vector3 _offset = Vector3.zero;
    private Transform _owner;
    private FloorDetection _lastDetection = new FloorDetection();
    private Vector3 _castDirection = Vector3.down;


    public FloorDetection GetFloorDetection()
    {
        return _lastDetection;
    }

    public void SetOffset(Vector3 offset)
    {
        _offset = offset;
    }

    public void SetCastLength(float castLength)
    {
        _castLength = castLength;
    }

    public void SetCastDirection(Vector3 direction)
    {
        _castDirection = direction;
    }

    public void Cast()
    {
        Vector3 wordOffset = _owner.TransformVector(_offset);
        Vector3 originPosition = _owner.position + wordOffset;

        RaycastHit hit;
        _lastDetection = new FloorDetection();
        _lastDetection.detectGround = false;

        if(_debugMode)
        {
            Debug.DrawRay(originPosition, _castDirection,Color.red,Time.fixedDeltaTime*1.01f);
        }

        if(Physics.Raycast(originPosition,_castDirection,out hit,_castLength,_layermask))
        {
            _lastDetection.detectGround = true;
            _lastDetection.hitDistance = hit.distance;
            _lastDetection.floorDistance = hit.distance + Vector3.Dot(wordOffset, _castDirection);
            _lastDetection.hitNormal = hit.normal;
            _lastDetection.collider = hit.collider;
        }
    }

    public void init(Transform transform)
    {
        _owner = transform;
    }

}
