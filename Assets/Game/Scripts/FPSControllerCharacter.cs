using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FPSControllerCharacter : MonoBehaviour
{

    //Options dans unity

    [SerializeField]
    private Transform _head = null;

    [SerializeField]
    private float _mouseSensitivity = 60f;

    [SerializeField]
    private float _verticalHeadMinimuAngle = -90f;

    [SerializeField]
    private float _verticalHeadMaximumAngle = 90f;

    [SerializeField, Range(0, 10)]
    private float _moveSpeed = 3f;

    [SerializeField, Range(0, 100)]
    private float _maxMoveAcceleration = 50;

    [SerializeField]
    private FloorSensor _floorSensor;

    [SerializeField]
    private float _step = 0.3f;

    [SerializeField]
    private float _height = 2.0f;

    [SerializeField]
    private float _radius = 0.5f;

    [SerializeField]
    private float _gravity = -9.81f;

    [SerializeField]
    private float _jumpHeight = 1.5f;

    [SerializeField]
    private float _cameraFOV = 70;

    [SerializeField]
    private float _cameraAimingFOV = 40;

    [SerializeField, Range(0, 1)]
    private float _AirFriction = 1;

    [SerializeField, Range(0, 1)]
    private float _drag = 0.01f;

    [SerializeField]
    private WeaponControler _weaponController;



    // variable membre de la classe
 
    private float _verticalHeadAngle = 0f;
    private Transform _body;
    private float _horizontalRotationAccumulation = 0;
    private Vector3 _facing = Vector3.zero;
    private Vector3 _straffing = Vector3.zero;
    private Vector2 _deltaLook = Vector2.zero;
    private Vector2 _lastDeltaLook = Vector2.zero;
    private Vector2 _deltaLookSmoothDampVelocity = Vector2.zero;
    private Vector2 _deltaMove = Vector2.zero;
    private Vector2 _lastDeltaMove = Vector2.zero;
    private Vector2 _deltaMoveSmoothDampVelocity = Vector2.zero;
    private Rigidbody _rigidbody;
    private Animator _animator;
    private CapsuleCollider _capsuleCollider;
    private bool _perfomedJump = false;
    private bool _performedFire = false;
    private bool _performedReload = false;
    private Vector3 _groundVelocity = Vector3.zero;
    private bool _aiming = false;
    private Camera _camera;
    private float _cameraFOVTarget = 0;
    private float _cameraFOVSmoothDampVelocity = 0;
    private Vector3 _momentum = Vector3.zero;
    private Vector3 _horizontalMove = Vector3.zero;
    private Vector3 _groundCorrection = Vector3.zero;
    private bool _isOnGround = false;
    private bool _isSliding = false;
    private FloorDetection _floorDetection;
    private float _slopeAngle = 0; //rad

    private int _firstFrames = 10;


    // Start is called before the first frame update
    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        _body = transform;
        _capsuleCollider = GetComponent<CapsuleCollider>();
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponentInChildren<Animator>();
        _camera = GetComponentInChildren<Camera>();

        _cameraFOVTarget = _cameraFOV;
        calibrateSensor();
        calibrateCollider();
    }

    private void OnCollisionEnter(Collision other) {
            
    }
    


    public void OnMove(InputValue value)
    {
        _deltaMove = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        _deltaLook = value.Get<Vector2>();
    }

    public void OnJump()
    {
        _perfomedJump = true;
    }

    public void OnFire()
    {
        _performedFire = true;
    }
    
    public void OnReload()
    {
        _performedReload = true;
    }

    public void OnAiming(InputValue value)
    {
        _aiming = value.Get<float>() > 0.5f ? true : false;
    }

    void OnCollisionStay(Collision collision)
    {
    }

    public void calibrateSensor()
    {
        _floorSensor.init(transform);
        //add 0.01 to avoid sensor on ground 
        _floorSensor.SetOffset(new Vector3(0, _step + 0.01f, 0));
        _floorSensor.SetCastLength(_step * 3);
    }

    public void calibrateCollider()
    {
        _capsuleCollider.height = _height - _step;
        _capsuleCollider.center = new Vector3(0, _capsuleCollider.height / 2 + _step, 0);
        _capsuleCollider.radius = _radius;
    }



    private void handleGround()
    {
        // On genere les infos du sol
        _floorSensor.Cast();
        _floorDetection = _floorSensor.GetFloorDetection(); 

        _isOnGround = false;
        _isSliding = false;

        if (_floorDetection.detectGround)
        {

            // On calcule la velocité vertical en fonction de la normal au sol
            float YVelocity = Vector3.Project(_momentum, _floorDetection.hitNormal).magnitude;

            // on determine si on est au sol en fonction de la celocité et de _floorDetection
            _isOnGround = YVelocity < 0.01f || _floorDetection.floorDistance < 0;

            // On calcule l'angle d u sol
            _slopeAngle = Vector3.Angle(_floorDetection.hitNormal, transform.up) * Mathf.Deg2Rad;

            // On calcule la friction
            float staticFriction = _floorDetection.collider.material.staticFriction;

            // On determine si on est en train de glisser en fonction de l'angle et de la static friction
            _isSliding = _isOnGround && staticFriction < Mathf.Tan(_slopeAngle);
        }

        // On calcule la ground Correction
        _groundCorrection = Vector3.zero;
        if( _isOnGround )
            _groundCorrection = (-_floorDetection.floorDistance / Time.fixedDeltaTime) * transform.up;
    }

    private void computeMomentum()
    {
        // Cette fonction permet de gerer l'élan du joueur (momentum)

        Vector3 _verticalMomentum = Vector3.zero;
        Vector3 _horizontalMomentum = Vector3.zero;
        
        // On divise l'élan en un elan verticale et un élan horizontale
        if ( _isOnGround)
            _verticalMomentum = Vector3.Project(_momentum, _floorDetection.hitNormal);
        else
            _verticalMomentum = Vector3.Project(_momentum, transform.up);
        _horizontalMomentum = _momentum - _verticalMomentum;


        // On initialise la friction à 0
        float frictionAttenuation = 0;

        // Si je suis au sol
        if(_isOnGround)
        {
            // Si je ne suis pas en train de glisse
            if (!_isSliding)
            {
                // On reset l'élan verticale
                _verticalMomentum = Vector3.zero;
                // On définit le nouveau élan
                _momentum = _horizontalMomentum;
            }
            else // sinon (je suis en train de glisser)
            {
                // On ajoute au moment verticale la gravité
                _verticalMomentum = -_maxMoveAcceleration * Time.fixedDeltaTime * transform.up;

                // On Projete le moment sur le sol pour determiner la resultante de la gravité de manière horizontale
                _momentum = _horizontalMomentum + _verticalMomentum;
                _momentum = Vector3.ProjectOnPlane(_momentum, _floorDetection.hitNormal);
            }

            // On calcule la friction 
            float dynamicFriction = _floorDetection.collider.material.dynamicFriction;
            frictionAttenuation = _maxMoveAcceleration * dynamicFriction * Mathf.Cos(_slopeAngle);

            // On cherche à atteindre la vitesse max (_horizontalMove*_moveSpeed) mais on ne peut pas à cause de la friction au sol 
            // Permet d'avoir des sols glissant sur lequels on patine 
            _momentum = Vector3.MoveTowards(_momentum, _horizontalMove*_moveSpeed, frictionAttenuation * Time.fixedDeltaTime);
        }
        else //sinon (on est dans les airs)
        {
            // On ajoute la gravité
            _verticalMomentum +=  _gravity * Time.fixedDeltaTime * transform.up;

            // On ajoute de la friction dans l'air 
            float airFriction = _maxMoveAcceleration * _AirFriction;
            // On cherche à atteindre la vitesse max (_horizontalMove*_moveSpeed) mais on ne peut pas à cause de la friction dans l'air (airControl)
            _horizontalMomentum = Vector3.MoveTowards(_horizontalMomentum, _horizontalMove*_moveSpeed, airFriction * Time.fixedDeltaTime);

            // On calcule le moment complet
            _momentum = _horizontalMomentum + _verticalMomentum;
        
            // On ajuste la vitess avec un drag ( cela permet de ne pas atteindre des vitesses extrême )
            _momentum *= Mathf.Max(0,1-_drag * Time.fixedDeltaTime ) ;

        }
        
        
        
    }

    private void tryJump()
    {
        // Cette fonction permet de gerer le saut

        // Si il y a l'input du saut et que je suis au sol et que je ne suis pas en train de glisser, je fais le saut
        if(_perfomedJump && _isOnGround && !_isSliding)
        {
            Debug.Log("jump");
            //calcule de la vitesse de saut pour atteindre une certaine hauteur en fonction de la gravité
            float jumpVelocity = Mathf.Max(0, _groundVelocity.y) + Mathf.Sqrt(2 * _jumpHeight * -_gravity);

            // si mon moment vertical est inférieur j'ajoute le reste de vitesst pour atteindre la vitesse de saut maximal
            if(Vector3.Dot(_momentum,transform.up) < jumpVelocity )
            {
                _momentum = _momentum - Vector3.Project(_momentum,transform.up);
                _momentum += transform.up * jumpVelocity;
            }

            // si je saute je ne suis plus au sol
            _isOnGround = false;
        }
    }    

    private void computeMovement()
    {
        // Cette fonction permet de gerer le mouvement cible en fonction des input du joueur

        Vector3 groundNormal = transform.up;

        // On determine la normal au sol si on est au sol
        if(_isOnGround)
        {
            groundNormal = _floorDetection.hitNormal;
        }

        // On projette les axes de déplacements en fonction du sol
        Vector3 groundFacingAxis = Vector3.ProjectOnPlane(_facing, groundNormal).normalized;
        Vector3 groundStraffingAxis = Vector3.ProjectOnPlane(_straffing, groundNormal).normalized;

        //on update _hotrizontalMove en fonction des inputs du joueur
        _horizontalMove = _lastDeltaMove.x * groundStraffingAxis + _lastDeltaMove.y * groundFacingAxis;
    }


    private void FixedUpdate()
    {   
        // Calcule le momentum en fonction du mouvement précédent et retranche le groundCorrection  
        _momentum = _rigidbody.velocity - _groundCorrection;

        // Si je suis au sol retire le mouvement Vertical en cas de collision
        if(_isOnGround)
            _momentum = _momentum - Vector3.Project(_momentum, _floorDetection.hitNormal);

        handleGround();
        computeMovement();
        computeMomentum();
        tryJump();

        //applique le momentum et la groundCorection
        _rigidbody.velocity = _momentum + _groundCorrection;


        // ne pas oublier de mettre _perfomedJump à false
        _perfomedJump = false;
    }

    private void HandleAiming()
    {
        // update la _cameraFOVTargeten fonction de _aiming
        _cameraFOVTarget = _aiming ? _cameraAimingFOV : _cameraFOV;

        // Update le field of view de la camera à chaque pas de temps pour atteindre la target (crée un mouvement plus lisse de la caméra)
        _camera.fieldOfView = Mathf.SmoothDamp(_camera.fieldOfView, _cameraFOVTarget, ref _cameraFOVSmoothDampVelocity, 0.04f);

        // Prévient l'animator si le cpontroller est en aiming ou non 
        _animator.SetBool("aiming", _aiming);
    }

    private void HandleCameraLook()
    {

        if(_firstFrames  > 0)
        {
            _deltaLook = Vector2.zero;
            _firstFrames--;
        }

        //Update _lastDeltaLook en  fonction de _deltaLook pour un mouvement plus lisse
        _lastDeltaLook = Vector2.SmoothDamp(_lastDeltaLook, _deltaLook, ref _deltaLookSmoothDampVelocity, 0.04f);

        // calcule la delta rotation horizontale et l'ajoute à _horizontalRotationAccumulation
        _horizontalRotationAccumulation += _lastDeltaLook.x * Time.deltaTime * _mouseSensitivity;

        // calcule et clamp la rotation verticale
        _verticalHeadAngle = Mathf.Clamp(
            _verticalHeadAngle + _lastDeltaLook.y * Time.deltaTime * _mouseSensitivity,
            _verticalHeadMinimuAngle,
            _verticalHeadMaximumAngle);

        // applique uniquement la rotation horizontale à la tête
        _head.localRotation = Quaternion.Euler(new Vector3(0, _horizontalRotationAccumulation, 0));

        // determine les axes de mouvements du controller en fonction d'ou il regarde horizontalement
        _facing = _head.forward;
        _straffing = _head.right;

        //Debug.Log(_verticalHeadAngle);

        // applique la rotation vertical et horizontale à la tête
        _head.localRotation = Quaternion.Euler(new Vector3(-_verticalHeadAngle, _horizontalRotationAccumulation, 0));
    }

    private void HandleMovement()
    {
        //Donne la vitesse actuelle à l'animator
        _animator.SetFloat("speed", _lastDeltaMove.magnitude);

        //Update _lastDeltaMove en  fonction de _deltaMove pour un mouvement plus lisse
        _lastDeltaMove = Vector2.SmoothDamp(_lastDeltaMove, _deltaMove, ref _deltaMoveSmoothDampVelocity, 0.04f);
    }

    private void HandleFire()
    {
        if (_performedFire)
        {
            //previent l'animator que l'on tire pour lancer l'animation
            _animator.SetTrigger("fire");

            //retire des balle au pistolet
            _weaponController.decreaseAmmo();


        }
    }


    //Permet de gerer le reload
    private void HandleReload()
    {
        if (_performedReload)
        {
            //previent l'animator que l'on recharge pour lancer l'animation
            _animator.SetTrigger("reload");
            
            //recharge l'arme
            _weaponController.reloadAmmo();
            

        }
    }

    // Update is called once per frame
    private void Update()
    {
        HandleAiming();
        HandleMovement();
        HandleCameraLook();
        HandleFire();
        HandleReload();

        //Si une action n'est pas réalisé lors du premier update apres que le joueur et appuyé on ne réalise pas l'action

        // ne pas oublier de mettre performedReload à false
        _performedReload = false;
        // ne pas oublier de mettre performedFire à false
        _performedFire = false;
    }

    void Start()
    {
        // start la coroutine pour le lateFixedUpdate
        StartCoroutine(LateFixedUpdate());
    }

    IEnumerator LateFixedUpdate()
    {
        while (true)
        {
            // On attend que le fixed Update est lieu
            yield return new WaitForFixedUpdate();

            // Cette fonction permet de deplacer notre controlleur en fonction de la plateforme en dessous de lui.

            _groundVelocity = Vector3.zero;

            if (_floorDetection.detectGround)
            {
                //On recupère le rigidbody associé à la plateforme
                Rigidbody groundMovePlateforme = _floorDetection.collider.transform.GetComponentInParent<Rigidbody>();

                if (groundMovePlateforme != null)
                {
                    
                    //offset entre le centre de rotation de la plateforme et le controller
                    Vector3 offset = _rigidbody.position - groundMovePlateforme.position;
                    //effectuer une rotation de l'offset en fonction de l'angularVelcity de la plateforme
                    Vector3 rotateOffset = Quaternion.Euler(groundMovePlateforme.angularVelocity*Mathf.Rad2Deg * Time.fixedDeltaTime) * offset;

                    //On deplace le controller en fonction du mouvement de la plateforme et de l'offset calculé précedemment
                    _rigidbody.MovePosition(groundMovePlateforme.position + rotateOffset + groundMovePlateforme.velocity * Time.fixedDeltaTime);

                    //On rotate le rigidbody en fonction de la plateforme
                    //On bouge le rigidbody à la place de la tête car on veux garder un mouvement fluide et on profite de l'interpolation des rigidbody de unity
                    //Cela n'a pas d'impact sur notre controller car on le déplace en fonction de l'a ou il regarde 
                    _rigidbody.MoveRotation(Quaternion.Euler(groundMovePlateforme.angularVelocity.y * transform.up * Mathf.Rad2Deg * Time.fixedDeltaTime) * _rigidbody.rotation);

                    //on garde en memoire la velocity de la plateforme pour le saut
                    _groundVelocity = groundMovePlateforme.velocity;
                }
            }
        }
    }
}
