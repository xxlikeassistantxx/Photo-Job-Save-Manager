using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Photo_Job_Save_Manager.Services
{
    public class FirebaseConnectivityTest
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public FirebaseConnectivityTest(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(15);
        }

        public async Task<ConnectivityTestResult> TestFirebaseConnectivityAsync()
        {
            var result = new ConnectivityTestResult();
            
            try
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] FirebaseConnectivityTest: Starting comprehensive connectivity test...");
                
                // Test 1: Configuration
                result.ConfigurationTest = await TestConfigurationAsync();
                
                // Test 2: Authentication API
                result.AuthApiTest = await TestAuthApiAsync();
                
                // Test 3: Realtime Database
                result.DatabaseTest = await TestRealtimeDatabaseAsync();
                
                // Test 4: Network connectivity
                result.NetworkTest = await TestNetworkConnectivityAsync();
                
                System.Diagnostics.Debug.WriteLine($"[DEBUG] FirebaseConnectivityTest: Test completed. Overall status: {result.IsOverallSuccess}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] FirebaseConnectivityTest: Test failed with exception: {ex.Message}");
                result.OverallError = ex.Message;
            }
            
            return result;
        }

        private async Task<TestResult> TestConfigurationAsync()
        {
            var result = new TestResult { TestName = "Configuration" };
            
            try
            {
                var apiKey = _configuration["Firebase:ApiKey"];
                var projectId = _configuration["Firebase:ProjectId"];
                var authDomain = _configuration["Firebase:AuthDomain"];
                var storageBucket = _configuration["Firebase:StorageBucket"];
                
                System.Diagnostics.Debug.WriteLine($"[DEBUG] FirebaseConnectivityTest: Configuration values:");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] FirebaseConnectivityTest: - API Key: {(!string.IsNullOrEmpty(apiKey) ? "Present" : "Missing")}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] FirebaseConnectivityTest: - Project ID: {projectId ?? "Missing"}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] FirebaseConnectivityTest: - Auth Domain: {authDomain ?? "Missing"}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] FirebaseConnectivityTest: - Storage Bucket: {storageBucket ?? "Missing"}");
                
                if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(projectId))
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = "Missing required Firebase configuration values";
                    return result;
                }
                
                result.IsSuccess = true;
                result.Details = $"API Key: Present, Project ID: {projectId}";
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
            }
            
            return result;
        }

        private async Task<TestResult> TestAuthApiAsync()
        {
            var result = new TestResult { TestName = "Authentication API" };
            
            try
            {
                var apiKey = _configuration["Firebase:ApiKey"];
                var url = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={apiKey}";
                
                var payload = new
                {
                    email = "test@test.com",
                    password = "testpassword",
                    returnSecureToken = true
                };
                
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                System.Diagnostics.Debug.WriteLine($"[DEBUG] FirebaseConnectivityTest: Testing Auth API at: {url}");
                
                var response = await _httpClient.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();
                
                System.Diagnostics.Debug.WriteLine($"[DEBUG] FirebaseConnectivityTest: Auth API response status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] FirebaseConnectivityTest: Auth API response: {responseString}");
                
                // We expect a 400 error for invalid credentials, which means the API is reachable
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var errorResponse = JsonDocument.Parse(responseString);
                    var error = errorResponse.RootElement.GetProperty("error");
                    var message = error.GetProperty("message").GetString();
                    
                    if (message?.Contains("INVALID_PASSWORD") == true || message?.Contains("EMAIL_NOT_FOUND") == true)
                    {
                        result.IsSuccess = true;
                        result.Details = "Authentication API is reachable (expected error for test credentials)";
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.ErrorMessage = $"Unexpected error: {message}";
                    }
                }
                else if (response.IsSuccessStatusCode)
                {
                    result.IsSuccess = true;
                    result.Details = "Authentication API is working (unexpected success with test credentials)";
                }
                else
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = $"HTTP {response.StatusCode}: {responseString}";
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
            }
            
            return result;
        }

        private async Task<TestResult> TestRealtimeDatabaseAsync()
        {
            var result = new TestResult { TestName = "Realtime Database" };
            
            try
            {
                var projectId = _configuration["Firebase:ProjectId"];
                // Try both old and new Firebase Realtime Database URL formats
                var databaseUrls = new[]
                {
                    $"https://{projectId}-default-rtdb.firebaseio.com/.json",
                    $"https://{projectId}.firebaseio.com/.json"
                };
                
                foreach (var databaseUrl in databaseUrls)
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] FirebaseConnectivityTest: Testing Realtime Database at: {databaseUrl}");
                    
                    var response = await _httpClient.GetAsync(databaseUrl);
                    var responseString = await response.Content.ReadAsStringAsync();
                    
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] FirebaseConnectivityTest: Database response status: {response.StatusCode}");
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] FirebaseConnectivityTest: Database response: {responseString}");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        result.IsSuccess = true;
                        result.Details = $"Realtime Database is accessible at {databaseUrl}";
                        return result;
                    }
                }
                
                // If we get here, both URLs failed
                result.IsSuccess = false;
                result.ErrorMessage = "Realtime Database not accessible. Please check if the database is enabled in your Firebase project.";
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
            }
            
            return result;
        }

        private async Task<TestResult> TestNetworkConnectivityAsync()
        {
            var result = new TestResult { TestName = "Network Connectivity" };
            
            try
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] FirebaseConnectivityTest: Testing general network connectivity...");
                
                // Test basic internet connectivity
                var response = await _httpClient.GetAsync("https://www.google.com");
                
                if (response.IsSuccessStatusCode)
                {
                    result.IsSuccess = true;
                    result.Details = "General internet connectivity is working";
                }
                else
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = $"Cannot reach external sites: HTTP {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
            }
            
            return result;
        }
    }

    public class ConnectivityTestResult
    {
        public TestResult ConfigurationTest { get; set; } = new();
        public TestResult AuthApiTest { get; set; } = new();
        public TestResult DatabaseTest { get; set; } = new();
        public TestResult NetworkTest { get; set; } = new();
        public string? OverallError { get; set; }
        
        public bool IsOverallSuccess => 
            ConfigurationTest.IsSuccess && 
            AuthApiTest.IsSuccess && 
            DatabaseTest.IsSuccess && 
            NetworkTest.IsSuccess &&
            string.IsNullOrEmpty(OverallError);
    }

    public class TestResult
    {
        public string TestName { get; set; } = "";
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Details { get; set; }
    }
} 