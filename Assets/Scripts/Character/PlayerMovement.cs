using UnityEngine;
using InfinityRunner.Core;
using InfinityRunner.Data;

namespace InfinityRunner.Character
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private RunConfig config;
        [SerializeField] private Animator animator;

        public PlayerState CurrentState { get; private set; } = PlayerState.Running;
        public int CurrentLane => _currentLane;
        public float CurrentForwardSpeed => _currentForwardSpeed;
        public bool IsGrounded => characterController.isGrounded;
        public bool IsSliding => _isSliding;

        private int _currentLane = 0;
        private float _verticalVelocity;
        private float _currentForwardSpeed;

        private bool _isSliding;
        private float _slideTimer;

        private float _defaultControllerHeight;
        private Vector3 _defaultControllerCenter;

        private void Reset()
        {
            characterController = GetComponent<CharacterController>();
        }

        private void Awake()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();

            _defaultControllerHeight = characterController.height;
            _defaultControllerCenter = characterController.center;

            _currentForwardSpeed = config != null ? config.startForwardSpeed : 8f;
        }

        private void Update()
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying)
                return;

            Tick(Time.deltaTime);
        }

        public void Tick(float deltaTime)
        {
            UpdateForwardSpeed(deltaTime);
            UpdateVerticalMovement(deltaTime);
            UpdateSlide(deltaTime);

            float targetX = _currentLane * config.laneOffset;
            float nextX = Mathf.MoveTowards(
                transform.position.x,
                targetX,
                config.laneChangeSpeed * deltaTime
            );

            float deltaX = nextX - transform.position.x;

            Vector3 motion = new Vector3(
                deltaX,
                _verticalVelocity * deltaTime,
                _currentForwardSpeed * deltaTime
            );

            characterController.Move(motion);

            UpdateState();
            UpdateAnimator();
        }

        public void MoveLeft()
        {
            if (_currentLane <= -1) return;
            _currentLane--;
        }

        public void MoveRight()
        {
            if (_currentLane >= 1) return;
            _currentLane++;
        }

        public void Jump()
        {
            if (!characterController.isGrounded) return;
            if (_isSliding) return;

            _verticalVelocity = config.jumpForce;
        }

        public void Slide()
        {
            if (!characterController.isGrounded) return;
            if (_isSliding) return;

            _isSliding = true;
            _slideTimer = config.slideDuration;

            ApplySlideCollider();
        }

        public void Kill()
        {
            CurrentState = PlayerState.Dead;
            GameManager.Instance?.GameOver();
        }

        private void UpdateForwardSpeed(float deltaTime)
        {
            _currentForwardSpeed += config.forwardAcceleration * deltaTime;
            _currentForwardSpeed = Mathf.Clamp(
                _currentForwardSpeed,
                config.startForwardSpeed,
                config.maxForwardSpeed
            );
        }

        private void UpdateVerticalMovement(float deltaTime)
        {
            if (characterController.isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -config.groundedVerticalForce;
            }

            _verticalVelocity -= config.gravity * deltaTime;
        }

        private void UpdateSlide(float deltaTime)
        {
            if (!_isSliding) return;

            _slideTimer -= deltaTime;
            if (_slideTimer <= 0f)
            {
                _isSliding = false;
                RestoreDefaultCollider();
            }
        }

        private void UpdateState()
        {
            if (CurrentState == PlayerState.Dead)
                return;

            if (_isSliding)
            {
                CurrentState = PlayerState.Sliding;
                return;
            }

            if (!characterController.isGrounded)
            {
                CurrentState = PlayerState.Jumping;
                return;
            }

            CurrentState = PlayerState.Running;
        }

        private void UpdateAnimator()
        {
            if (animator == null) return;

            animator.SetBool("Grounded", characterController.isGrounded);
            animator.SetBool("Sliding", _isSliding);
            animator.SetFloat("VerticalVelocity", _verticalVelocity);
            animator.SetFloat("ForwardSpeed", _currentForwardSpeed);
        }

        private void ApplySlideCollider()
        {
            characterController.height = config.slideControllerHeight;

            float centerYOffset = (_defaultControllerHeight - config.slideControllerHeight) * 0.5f;
            characterController.center = new Vector3(
                _defaultControllerCenter.x,
                _defaultControllerCenter.y - centerYOffset,
                _defaultControllerCenter.z
            );
        }

        private void RestoreDefaultCollider()
        {
            characterController.height = _defaultControllerHeight;
            characterController.center = _defaultControllerCenter;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;

            if (config == null) return;

            Vector3 pos = transform.position;
            Gizmos.DrawLine(pos + Vector3.left * config.laneOffset, pos + Vector3.left * config.laneOffset + Vector3.forward * 3f);
            Gizmos.DrawLine(pos, pos + Vector3.forward * 3f);
            Gizmos.DrawLine(pos + Vector3.right * config.laneOffset, pos + Vector3.right * config.laneOffset + Vector3.forward * 3f);
        }
    }
}