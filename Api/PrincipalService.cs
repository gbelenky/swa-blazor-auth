using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

namespace Api
{
    public interface IPrincipalService
    {
        Task<ClaimsPrincipal> GetPrincipal(HttpRequest req);
    }

    public class PrincipalService : IPrincipalService
    {
        readonly ILogger<PrincipalService> _log;
        readonly IAuthenticatedGraphClientFactory _graphClientFactory;

        public PrincipalService(
            IAuthenticatedGraphClientFactory graphClientFactory,
            ILogger<PrincipalService> log
        )
        {
            _graphClientFactory = graphClientFactory;
            _log = log;
        }

        public async Task<ClaimsPrincipal> GetPrincipal(HttpRequest req)
        {
            try
            {
                MsClientPrincipal? principal = MakeMsClientPrincipal(req);

                if (principal == null)
                    return new ClaimsPrincipal();

                if (!principal.UserRoles?.Where(NotAnonymous).Any() ?? true)
                    return new ClaimsPrincipal();

                ClaimsIdentity identity = await MakeClaimsIdentity(principal);

                return new ClaimsPrincipal(identity);
            }
            catch (Exception e)
            {
                _log.LogError(e, "Error parsing claims principal");
                return new ClaimsPrincipal();
            }
        }

        MsClientPrincipal? MakeMsClientPrincipal(HttpRequest req)
        {
            MsClientPrincipal? principal = null;

            if (req.Headers.TryGetValue("x-ms-client-principal", out var header))
            {
                var data = header.FirstOrDefault();
                if (data != null)
                {
                    var decoded = Convert.FromBase64String(data);
                    var json = Encoding.UTF8.GetString(decoded);
                    _log.LogInformation($"x-ms-client-principal: {json}");
                    principal = JsonSerializer.Deserialize<MsClientPrincipal>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
            }

            return principal;
        }

        async Task<ClaimsIdentity> MakeClaimsIdentity(MsClientPrincipal principal)
        {
            var identity = new ClaimsIdentity(principal.IdentityProvider);

            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, principal.UserId!));
            identity.AddClaim(new Claim(ClaimTypes.Name, principal.UserDetails!));

            if (principal.UserRoles != null)
                identity.AddClaims(principal.UserRoles
                    .Where(NotAnonymous)
                    .Select(userRole => new Claim(ClaimTypes.Role, userRole)));

            var username = principal.UserDetails;
            if (username != null)
            {
                var userAppRoleAssignments = await GetUserAppRoleAssignments(username);
                identity.AddClaims(userAppRoleAssignments
                    .Select(userAppRoleAssignments => new Claim(ClaimTypes.Role, userAppRoleAssignments)));
            }

            return identity;
        }

        static bool NotAnonymous(string r) =>
            !string.Equals(r, "anonymous", StringComparison.CurrentCultureIgnoreCase);

        async Task<string[]> GetUserAppRoleAssignments(string username)
        {
            try
            {
                var (graphClient, clientId) = _graphClientFactory.GetAuthenticatedGraphClientAndClientId();
                _log.LogInformation("Getting AppRoleAssignments for {username}", username);

                var userRoleAssignments = await graphClient.Users[username]
                    .AppRoleAssignments
                    .Request()
                    .GetAsync();

                var roleIds = new List<string>();
                var pageIterator = PageIterator<AppRoleAssignment>
                    .CreatePageIterator(
                        graphClient,
                        userRoleAssignments,
                        // Callback executed for each item in the collection
                        (appRoleAssignment) =>
                        {
                            if (appRoleAssignment.AppRoleId.HasValue && appRoleAssignment.AppRoleId.Value != Guid.Empty)
                                roleIds.Add(appRoleAssignment.AppRoleId.Value.ToString());

                            return true;
                        },
                        // Used to configure subsequent page requests
                        (baseRequest) =>
                        {
                            // Re-add the header to subsequent requests
                            baseRequest.Header("Prefer", "outlook.body-content-type=\"text\"");
                            return baseRequest;
                        });

                await pageIterator.IterateAsync();

                var applications = await graphClient.Applications
                    .Request()
                    .Filter($"appId eq '{clientId}'") // we're only interested in the app that we're running as
                    .GetAsync();

                var appRoleAssignments = applications
                    .FirstOrDefault()
                    ?.AppRoles
                    ?.Where(appRole => appRole.Id.HasValue && roleIds.Contains(appRole.Id!.Value.ToString()))
                    .Select(appRole => appRole.Value)
                    .ToArray();

                return appRoleAssignments ?? Array.Empty<string>();
            }
            catch (Exception e)
            {
                _log.LogError(e, "Error getting AppRoleAssignments");
                return Array.Empty<string>();
            }
        }

        class MsClientPrincipal
        {
            public string? IdentityProvider { get; set; }
            public string? UserId { get; set; }
            public string? UserDetails { get; set; }
            public IEnumerable<string>? UserRoles { get; set; }
        }
    }
}