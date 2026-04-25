using UnityEngine;
using InfinityRunner.Core;
using InfinityRunner.Obstacles;
using InfinityRunner.Collectibles;

namespace InfinityRunner.Character
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerCollision : MonoBehaviour
    {
        [SerializeField] private PlayerMovement movement;

        private void Reset()
        {
            movement = GetComponent<PlayerMovement>();
        }

        private void Awake()
        {
            if (movement == null)
                movement = GetComponent<PlayerMovement>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying)
                return;

            TryCollect(other);
            TryHitObstacle(other);
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying)
                return;

            if (hit.collider == null)
                return;

            TryHitObstacle(hit.collider);
        }

        private void TryCollect(Collider other)
        {
            Collectible collectible = other.GetComponent<Collectible>() ?? other.GetComponentInParent<Collectible>();
            if (collectible != null)
            {
                collectible.Collect(movement);
            }
        }

        private void TryHitObstacle(Collider other)
        {
            Obstacle obstacle = other.GetComponent<Obstacle>() ?? other.GetComponentInParent<Obstacle>();
            if (obstacle != null)
            {
                obstacle.Hit(movement);
            }
        }
    }
}