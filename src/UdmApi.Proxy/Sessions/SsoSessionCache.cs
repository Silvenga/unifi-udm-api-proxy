using System.Collections.Concurrent;

namespace UdmApi.Proxy.Sessions
{
    public interface ISsoSessionCache
    {
        bool TryGet(string token, out string currentToken);
        void Update(string token, string currentToken);
        void Add(string token);
    }

    public class SsoSessionCache : ISsoSessionCache
    {
        private readonly ConcurrentDictionary<string, string> _sessions = new ConcurrentDictionary<string, string>();

        public bool TryGet(string token, out string currentToken)
        {
            return _sessions.TryGetValue(token, out currentToken);
        }

        public void Update(string token, string currentToken)
        {
            _sessions.AddOrUpdate(token, currentToken, (key, oldValue) => currentToken);
        }

        public void Add(string token)
        {
            _sessions.AddOrUpdate(token, token, (key, oldValue) => token);
        }
    }
}