using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using UdmApi.Proxy.Data;
using UdmApi.Proxy.Data.Models;

namespace UdmApi.Proxy.Sessions
{
    public class DatabaseSsoSessionCache : ISsoSessionCache
    {
        private readonly ApplicationContext _context;

        public DatabaseSsoSessionCache(ApplicationContext context)
        {
            _context = context;
        }

        public bool TryGet(string token, out string currentToken)
        {
            currentToken = _context.Sessions
                                   .Where(x => x.OriginalToken == token)
                                   .OrderByDescending(x => x.Id)
                                   .Select(x => x.CurrentToken)
                                   .FirstOrDefault();
            return currentToken != default;
        }

        public bool TryGet(string token, out string currentToken, out string csrfToken)
        {
            csrfToken = default;
            var result = TryGet(token, out currentToken);

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
            using var unit = _context.Database.BeginTransaction();
            var entity = _context.Sessions
                                 .Where(x => x.OriginalToken == token)
                                 .OrderByDescending(x => x.Id)
                                 .First();

            entity.CurrentToken = currentToken;
            _context.SaveChanges();
            unit.Commit();
        }

        public void Add(string token)
        {
            _context.Sessions.Add(new SsoSession
            {
                CreatedOn = DateTime.UtcNow,
                CurrentToken = token,
                OriginalToken = token
            });
            _context.SaveChanges();
        }
    }
}