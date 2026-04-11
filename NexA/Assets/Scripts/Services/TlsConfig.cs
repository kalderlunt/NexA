using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

namespace NexA.Hub.Services
{
    public static class TlsConfig
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
#if UNITY_EDITOR
            ServicePointManager.ServerCertificateValidationCallback = (_, _, _, errors) =>
                errors == SslPolicyErrors.None || errors == SslPolicyErrors.RemoteCertificateChainErrors;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.DefaultConnectionLimit = 50;
#endif
        }
    }
}