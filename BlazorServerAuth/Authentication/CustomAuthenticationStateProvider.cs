﻿using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;

namespace BlazorServerAuth.Authentication
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private ProtectedSessionStorage _sessionStorage;
        private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        public CustomAuthenticationStateProvider(ProtectedSessionStorage sessionStorage)
        {
            _sessionStorage = sessionStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var userSessionStorageResult = await _sessionStorage.GetAsync<UserSession>("UserSession");
                var userSession = userSessionStorageResult.Success ? userSessionStorageResult.Value : null;
                if (userSession == null)
                {
                    return await Task.FromResult(new AuthenticationState(_anonymous));
                }

                var claimPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                {
                    new Claim(ClaimTypes.Name, userSession.UserId),
                   
                       
                }, "CustomAuth"));

                return await Task.FromResult(new AuthenticationState(claimPrincipal));
            }
            catch (Exception)
            {
                return await Task.FromResult(new AuthenticationState(_anonymous));
            }
        }

        public async Task UpdateAuthenticationState(UserSession userSession)
        {
            ClaimsPrincipal claimsPrincipal;

            if (userSession != null)
            {
                await _sessionStorage.SetAsync("UserSession", userSession);
                claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                {
                    new Claim(ClaimTypes.Name, userSession.UserId),
                }));
            }
            else
            {
                await _sessionStorage.DeleteAsync("UserSession");
                claimsPrincipal = _anonymous;
            }
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
        }
    }
}
