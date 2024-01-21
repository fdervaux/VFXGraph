using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlateforme : MonoBehaviour
{

    public List<Transform> _trajectoryPoints = new List<Transform>();

    public float _waitingTime = 1.0f; //s

    private float _currentPosition = 0; // position on trajectory between 0 and trajectoryPoints.Count

    public float _animationDuration = 10.0f; //s

    private Rigidbody _rigidbody;

    private float _waitingTimeRemaining = 0.0f;

    private int _previousAnimationIndex = 0;

    public AnimationCurve _animationCurve;


    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = GetComponentInChildren<Rigidbody>();
        _rigidbody.position = _trajectoryPoints[0].position;
        _rigidbody.rotation = _trajectoryPoints[0].rotation;
    }

    private void FixedUpdate()
    {

        if(_waitingTimeRemaining > 0)
        {
            _waitingTimeRemaining -= Time.fixedDeltaTime;
        }
        else
        {
            _currentPosition = (_currentPosition + Time.fixedDeltaTime / (_animationDuration)) % _trajectoryPoints.Count;

            int index = Mathf.FloorToInt(_currentPosition );

            if (index != _previousAnimationIndex)
            {
                _waitingTimeRemaining = _waitingTime;
            }
            else
            {
                float factor = (_currentPosition ) - index;
                factor = _animationCurve.Evaluate(factor);

                int nextIndex = (index + 1) == _trajectoryPoints.Count ? 0 : index + 1;

                Vector3 position = Vector3.Lerp(_trajectoryPoints[index].position, _trajectoryPoints[nextIndex].position, factor);
                Quaternion rotation = Quaternion.Lerp(_trajectoryPoints[index].rotation, _trajectoryPoints[nextIndex].rotation, factor);

                _rigidbody.MovePosition(position);
                _rigidbody.MoveRotation(rotation);
            }

            _previousAnimationIndex = index;
        }

    }
}
