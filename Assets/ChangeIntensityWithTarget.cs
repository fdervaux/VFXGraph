using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ChangeIntensityWithTarget : MonoBehaviour
{
    private VisualEffect _visualEffect;

    [SerializeField] private Transform _targetPlayer;

    [SerializeField] private float _maxDistance = 10;
    [SerializeField] private float _minDistance = 1;

    private readonly int AmplitudeID = Shader.PropertyToID("ElectricNoiseAmplitude");

    private readonly int Color1ID = Shader.PropertyToID("Electric color 1");

    private readonly int Color2ID = Shader.PropertyToID("Electric Color 2");

    [SerializeField, ColorUsage(true, true)] private Color _color1A;
    
    [SerializeField, ColorUsage(true, true)] private Color _color2A;
    
    [SerializeField, ColorUsage(true, true)] private Color _color1B;
    
    [SerializeField, ColorUsage(true, true)] private Color _color2B;


    // Start is called before the first frame update
    void Start()
    {
        _visualEffect = GetComponent<VisualEffect>();
    }

    // Update is called once per frame
    void Update()
    {
        float distanceToPlayer = Vector3.Magnitude(transform.position - _targetPlayer.position);

        float coef = Math.Clamp(Remap(distanceToPlayer,_minDistance,_maxDistance,1,0),0,1);

        
        if(distanceToPlayer<5)
        {
            _visualEffect.SetVector4(Color1ID,_color1A);
            _visualEffect.SetVector4(Color2ID,_color2A);
        }
        else
        {
            _visualEffect.SetVector4(Color1ID,_color1B);
            _visualEffect.SetVector4(Color2ID,_color2B);
        }

        _visualEffect.SetFloat(AmplitudeID,coef+0.5f);


    }


    public float Remap( float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}
