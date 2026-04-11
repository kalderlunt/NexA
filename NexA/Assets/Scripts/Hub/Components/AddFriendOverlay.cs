using DG.Tweening;
using NexA.Hub.Models;
using NexA.Hub.Services;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NexA.Hub.Components
{
    /// <summary>
    /// Overlay inline dans le SocialPanel pour rechercher un utilisateur et envoyer une demande d'ami.
    ///
    /// Hiérarchie attendue (enfant de SocialPanel) :
    ///  ┌──────────────────────────────────┐
    ///  │ [←] Ajouter un ami              │  ← TopRow (TitleText + CloseButton)
    ///  │ [____________________] [🔍]     │  ← SearchRow (InputField + SearchButton)
    ///  │ ─────────────────────────────── │
    ///  │  ● Pseudo      Niv.5  [+ Ami]  │  ← ResultItem (ScrollView > Content)
    ///  │  ● Pseudo2     Niv.3  [✓ Ami]  │
    ///  └──────────────────────────────────┘
    /// </summary>
    public class AddFriendOverlay : MonoBehaviour
    {
        [Header("Structure")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Search")]
        [SerializeField] private TMP_InputField searchInput;
        [SerializeField] private Button searchButton;

        [Header("Résultats")]
        [SerializeField] private Transform resultsContainer;
        [SerializeField] private GameObject resultItemPrefab;   // AddFriendResultItem prefab
        [SerializeField] private TextMeshProUGUI noResultsText;
        [SerializeField] private GameObject loadingIndicator;

        [Header("Navigation")]
        [SerializeField] private Button closeButton;            // ← revenir au panel

        // ── État interne ───────────────────────────────────────────────
        private bool _searching = false;

        // ── Unity lifecycle ────────────────────────────────────────────

        private void Awake()
        {
            // S'assurer que le CanvasGroup est prêt
            if (!canvasGroup)
                canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Start()
        {
            closeButton?.onClick.AddListener(OnCloseClicked);
            searchButton?.onClick.AddListener(OnSearchClicked);
            searchInput?.onSubmit.AddListener(_ => OnSearchClicked());

            // Cacher par défaut
            gameObject.SetActive(false);
        }

        // ── Public API ─────────────────────────────────────────────────

        /// <summary>Ouvre l'overlay avec une animation fade-in.</summary>
        public void Show()
        {
            gameObject.SetActive(true);

            if (canvasGroup)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, 0.25f).SetEase(Ease.OutCubic);
            }

            // Vider le champ et les résultats
            if (searchInput)
            {
                searchInput.text = "";
                searchInput.Select();
                searchInput.ActivateInputField();
            }

            ClearResults();

            if (noResultsText) noResultsText.gameObject.SetActive(false);
            if (loadingIndicator) loadingIndicator.SetActive(false);
        }

        /// <summary>Ferme l'overlay avec un fade-out.</summary>
        public void Hide()
        {
            if (!gameObject.activeSelf) return;

            EventSystem.current?.SetSelectedGameObject(null);

            if (canvasGroup)
            {
                canvasGroup.DOFade(0f, 0.2f)
                    .SetEase(Ease.InCubic)
                    .OnComplete(() => gameObject.SetActive(false));
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        // ── Boutons ────────────────────────────────────────────────────

        private void OnCloseClicked()
        {
            EventSystem.current?.SetSelectedGameObject(null);
            Hide();
        }

        private async void OnSearchClicked()
        {
            if (_searching) return;

            string query = searchInput ? searchInput.text.Trim() : "";

            if (string.IsNullOrEmpty(query) || query.Length < 2)
            {
                ToastManager.Show("Minimum 2 caractères", ToastType.Warning);
                return;
            }

            _searching = true;
            ClearResults();

            if (loadingIndicator) loadingIndicator.SetActive(true);
            if (noResultsText)    noResultsText.gameObject.SetActive(false);

            try
            {
                List<User> users = await APIService.Instance.SearchUsersAsync(query);

                if (loadingIndicator) loadingIndicator.SetActive(false);

                if (users == null || users.Count == 0)
                {
                    if (noResultsText)
                    {
                        noResultsText.text = $"Aucun résultat pour « {query} »";
                        noResultsText.gameObject.SetActive(true);
                    }
                }
                else
                {
                    foreach (User user in users)
                        SpawnResultItem(user);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AddFriendOverlay] Erreur recherche : {ex.Message}");
                if (loadingIndicator) loadingIndicator.SetActive(false);
                ToastManager.Show("Erreur lors de la recherche", ToastType.Error);
            }
            finally
            {
                _searching = false;
            }
        }

        // ── Construction résultats ─────────────────────────────────────

        private void SpawnResultItem(User user)
        {
            if (!resultItemPrefab || !resultsContainer) return;

            GameObject go = Instantiate(resultItemPrefab, resultsContainer);
            go.name = $"Result - {user.username}";

            AddFriendResultItem item = go.GetComponent<AddFriendResultItem>();
            if (item)
                item.Setup(user, OnSendRequestClicked);

            // Petite animation d'apparition
            CanvasGroup cg = go.GetComponent<CanvasGroup>();
            if (cg)
            {
                cg.alpha = 0f;
                float delay = resultsContainer.childCount * 0.05f;
                cg.DOFade(1f, 0.2f).SetDelay(delay).SetEase(Ease.OutCubic);
            }
        }

        private void ClearResults()
        {
            if (!resultsContainer) return;
            foreach (Transform child in resultsContainer)
                Destroy(child.gameObject);
        }

        // ── Envoi demande ──────────────────────────────────────────────

        private async void OnSendRequestClicked(string userId, AddFriendResultItem item)
        {
            item?.SetLoading(true);

            try
            {
                await APIService.Instance.SendFriendRequestAsync(userId);
                item?.SetSent();
                ToastManager.Show("Demande d'ami envoyée !", ToastType.Success);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[AddFriendOverlay] Erreur envoi demande : {ex.Message}");
                item?.SetLoading(false);
                ToastManager.Show("Erreur lors de l'envoi", ToastType.Error);
            }
        }

        // ── Cleanup ────────────────────────────────────────────────────

        private void OnDestroy()
        {
            closeButton?.onClick.RemoveAllListeners();
            searchButton?.onClick.RemoveAllListeners();
        }
    }
}

