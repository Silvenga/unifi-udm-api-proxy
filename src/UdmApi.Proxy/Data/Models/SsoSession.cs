using System;

namespace UdmApi.Proxy.Data.Models
{
    public class SsoSession
    {
        public int Id { get; set; }

        // TODO Should be unique and indexed.

        public string OriginalToken { get; set; }

        public string CurrentToken { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}