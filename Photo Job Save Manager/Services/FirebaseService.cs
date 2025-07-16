using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;
using Photo_Job_Save_Manager.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Photo_Job_Save_Manager.Services
{
    public class FirebaseService
    {
        private readonly string _firebaseUrl;
        private readonly string _authToken;
        private readonly string _userId;
        private readonly string _apiKey;
        private FirebaseClient _client;

        public FirebaseService(string authToken, string userId, IConfiguration configuration)
        {
            _authToken = authToken;
            _userId = userId;
            _apiKey = configuration["Firebase:ApiKey"];
            var projectId = configuration["Firebase:ProjectId"];
            _firebaseUrl = $"https://{projectId}.firebaseio.com/";
            _client = new FirebaseClient(_firebaseUrl);
        }

        // Authenticate user (Firebase Auth REST API)
        public static async Task<(string authToken, string userId)> AuthenticateAsync(string email, string password, string apiKey)
        {
            try
            {
                var client = new HttpClient();
                var request = new
                {
                    email = email,
                    password = password,
                    returnSecureToken = true
                };
                var response = await client.PostAsJsonAsync($"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={apiKey}", request);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync<FirebaseAuthResponse>();
                return (result.idToken, result.localId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] FirebaseService: Authentication error: {ex.Message}");
                throw;
            }
        }

        private class FirebaseAuthResponse
        {
            [JsonPropertyName("idToken")]
            public string idToken { get; set; }
            [JsonPropertyName("localId")]
            public string localId { get; set; }
        }

        // Upload a job to Firebase
        public async Task UploadJobAsync(Job job)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] FirebaseService: Uploading job {job.Id} to Firebase");
                await _client.Child("users").Child(_userId).Child("jobs").Child(job.Id).PutAsync(job);
                System.Diagnostics.Debug.WriteLine($"[DEBUG] FirebaseService: Successfully uploaded job {job.Id}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] FirebaseService: Error uploading job: {ex.Message}");
                throw;
            }
        }

        // Download a job from Firebase
        public async Task<Job> DownloadJobAsync(string jobId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] FirebaseService: Downloading job {jobId} from Firebase");
                var result = await _client.Child("users").Child(_userId).Child("jobs").Child(jobId).OnceAsync<Job>();
                var job = result.FirstOrDefault()?.Object;
                System.Diagnostics.Debug.WriteLine($"[DEBUG] FirebaseService: Successfully downloaded job {jobId}: {job != null}");
                return job;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] FirebaseService: Error downloading job: {ex.Message}");
                throw;
            }
        }

        // Upload a photo to Firebase Storage (stub)
        public async Task<string> UploadPhotoAsync(string localFilePath, string jobId, string fileName)
        {
            // TODO: Implement photo upload when Firebase Storage is properly configured
            await Task.Delay(1); // Placeholder
            return "placeholder_url";
        }
    }
} 