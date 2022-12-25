namespace Client
{

    public class User
    {
        public Clientprincipal clientPrincipal { get; set; }
    }

    public class Clientprincipal
    {
        public string identityProvider { get; set; }
        public string userId { get; set; }
        public string userDetails { get; set; }
        public string[] userRoles { get; set; }
    }

}
