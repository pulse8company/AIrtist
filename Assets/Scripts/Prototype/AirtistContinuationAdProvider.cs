using System;
using UnityEngine;

namespace Airtist.Prototype
{
    // An SDK adapter must report true only from its verified reward event, never on close.
    // It must complete false on load/show failure or closure without a reward, on Unity's main thread.
    // Callers share this provider for continuation, brush hints and energy; only one request is in flight.
    // No adapter is supplied yet: the UI explicitly disables video continuation.
    public abstract class AirtistContinuationAdProvider : MonoBehaviour
    {
        public abstract bool IsReady { get; }
        public abstract void ShowRewarded(Action<bool> completed);
    }
}
