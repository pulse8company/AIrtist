using UnityEngine;

namespace Airtist.Prototype
{
    [CreateAssetMenu(menuName="AIrtist/Economy balance")]
    public sealed class AirtistEconomyBalance : ScriptableObject
    {
        [Min(1)] public int energyCap=100;
        [Min(1)] public int regenerationSeconds=300;
        [Min(1)] public int searchEnergy=2;
        [Min(0)] public int completionEnergy=2;
        [Min(0)] public int firstCompletionCoins=30;
        [Min(0)] public int additionalStarCoins=5;
        [Min(1)] public int energyPackCoins=100;
        [Min(1)] public int energyPackAmount=40;
        [Min(1)] public int rewardedEnergy=20;
        [Min(0)] public int rewardedEnergyDailyLimit=3;
    }
}
