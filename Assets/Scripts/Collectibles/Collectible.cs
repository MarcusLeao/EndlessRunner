using UnityEngine;

namespace InfinityRunner.Collectibles
{
    public abstract class Collectible : MonoBehaviour
    {
        private bool _collected;

        public void Collect(Character.PlayerMovement player)
        {
            if (_collected || player == null)
                return;

            _collected = true;
            OnCollected(player);

            Destroy(gameObject);
        }

        protected abstract void OnCollected(Character.PlayerMovement player);
    }
}