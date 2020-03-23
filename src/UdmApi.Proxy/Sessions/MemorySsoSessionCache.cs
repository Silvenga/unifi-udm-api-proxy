using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;

namespace UdmApi.Proxy.Sessions
{
    public interface ISsoSessionCache
    {
        bool TryGet(string token, out string currentToken);
        bool TryGet(string token, out string currentToken, out string csrfToken);
        void Update(string token, string currentToken);
        void Add(string token);
    }

    public class MemorySsoSessionCache : ISsoSessionCache
    {
        private readonly ConcurrentDictionary<string, string> _sessions = new ConcurrentDictionary<string, string>();

        public bool TryGet(string token, out string currentToken)
        {
            return _sessions.TryGetValue(token, out currentToken);
        }

        public bool TryGet(string token, out string currentToken, out string csrfToken)
        {
            csrfToken = default;
            var result = _sessions.TryGetValue(token, out currentToken);

            if (result)
            {
                var jwt = new JwtSecurityToken(currentToken);
                if (jwt.Payload.TryGetValue("csrfToken", out var csrfTokenValue)
                    && csrfTokenValue is string csrfTokenStr)
                {
                    csrfToken = csrfTokenStr;
                }
            }

            return result;
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