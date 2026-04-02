using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace NexA.Hub.Services
{
    /// <summary>
    /// Client STOMP léger au-dessus de NativeWebSocket.
    /// Gère le parsing et la construction de frames STOMP 1.2.
    /// </summary>
    public static class StompClient
    {
        // ── Frame building ─────────────────────────────────────────────

        /// <summary>Construit une frame STOMP CONNECT avec le JWT.</summary>
        public static string BuildConnectFrame(string token)
        {
            return "CONNECT\n" +
                   "accept-version:1.2\n" +
                   "heart-beat:10000,10000\n" +
                   "Authorization:Bearer " + token + "\n" +
                   "\n\0";
        }

        /// <summary>Construit une frame STOMP SUBSCRIBE.</summary>
        public static string BuildSubscribeFrame(string subscriptionId, string destination)
        {
            return "SUBSCRIBE\n" +
                   "id:" + subscriptionId + "\n" +
                   "destination:" + destination + "\n" +
                   "\n\0";
        }

        /// <summary>Construit une frame STOMP DISCONNECT.</summary>
        public static string BuildDisconnectFrame()
        {
            return "DISCONNECT\n" +
                   "receipt:disconnect-receipt\n" +
                   "\n\0";
        }

        // ── Frame parsing ──────────────────────────────────────────────

        /// <summary>Résultat du parsing d'une frame STOMP.</summary>
        public class StompFrame
        {
            public string Command;
            public Dictionary<string, string> Headers = new();
            public string Body;
        }

        /// <summary>Parse une frame STOMP brute reçue via WebSocket.</summary>
        public static StompFrame Parse(string raw)
        {
            if (string.IsNullOrEmpty(raw))
                return null;

            // Retirer le null terminator
            raw = raw.TrimEnd('\0');

            // Heartbeat (frame vide)
            if (string.IsNullOrWhiteSpace(raw))
                return new StompFrame { Command = "HEARTBEAT" };

            var frame = new StompFrame();
            int pos = 0;

            // Command = première ligne
            int lineEnd = raw.IndexOf('\n', pos);
            if (lineEnd < 0) return null;
            frame.Command = raw.Substring(pos, lineEnd - pos).Trim('\r');
            pos = lineEnd + 1;

            // Headers = lignes suivantes jusqu'à la ligne vide
            while (pos < raw.Length)
            {
                lineEnd = raw.IndexOf('\n', pos);
                if (lineEnd < 0) lineEnd = raw.Length;

                string line = raw.Substring(pos, lineEnd - pos).Trim('\r');
                pos = lineEnd + 1;

                if (string.IsNullOrEmpty(line))
                    break; // fin des headers

                int colon = line.IndexOf(':');
                if (colon > 0)
                {
                    string key = line.Substring(0, colon);
                    string val = line.Substring(colon + 1);
                    frame.Headers[key] = val;
                }
            }

            // Body = tout ce qui reste
            if (pos < raw.Length)
                frame.Body = raw.Substring(pos);

            return frame;
        }
    }
}