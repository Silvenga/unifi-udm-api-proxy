namespace UdmApi.Proxy.Sessions
{
    public interface ISsoSessionCache
    {
        bool TryGet(string token, out string currentToken);
        bool TryGet(string token, out string currentToken, out string csrfToken);
        void Update(string token, string currentToken);
        void Add(string token);
    }
}