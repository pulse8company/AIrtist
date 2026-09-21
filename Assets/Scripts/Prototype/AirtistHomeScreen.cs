using System;
using TMPro;
using UnityEngine;

namespace Airtist.Prototype
{
    /// <summary>Interactive, aspect-fitted Home using the approved illustrated artwork.</summary>
    public sealed class AirtistHomeScreen : MonoBehaviour
    {
        // Gift, Map, Collection, Store, Profile, Continue, Daily reward, World map.
        [SerializeField] private UnityEngine.UI.Button[] buttons;
        [SerializeField] private GameObject claimedBadge;

        public UnityEngine.UI.Button[] Buttons => buttons;

        public void Configure(UnityEngine.UI.Button[] controls, GameObject badge)
        {
            buttons = controls;
            claimedBadge = badge;
        }

        public void Bind(Action[] actions)
        {
            if (buttons == null || actions.Length != buttons.Length)
                throw new InvalidOperationException("Home requires one action for each illustrated button.");

            for (int i = 0; i < buttons.Length; i++)
            {
                Action action = actions[i];
                buttons[i].onClick.AddListener(() => action());
            }
        }

        public void SetBonusClaimed(bool claimed)
        {
            buttons[0].interactable = !claimed;
            buttons[6].interactable = !claimed;
            claimedBadge.SetActive(claimed);
        }
    }
}
