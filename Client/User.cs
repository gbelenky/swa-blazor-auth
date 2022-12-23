namespace Client
{
    public class User
    {
        public string identityProvider { get; set; }
        public string userId { get; set; }
        public string userDetails { get; set; }
        public string userRoles { get; set; }
        public (string, string)[] claims { get; set; }
    }
}
