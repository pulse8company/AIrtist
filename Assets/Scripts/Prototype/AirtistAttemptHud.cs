using TMPro;
using UnityEngine;

namespace Airtist.Prototype
{
    public sealed class AirtistAttemptHud : MonoBehaviour
    {
        public bool separateCaptions;
        public TextMeshProUGUI timeLabel, clicksLabel, energyLabel, coinsLabel, endMessage;
        public GameObject endPanel;
        public UnityEngine.UI.Button restart, home, close, rewardedContinue, bonusContinue;
    }
}
