namespace Api.Auth;

public class Identity
{
    public string identityProvider { get; set; } = String.Empty;
    public string userId { get; set; } = String.Empty;
    public string userDetails { get; set; } = String.Empty;
    public string[] userRoles { get; set; } = new string[0];

}
