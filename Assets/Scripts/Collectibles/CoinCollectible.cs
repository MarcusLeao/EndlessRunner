using UnityEngine;
using InfinityRunner.Systems;
using InfinityRunner.Core;

namespace InfinityRunner.Collectibles
{
    public class CoinCollectible : Collectible
    {
        [SerializeField] private int coinValue = 1;

        protected override void OnCollected(Character.PlayerMovement player)
        {
            AudioManager.Instance?.PlayCoin();
            ScoreManager.Instance?.AddCoins(coinValue);
        }
    }
}
