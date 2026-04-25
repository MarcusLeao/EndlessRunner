using UnityEngine;

namespace InfinityRunner.Obstacles
{
    public class Obstacle : MonoBehaviour
    {
        [SerializeField] private bool instantKill = true;

        private bool _hasBeenHit;

        public void Hit(Character.PlayerMovement player)
        {
            if (_hasBeenHit || player == null)
                return;

            _hasBeenHit = true;

            if (instantKill)
            {
                player.Kill();
            }
        }
    }
}