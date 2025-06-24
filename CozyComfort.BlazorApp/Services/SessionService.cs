using Microsoft.AspNetCore.Http;

namespace CozyComfort.BlazorApp.Services
{
    public class SessionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<SessionService> _logger;

        public SessionService(IHttpContextAccessor httpContextAccessor, ILogger<SessionService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public string GetSessionId()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                {
                    _logger.LogWarning("HttpContext is null when trying to get session ID");
                    return GenerateSessionId();
                }

                // Try to get existing session ID
                var sessionId = httpContext.Session.GetString("SessionId");

                if (string.IsNullOrEmpty(sessionId))
                {
                    // Generate new session ID
                    sessionId = GenerateSessionId();
                    httpContext.Session.SetString("SessionId", sessionId);
                    _logger.LogInformation("Generated new session ID: {SessionId}", sessionId);
                }

                return sessionId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting session ID");
                return GenerateSessionId();
            }
        }

        public void SetSessionId(string sessionId)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    httpContext.Session.SetString("SessionId", sessionId);
                    _logger.LogInformation("Set session ID: {SessionId}", sessionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting session ID");
            }
        }

        public void ClearSession()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    httpContext.Session.Clear();
                    _logger.LogInformation("Cleared session");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing session");
            }
        }

        public T? GetValue<T>(string key)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null) return default(T);

                var value = httpContext.Session.GetString(key);
                if (string.IsNullOrEmpty(value)) return default(T);

                if (typeof(T) == typeof(string))
                {
                    return (T)(object)value;
                }

                return System.Text.Json.JsonSerializer.Deserialize<T>(value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting session value for key: {Key}", key);
                return default(T);
            }
        }

        public void SetValue<T>(string key, T value)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null) return;

                string serializedValue;
                if (typeof(T) == typeof(string))
                {
                    serializedValue = value?.ToString() ?? string.Empty;
                }
                else
                {
                    serializedValue = System.Text.Json.JsonSerializer.Serialize(value);
                }

                httpContext.Session.SetString(key, serializedValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting session value for key: {Key}", key);
            }
        }

        public void RemoveValue(string key)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    httpContext.Session.Remove(key);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing session value for key: {Key}", key);
            }
        }

        private static string GenerateSessionId()
        {
            return Guid.NewGuid().ToString("N")[..16]; // Use first 16 characters for shorter ID
        }
    }
}