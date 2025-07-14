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

namespace Photo_Job_Save_Manager.Services
{
    public class FirebaseService
    {
        private readonly string _firebaseUrl = "https://<your-project-id>.firebaseio.com/"; // Set from appsettings.json
        private readonly string _authToken; // Set after login
        private readonly string _userId; // Set after login
        private FirebaseClient _client;

        public FirebaseService(string authToken, string userId, string firebaseUrl)
        {
            _authToken = authToken;
            _userId = userId;
            _firebaseUrl = firebaseUrl;
            _client = new FirebaseClient(_firebaseUrl);
        }

        // Authenticate user (Firebase Auth REST API)
        public static async Task<(string authToken, string userId)> AuthenticateAsync(string email, string password, string apiKey)
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
            await _client.Child("users").Child(_userId).Child("jobs").Child(job.Id).PutAsync(job);
        }

        // Download a job from Firebase
        public async Task<Job> DownloadJobAsync(string jobId)
        {
            var result = await _client.Child("users").Child(_userId).Child("jobs").Child(jobId).OnceAsync<Job>();
            return result.FirstOrDefault()?.Object;
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