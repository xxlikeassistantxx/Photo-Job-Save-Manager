# Photo Job Save Manager

A comprehensive photo job management application built with .NET MAUI and Firebase integration.

## Features

### Authentication System
- **Login Screen**: Secure authentication with email/password
- **Registration**: New user registration with email verification
- **Password Reset**: Forgot password functionality with email reset
- **Auto Logout**: Automatic logout after 1 year of inactivity
- **User Feedback**: Comprehensive error handling and user feedback

### Job Management
- **Job Types**: Create custom job types with configurable fields
- **Photo Storage**: Secure photo storage using Firebase Storage
- **Job Details**: Store customer information, pricing, status, and more
- **Search & Filter**: Find and organize jobs efficiently

## Setup Instructions

### 1. Firebase Configuration

1. Create a new Firebase project at [Firebase Console](https://console.firebase.google.com/)
2. Enable Authentication with Email/Password provider
3. Enable Firebase Storage
4. Get your Firebase configuration:
   - Go to Project Settings
   - Copy the API Key and Project ID
   - Update the `appsettings.json` file with your Firebase credentials

### 2. Update Firebase Settings

Edit `appsettings.json`:
```json
{
  "Firebase": {
    "ApiKey": "your-actual-api-key",
    "ProjectId": "your-project-id",
    "AuthDomain": "your-project-id.firebaseapp.com",
    "StorageBucket": "your-project-id.appspot.com"
  }
}
```

### 3. Build and Run

```bash
dotnet build
dotnet run
```

## Architecture

This application follows the MVVM (Model-View-ViewModel) pattern:

### Models
- `User.cs`: User authentication and profile data
- `JobType.cs`: Job type definitions with configurable fields
- `Job.cs`: Individual job entries with photos and data

### Services
- `IAuthService.cs`: Authentication interface
- `FirebaseAuthService.cs`: Firebase authentication implementation

### ViewModels
- `BaseViewModel.cs`: Common ViewModel functionality
- `LoginViewModel.cs`: Login screen logic
- `RegisterViewModel.cs`: Registration screen logic
- `ForgotPasswordViewModel.cs`: Password reset logic

### Views
- `LoginPage.xaml`: Login screen UI
- `RegisterPage.xaml`: Registration screen UI
- `ForgotPasswordPage.xaml`: Password reset screen UI

## Security Features

- **Password Requirements**: 8-17 characters, 1 uppercase, 1 number, 1 special character
- **Email Verification**: Required email verification for new accounts
- **Secure Storage**: Firebase Storage for photo and data security
- **Session Management**: Automatic logout after extended inactivity

## User Experience

- **Real-time Feedback**: Loading indicators and status messages
- **Form Validation**: Comprehensive input validation with helpful error messages
- **Responsive Design**: Works across all supported platforms
- **Dark/Light Theme**: Automatic theme switching based on system preferences

## Next Steps

The authentication system is now complete. The next phase will include:

1. **Main Dashboard**: Job type management and overview
2. **Job Type Creation**: Custom field configuration
3. **Job Entry**: Photo capture and data entry
4. **Storage Management**: Firebase Storage integration for photos
5. **Search & Filter**: Job organization and retrieval

## Dependencies

- .NET MAUI 9.0
- Firebase.Auth
- Firebase.Storage
- CommunityToolkit.Maui
- Microsoft.Extensions.DependencyInjection

## Support

For issues or questions, please check the Firebase documentation or create an issue in this repository. 