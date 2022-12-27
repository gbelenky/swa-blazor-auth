namespace Client
{

    public class User
    {
        public Clientprincipal clientPrincipal { get; set; }
    }

    public class Clientprincipal
    {
        public string userId { get; set; }
        public string[] userRoles { get; set; }
        public Claim[] claims { get; set; }
        public string identityProvider { get; set; }
        public string userDetails { get; set; }
    }

    public class Claim
    {
        public string typ { get; set; }
        public string val { get; set; }
    }

}
