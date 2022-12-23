namespace Client
{
    public class User
    {
        public string identityProvider { get; set; }
        public string userId { get; set; }
        public string userDetails { get; set; }
        public string[] userRoles { get; set; }
        public Claim[] claims { get; set; }
    }

    public class Claim
    {
        public string typ { get; set; }
        public string val { get; set; }
    }
}
