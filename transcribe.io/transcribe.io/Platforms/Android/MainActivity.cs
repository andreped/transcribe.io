// <copyright file="MainActivity.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using Android.App;
using Android.Content.PM;
using Android.OS;
using Android;
using AndroidX.Core.App;
using AndroidX.Core.Content;

namespace transcribe.io;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    const int RequestMicrophoneId = 1000;

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Check and request microphone permission at runtime
        if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.RecordAudio) != Permission.Granted)
        {
            ActivityCompat.RequestPermissions(this, new[] { Manifest.Permission.RecordAudio }, RequestMicrophoneId);
        }
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
    {
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        // Optionally handle permission result here
    }
}