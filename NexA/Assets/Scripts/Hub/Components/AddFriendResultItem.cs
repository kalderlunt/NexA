using System;
using NexA.Hub.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NexA.Hub.Components
{
    /// <summary>
    /// Item de résultat de recherche dans AddFriendOverlay.
    ///
    /// Hiérarchie :
    ///   AddFriendResultItem
    ///   ├── AvatarImage (Image)
    ///   ├── InfoContainer
    ///   │   ├── UsernameText (TMP)
    ///   │   └── LevelText (TMP)
    ///   └── ActionButton (Button)
    ///       └── ButtonLabel (TMP)  ←  "+ Ami" / "✓ Envoyé" / "Ami"
    /// </summary>
    public class AddFriendResultItem : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI usernameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Image statusDot;           // optionnel : petit point coloré
        [SerializeField] private Button actionButton;
        [SerializeField] private TextMeshProUGUI buttonLabel;
        [SerializeField] private GameObject loadingSpinner; // optionnel

        // ── État ──────────────────────────────────────────────────────
        private string _userId;
        private Action<string, AddFriendResultItem> _onSendRequest;
        private bool _sent = false;

        // ── Setup ──────────────────────────────────────────────────────

        /// <summary>
        /// Initialise cet item avec les données d'un utilisateur trouvé par la recherche.
        /// </summary>
        public void Setup(User user, Action<string, AddFriendResultItem> onSendRequest)
        {
            _userId         = user.id;
            _onSendRequest  = onSendRequest;

            if (usernameText) usernameText.text = user.username;
            if (levelText)    levelText.text    = $"Niv. {user.level}";

            // Point de statut (optionnel)
            if (statusDot)
            {
                statusDot.color = (user.status?.ToUpper()) switch
                {
                    "ONLINE"  => new Color(0.2f, 0.9f, 0.3f),
                    "IN_GAME" => new Color(1f, 0.65f, 0.1f),
                    _         => new Color(0.5f, 0.5f, 0.5f)
                };
            }

            // État du bouton selon la relation
            if (user.isFriend)
            {
                SetAlreadyFriend();
            }
            else
            {
                SetAddable();
            }
        }

        // ── États du bouton ────────────────────────────────────────────

        private void SetAddable()
        {
            _sent = false;
            if (actionButton)  actionButton.interactable = true;
            if (buttonLabel)   buttonLabel.text = "+ Ami";
            if (loadingSpinner) loadingSpinner.SetActive(false);

            actionButton?.onClick.RemoveAllListeners();
            actionButton?.onClick.AddListener(OnButtonClicked);
        }

        private void SetAlreadyFriend()
        {
            if (actionButton)  actionButton.interactable = false;
            if (buttonLabel)   buttonLabel.text = "✓ Ami";
            if (loadingSpinner) loadingSpinner.SetActive(false);
            actionButton?.onClick.RemoveAllListeners();
        }

        /// <summary>Affiche un spinner pendant la requête.</summary>
        public void SetLoading(bool loading)
        {
            if (actionButton)   actionButton.interactable = !loading;
            if (loadingSpinner) loadingSpinner.SetActive(loading);
            if (loading && buttonLabel) buttonLabel.text = "…";
        }

        /// <summary>Affiche l'état "demande envoyée".</summary>
        public void SetSent()
        {
            _sent = true;
            if (actionButton)   actionButton.interactable = false;
            if (buttonLabel)    buttonLabel.text = "✓ Envoyé";
            if (loadingSpinner) loadingSpinner.SetActive(false);
            actionButton?.onClick.RemoveAllListeners();
        }

        // ── Click ──────────────────────────────────────────────────────

        private void OnButtonClicked()
        {
            if (_sent) return;
            _onSendRequest?.Invoke(_userId, this);
        }

        private void OnDestroy()
        {
            actionButton?.onClick.RemoveAllListeners();
        }
    }
}

