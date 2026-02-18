using Android.Content.PM;
using ChibitsLink.main.cs.service;
using Microsoft.Maui.ApplicationModel;

namespace ChibitsLink.Platforms.Android;

public class OrientationService : IOrientationService
{
    public void SetLandscape()
    {
        var activity = Platform.CurrentActivity;
        if (activity != null)
        {
            activity.RequestedOrientation = ScreenOrientation.Landscape;
        }
    }

    public void SetPortrait()
    {
        var activity = Platform.CurrentActivity;
        if (activity != null)
        {
            activity.RequestedOrientation = ScreenOrientation.Portrait;
        }
    }
}
