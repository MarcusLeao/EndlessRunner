using UnityEngine;
using InfinityRunner.Systems;

namespace InfinityRunner.Collectibles
{
    public class CoinCollectible : Collectible
    {
        [SerializeField] private int coinValue = 1;

        protected override void OnCollected(Character.PlayerMovement player)
        {
            ScoreManager.Instance?.AddCoins(coinValue);
        }
    }
}