using System;
using System.Threading.Tasks;
using NexA.Hub.Services;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NexA.Hub.Screens
{
    /// <summary>
    /// Classe de base pour tous les écrans du client
    /// Gère les animations d'entrée/sortie et les états de loading/error
    /// </summary>
    public abstract class ScreenBase : MonoBehaviour
    {
        public abstract ScreenType ScreenType { get; }

        /// <summary>
        /// Afficher l'écran (avec animations)
        /// </summary>
        public abstract Task ShowAsync(object data = null);

        /// <summary>
        /// Cacher l'écran (avec animations)
        /// </summary>
        public abstract Task HideAsync();

        /// <summary>
        /// Exécute une action async avec gestion automatique du loading/erreurs
        /// </summary>
        protected async Task ExecuteWithLoadingAsync(Func<Task> action)
        {
            try
            {
                ShowLoading(true);
                await action();
            }
            catch (AuthException ex)
            {
                ShowError(ex.Message);
                Debug.LogError($"[{ScreenType}] Auth error: {ex.Message}");
            }
            catch (APIException ex)
            {
                ShowError(ex.Message);
                Debug.LogError($"[{ScreenType}] API error: {ex.Code} - {ex.Message}");
            }
            catch (Exception ex)
            {

                ShowError("Une erreur est survenue. Veuillez réessayer.");
                Debug.LogError($"[{ScreenType}] Unexpected error: {ex}");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        protected virtual void ShowLoading(bool show)
        {
            // Override dans les classes enfants
        }

        protected virtual void ShowError(string message)
        {
            // Override dans les classes enfants
        }
    }
}

