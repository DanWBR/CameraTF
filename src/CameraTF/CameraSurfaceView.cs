using System;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Graphics;
using MotoDetector.CameraAccess;

namespace MotoDetector
{
    public class CameraSurfaceView : TextureView
    {
        private bool surfaceCreated;

        public CameraAnalyzer cameraAnalyzer;

        public CameraSurfaceView(Context context)
            : base(context)
        {
            Init();
        }

        protected CameraSurfaceView(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
            Init();
        }

        private void Init()
        {
            if (cameraAnalyzer == null) cameraAnalyzer = new CameraAnalyzer(this);

        }

        public void Restart()
        {
            cameraAnalyzer.Stop();
            cameraAnalyzer.cameraController.mSurfaceTextureListener.ClearHandlers();
            cameraAnalyzer.cameraController.mSurfaceTextureListener = null;
            cameraAnalyzer = null;
            cameraAnalyzer = new CameraAnalyzer(this);
        }

        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();

            Init();
        }

        protected override void OnWindowVisibilityChanged(ViewStates visibility)
        {
            base.OnWindowVisibilityChanged(visibility);
            if (visibility == ViewStates.Visible)
                Init();
        }

        public override async void OnWindowFocusChanged(bool hasWindowFocus)
        {
            base.OnWindowFocusChanged(hasWindowFocus);

            if (!hasWindowFocus) return;

            await PermissionsHandler.PermissionRequestTask;

            if (surfaceCreated)
            {
                //cameraAnalyzer.RefreshCamera();
            }
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            var width = ResolveSize(SuggestedMinimumWidth, widthMeasureSpec);
            var height = ResolveSize(SuggestedMinimumHeight, heightMeasureSpec);
            var displayheight = Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Height;
            var displaywidth = Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Width;
            var dratio = displayheight / displaywidth;
            var cratio = (float)height / (float)width;
            MainActivity.ctodratio = (float)(dratio / cratio);
            SetMeasuredDimension((int)((double)width * dratio / cratio), height);
            //base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
        }

    }
}
