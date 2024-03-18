using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.VisualScripting;
using UnityEngine;

public static class Authenticator 
{
    public static AuthState currentAuth = AuthState.NotAuthorized;
    
    public static async Task<AuthState> Authenticate(int maxTries) {
        if (currentAuth == AuthState.Authorized) return AuthState.Authorized;

        int tries = 0;

        AuthState authorizing;
        while (tries < maxTries) 
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            if (AuthenticationService.Instance.IsSignedIn && AuthenticationService.Instance.IsAuthorized) 
            {
                Debug.LogWarning("We have been authorized");
                currentAuth = AuthState.Authorized;
                break;

            }
            tries++;
            await Task.Delay(1000);
        }
        return currentAuth;
    }
    
    
    public enum AuthState {
        NotAuthorized,
        Authorizing,
        Authorized
    }
    
    
}
