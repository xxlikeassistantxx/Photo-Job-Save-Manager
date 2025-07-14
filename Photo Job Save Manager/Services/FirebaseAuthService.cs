using Photo_Job_Save_Manager.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Photo_Job_Save_Manager.Services
{
    public class FirebaseAuthService : IAuthService
    {
        private readonly string _apiKey = "AIzaSyDYCKj1mp7GrEftKYPMnoXYrt6EwNsje6c";
        private readonly string _projectId = "photo-job-manager";
        private string? _idToken;
        private string? _refreshToken;
        private string? _localId;
        private bool _emailVerified;

        public async Task<bool> IsUserLoggedInAsync()
        {
            return !string.IsNullOrEmpty(_idToken);
        }

        public async Task<User?> GetCurrentUserAsync()
        {
            if (string.IsNullOrEmpty(_idToken) || string.IsNullOrEmpty(_localId))
                return null;

            var url = $"https://identitytoolkit.googleapis.com/v1/accounts:lookup?key={_apiKey}";
            var payload = new { idToken = _idToken };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var client = new HttpClient();
            var response = await client.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var result = JsonDocument.Parse(responseString);
                var user = result.RootElement.GetProperty("users")[0];
                return new User
                {
                    Id = user.GetProperty("localId").GetString() ?? string.Empty,
                    Email = user.GetProperty("email").GetString() ?? string.Empty,
                    Username = user.TryGetProperty("displayName", out var displayName) ? displayName.GetString() ?? string.Empty : string.Empty,
                    CreatedAt = user.TryGetProperty("createdAt", out var createdAt) ? DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(createdAt.GetString() ?? "0")).DateTime : DateTime.Now,
                    StorageUsed = 0,
                    IsEmailVerified = user.GetProperty("emailVerified").GetBoolean()
                };
            }
            return null;
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            var url = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={_apiKey}";
            var payload = new
            {
                email = email,
                password = password,
                returnSecureToken = true
            };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var client = new HttpClient();
            var response = await client.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var result = JsonDocument.Parse(responseString);
                _idToken = result.RootElement.GetProperty("idToken").GetString();
                _refreshToken = result.RootElement.GetProperty("refreshToken").GetString();
                _localId = result.RootElement.GetProperty("localId").GetString();

                // Now lookup user info to check emailVerified
                var lookupUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:lookup?key={_apiKey}";
                var lookupPayload = new { idToken = _idToken };
                var lookupJson = JsonSerializer.Serialize(lookupPayload);
                var lookupContent = new StringContent(lookupJson, Encoding.UTF8, "application/json");
                var lookupResponse = await client.PostAsync(lookupUrl, lookupContent);
                var lookupResponseString = await lookupResponse.Content.ReadAsStringAsync();
                if (lookupResponse.IsSuccessStatusCode)
                {
                    var lookupResult = JsonDocument.Parse(lookupResponseString);
                    var user = lookupResult.RootElement.GetProperty("users")[0];
                    _emailVerified = user.GetProperty("emailVerified").GetBoolean();
                    if (!_emailVerified)
                    {
                        throw new Exception("Please verify your email address before logging in.");
                    }
                    return true;
                }
                else
                {
                    throw new Exception("Failed to verify email status. Please try again.");
                }
            }
            else
            {
                try
                {
                    var error = JsonDocument.Parse(responseString).RootElement;
                    var message = error.GetProperty("error").GetProperty("message").GetString();
                    throw new Exception($"Login failed: {message}");
                }
                catch
                {
                    throw new Exception("Login failed: Unknown error.");
                }
            }
        }

        public async Task<bool> RegisterAsync(string username, string email, string password)
        {
            var url = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={_apiKey}";
            var payload = new
            {
                email = email,
                password = password,
                returnSecureToken = true
            };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var client = new HttpClient();
            var response = await client.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                // Send email verification
                var idToken = JsonDocument.Parse(responseString).RootElement.GetProperty("idToken").GetString();
                await SendEmailVerificationAsync(idToken);
                return true;
            }
            else
            {
                try
                {
                    var error = JsonDocument.Parse(responseString).RootElement;
                    var message = error.GetProperty("error").GetProperty("message").GetString();
                    string userMessage = message switch
                    {
                        "EMAIL_EXISTS" => "This email is already registered. Please use a different email or sign in.",
                        "OPERATION_NOT_ALLOWED" => "Password sign-in is disabled for this project.",
                        "TOO_MANY_ATTEMPTS_TRY_LATER" => "We have blocked all requests from this device due to unusual activity. Try again later.",
                        "WEAK_PASSWORD : Password should be at least 6 characters" => "Password is too weak. Please use a stronger password.",
                        _ => $"Registration failed: {message}"
                    };
                    throw new Exception(userMessage);
                }
                catch
                {
                    throw new Exception("Registration failed: Unknown error.");
                }
            }
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email)
        {
            var url = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={_apiKey}";
            var payload = new
            {
                requestType = "PASSWORD_RESET",
                email = email
            };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var client = new HttpClient();
            var response = await client.PostAsync(url, content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> VerifyEmailAsync()
        {
            if (string.IsNullOrEmpty(_idToken)) return false;
            return await SendEmailVerificationAsync(_idToken);
        }

        private async Task<bool> SendEmailVerificationAsync(string? idToken)
        {
            if (string.IsNullOrEmpty(idToken)) return false;
            var url = $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={_apiKey}";
            var payload = new
            {
                requestType = "VERIFY_EMAIL",
                idToken = idToken
            };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var client = new HttpClient();
            var response = await client.PostAsync(url, content);
            return response.IsSuccessStatusCode;
        }

        public async Task LogoutAsync()
        {
            _idToken = null;
            _refreshToken = null;
            _localId = null;
            _emailVerified = false;
        }

        public async Task<bool> IsEmailVerifiedAsync()
        {
            if (string.IsNullOrEmpty(_idToken)) return false;
            var url = $"https://identitytoolkit.googleapis.com/v1/accounts:lookup?key={_apiKey}";
            var payload = new { idToken = _idToken };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var client = new HttpClient();
            var response = await client.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var result = JsonDocument.Parse(responseString);
                var user = result.RootElement.GetProperty("users")[0];
                return user.GetProperty("emailVerified").GetBoolean();
            }
            return false;
        }
    }
} 