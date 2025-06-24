namespace CozyComfort.BlazorApp.Services
{
    public class SimpleSessionService
    {
        private static readonly string _sessionId = "demo-session-" + DateTime.Now.Ticks.ToString()[^8..];

        public string GetSessionId()
        {
            return _sessionId;
        }
    }
}