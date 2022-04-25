using UnityEngine;
using System;
using System.Collections;
using UnityEngine.InputSystem;
using Cinemachine;

public enum PlayerStates
{
    walking, sprinting, falling, dashing
}
public enum CameraStates
{
    locked, free
}

public class PlayerController : MonoBehaviour
{

    [SerializeField] private float _dashDistance;

    [SerializeField] private float _baseSpeed;
    [SerializeField] private float _speedWalkLimit;
    [SerializeField] private float _speedSprintLimit;
    private float _speedLimit;
    [SerializeField] private float _groundDrag;
    [SerializeField] private float _sprintDrag;
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private GameObject CMVCam;
    [SerializeField] private GameObject _model;

    [SerializeField] private float _maxHealth;
    private float _health;
    private float _speed;

    [SerializeField] private float _turnLerpTChange;
    private float _turnLerpT;
    private float _previousAngle;


    [SerializeField] private Vector2 _rotationSpeed;
    [SerializeField] private Vector2 _xLimit;
    private Vector2 _rotation;

    private RaycastHit prevRayHit = new RaycastHit();
    private Vector3 _velocity;
    private float _elapsedWalkTime;
    [SerializeField] private float _maxWalkTime;
    private float _drag;

    private PlayerStates _states = PlayerStates.walking;
    private CameraStates _cameraStates = CameraStates.free;
    private float _fallingSpeed;
    [SerializeField] private float _baseFallingSpeed = 1;
    [SerializeField] private float _jumpSpeed;
    private GameObject _lockTarget;
    private float _prevLockAxis;
    private InputActions _inputActions;
    
    private float _dashTravelled;
    private float _dashVelocity;


    [SerializeField] private float _dashMod = 3;

    

    private void Start()
    {
        _health = _maxHealth;

        _inputActions = new InputActions();
        Cursor.lockState = CursorLockMode.Locked;
        _speed = _baseSpeed;
        _fallingSpeed = _baseFallingSpeed;

        AddListeners();
    }

    private void AddListeners()
    {
        _inputActions.Player.Enable();
        _inputActions.Player.Dash.performed += DashCheck;
        _inputActions.Player.Ability.performed += Ability;
        _inputActions.Player.Attack.performed += Attack;
        _inputActions.Player.Jump.performed += Jump;
    }

    // Update is called once per frame
    void Update()
    {
        StatesControl();
        MainMovement();
        CameraBehaviour();


    }



    #region States
    private void StatesControl()
    {
        FallingCheck();
        //LookingAt();

    }

    //private void LookingAt()
    //{
    //    RaycastHit hit;
    //    var screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);

    //    if (Physics.Raycast(_playerCamera.ScreenPointToRay(screenCenter), out hit))
    //    {
    //        if (hit.collider != prevRayHit.collider)
    //        {
    //            var enemyScript = hit.collider.gameObject.GetComponent<EnemyScript>();
    //            if (enemyScript != null)
    //            {
    //                enemyScript.Highlight();

    //            }
    //            if (!prevRayHit.Equals(new RaycastHit()) && prevRayHit.collider.GetComponent<EnemyScript>() != null)
    //                prevRayHit.collider.GetComponent<EnemyScript>().Dehighlight();
    //        }
    //        prevRayHit = hit;
    //    }
    //    else if (prevRayHit.collider != null)
    //    {
    //        if (prevRayHit.collider.GetComponent<EnemyScript>() != null)
    //        {
    //            prevRayHit.collider.GetComponent<EnemyScript>().Dehighlight();
    //            prevRayHit = new RaycastHit();
    //        }
    //    }

    //    var lockAxis = Input.GetAxis("Lock");
    //    if ( lockAxis > 0 && _prevLockAxis == 0)
    //    {
    //        if (_cameraStates == CameraStates.locked)
    //            _cameraStates = CameraStates.free;
    //        else if (_cameraStates == CameraStates.free && prevRayHit.collider != null)
    //        {
    //            if (prevRayHit.collider.GetComponent<EnemyScript>() != null)
    //            {
    //                _lockTarget = prevRayHit.collider.gameObject;
    //                _cameraStates = CameraStates.locked;
    //                Debug.Log("locked");
    //            }
    //        }

    //    }
    //    _prevLockAxis = lockAxis;
    //}

    private void FallingCheck()
    {
        RaycastHit hit;
        float distance = 1000;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, distance))
        {
            if (transform.position.y - hit.point.y >= 1.001f && _states != PlayerStates.falling)
            {
                _states = PlayerStates.falling;
                _speed = _baseSpeed / 2;
                _fallingSpeed = 9.8f;
            }
            else if (_states == PlayerStates.falling)
            {
                _states = PlayerStates.walking;
                _speed = _baseSpeed;
                _fallingSpeed = _baseFallingSpeed;
            }

        }

    }



    private void DashCheck(InputAction.CallbackContext context)
    {
        if (context.performed && _states != PlayerStates.dashing)
        {
            _states = PlayerStates.dashing;
        }
    }
    #endregion
    private void Attack(InputAction.CallbackContext obj)
    {
        //throw new NotImplementedException();
    }

    private void Ability(InputAction.CallbackContext context)
    {
        //throw new NotImplementedException();
    }

    #region CharacterMovement 
    private void MainMovement()
    {
        if (_states == PlayerStates.dashing)
        {
            Dashing();
            return;
        }
        ApplyDrag();
        InputMovement();
        Gravity();
        LimitMovement();

        this.GetComponent<CharacterController>().Move(_velocity);
    }

    private void ApplyDrag()
    {
        var movement = _inputActions.Player.HorizontalMovement.ReadValue<Vector2>();
        if (movement.magnitude <= 0.1)
            _velocity *= 1 - _drag;
        if (_velocity.magnitude <= 0.01)
            _velocity = new Vector3();
 
    }

    private void LimitMovement()
    {
        _velocity = Vector3.ClampMagnitude(_velocity, _speedLimit);
    }

    private void InputMovement()
    {
        if (_states != PlayerStates.dashing)
        {
            var movement = _inputActions.Player.HorizontalMovement.ReadValue<Vector2>();
            var forwardsMovement = movement.y * _speed * Time.deltaTime * this.transform.forward;
            var sidewaysMovement = movement.x * _speed * Time.deltaTime * this.transform.right;
            CharacterTurning(movement.x, movement.y);

            _velocity += forwardsMovement;
            _velocity += sidewaysMovement;


        }
    }

    private void CharacterTurning(float xMove, float yMove)
    {
        float angle = 0;
        angle = AngleCalculations(xMove, yMove);
        SprintCheck(angle);
        if (angle <= _previousAngle + 1 && angle >= _previousAngle - 1)
            _turnLerpT += _turnLerpTChange + Time.deltaTime;
        else
            _turnLerpT -= _turnLerpTChange + Time.deltaTime;

        Quaternion target = Quaternion.Euler(this.transform.rotation.x, angle, this.transform.rotation.z);
        _model.transform.localRotation = Quaternion.Lerp(_model.transform.localRotation, target, _turnLerpT);
        _previousAngle = angle;

    }

    private float AngleCalculations(float xMove, float yMove)
    {
        if (yMove == 0)
        {
            if (xMove == 1)
                return 90;
            if (xMove == -1)
                return -90;
        }
        if (xMove == 0)
        {
            if (yMove == 1)
                return 0;
            if (yMove == -1)
                return 180;
        }
        if (yMove == 0 && yMove == 0)
            return _previousAngle;


        float degAngle = 90;
        float radAngle = Mathf.Atan(xMove / yMove);
        degAngle = radAngle * Mathf.Rad2Deg;
        if (yMove < 0)
            degAngle = 180 + degAngle;
        return degAngle;
    }

    private void Gravity()
    {
        var downMovement = _fallingSpeed * Time.deltaTime * Vector3.down;

        _velocity += downMovement;

    }

    private void Dashing()
    {

        _dashVelocity += _speed * _dashMod * Time.deltaTime;
        this.GetComponent<CharacterController>().Move(_dashVelocity * _model.transform.forward);
        _dashTravelled += _dashVelocity;
        Debug.Log(_dashTravelled);
        if (_dashDistance <= _dashTravelled)
        {
            _dashVelocity = 0;
            _dashTravelled = 0;
            _states = PlayerStates.walking;
        }
    }

    private void Jump(InputAction.CallbackContext context)
    {
        Debug.Log(_states);
        if (_states != PlayerStates.falling && _states != PlayerStates.dashing && context.performed)
            _velocity.y = _jumpSpeed;
    }

    private void SprintCheck(float angle)
    {
        _elapsedWalkTime += Time.deltaTime;
        if (angle > _previousAngle + 15 || angle < _previousAngle - 15)
        {
            _elapsedWalkTime = 0;
            _drag = _groundDrag;
            _speedLimit = _speedWalkLimit;
            
        }
        else if (_elapsedWalkTime >= _maxWalkTime)
        {
            _drag = _sprintDrag;
            _speedLimit = _speedSprintLimit;
        }

    }

    #endregion

    #region CameraMovement

    private void CameraBehaviour()
    {
        CameraRotatingMovement();
    }
    private void CameraRotatingMovement()
    {
        if (_cameraStates == CameraStates.free)
            HorizontalMovement();
        else if (_cameraStates == CameraStates.locked)
            LookAtLock();
        VerticalMovement();
        
    }

    private void LookAtLock()
    {
        var xDis = _lockTarget.transform.position.x - this.transform.position.x;
        var zDis = _lockTarget.transform.position.z - this.transform.position.z;
        var angle = AngleCalculations(xDis, zDis);
        this.transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.x * Mathf.Rad2Deg, angle, this.transform.rotation.z * Mathf.Deg2Rad));

    }

    private void VerticalMovement()
    {
        _rotation.x += _inputActions.Player.CameraMovement.ReadValue<Vector2>().y * _rotationSpeed.y * Time.deltaTime;
        _rotation.x = Mathf.Clamp(_rotation.x, _xLimit.x, _xLimit.y);

    }

    private void HorizontalMovement()
    {
        _rotation.y += _inputActions.Player.CameraMovement.ReadValue<Vector2>().x * _rotationSpeed.x * Time.deltaTime;
        this.gameObject.transform.rotation = Quaternion.Euler(0, -_rotation.y, 0);
    }
    #endregion CameraMovement

    public void OnHit()
    {
        _health--;
    }
}
