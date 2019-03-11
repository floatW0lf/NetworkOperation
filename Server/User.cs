using System;

namespace NetworkOperation
{
    public class User
    {
        public long Id { get; set; }
        public long Token { get; set; }
        public string UniqData { get; set; }
        public DateTime LastActivity { get; set; }
    }
}