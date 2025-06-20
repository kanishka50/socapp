namespace CozyComfort.BlazorApp.Services
{
    public class SessionService
    {
        private string? _sessionId;

        public string GetSessionId()
        {
            if (string.IsNullOrEmpty(_sessionId))
            {
                _sessionId = Guid.NewGuid().ToString();
            }
            return _sessionId;
        }
    }
}