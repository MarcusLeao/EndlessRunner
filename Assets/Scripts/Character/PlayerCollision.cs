using UnityEngine;
using InfinityRunner.Core;
using InfinityRunner.Obstacles;
using InfinityRunner.Collectibles;

namespace InfinityRunner.Character
{
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

            Obstacle obstacle = other.GetComponent<Obstacle>() ?? other.GetComponentInParent<Obstacle>();
            if (obstacle != null)
            {
                obstacle.Hit(movement);
                return;
            }

            Collectible collectible = other.GetComponent<Collectible>() ?? other.GetComponentInParent<Collectible>();
            if (collectible != null)
            {
                collectible.Collect(movement);
            }
        }
    }
}