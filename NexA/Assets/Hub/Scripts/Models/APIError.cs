using System;

namespace NexA.Hub.Models
{
    // ============================================
    // ERROR MODELS
    // ============================================

    [Serializable]
    public class APIErrorResponse
    {
        public bool success;
        public ErrorData error;
    }

    [Serializable]
    public class ErrorData
    {
        public string code;
        public string message;
        public object details;
    }
}

