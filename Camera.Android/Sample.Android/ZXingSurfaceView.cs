using System;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Graphics;
using ZXing.Mobile.CameraAccess;

namespace ZXing.Mobile
{
    public class ZXingSurfaceView : SurfaceView, ISurfaceHolderCallback, IScannerView
    {
        public ZXingSurfaceView(Context context)
            : base(context)
        {
            Init();
        }

        protected ZXingSurfaceView(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
            Init();
        }

		bool addedHolderCallback = false;

        private void Init()
        {
			if (_cameraAnalyzer == null)
	            _cameraAnalyzer = new CameraAnalyzer(this);

			_cameraAnalyzer.ResumeAnalysis();

			if (!addedHolderCallback) {
				Holder.AddCallback(this);
				Holder.SetType(SurfaceType.PushBuffers);
				addedHolderCallback = true;
			}
        }

        public async void SurfaceCreated(ISurfaceHolder holder)
        {
            await ZXing.Net.Mobile.Android.PermissionsHandler.PermissionRequestTask;

            _cameraAnalyzer.SetupCamera();

            _surfaceCreated = true;
        }

        public async void SurfaceChanged(ISurfaceHolder holder, Format format, int wx, int hx)
        {
            await ZXing.Net.Mobile.Android.PermissionsHandler.PermissionRequestTask;

            _cameraAnalyzer.RefreshCamera();
        }

        public async void SurfaceDestroyed(ISurfaceHolder holder)
        {
            await ZXing.Net.Mobile.Android.PermissionsHandler.PermissionRequestTask;

            try {
				if (addedHolderCallback) {
					Holder.RemoveCallback(this);
					addedHolderCallback = false;
				}
            } catch { }

            _cameraAnalyzer.ShutdownCamera();
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            var r = base.OnTouchEvent(e);

            switch (e.Action)
            {
                case MotionEventActions.Down:
                    return true;
                case MotionEventActions.Up:
                    var touchX = e.GetX();
                    var touchY = e.GetY();
                    this.AutoFocus((int)touchX, (int)touchY);
                    break;
            }

            return r;
        }

        public void AutoFocus()
        {
            _cameraAnalyzer.AutoFocus();
        }

        public void AutoFocus(int x, int y)
        {
            _cameraAnalyzer.AutoFocus(x, y);
        }

        public void StartScanning()
        {
            _cameraAnalyzer.ResumeAnalysis();
        }

        public void StopScanning()
        {
            _cameraAnalyzer.ShutdownCamera();
        }

        public void PauseAnalysis()
        {
            _cameraAnalyzer.PauseAnalysis();
        }

        public void ResumeAnalysis()
        {
            _cameraAnalyzer.ResumeAnalysis();
        }

        public void Torch(bool on)
        {
            if (on)
                _cameraAnalyzer.Torch.TurnOn();
            else
                _cameraAnalyzer.Torch.TurnOff();
        }

        public void ToggleTorch()
        {
            _cameraAnalyzer.Torch.Toggle();
        }

        public bool IsTorchOn => _cameraAnalyzer.Torch.IsEnabled;

        public bool IsAnalyzing => _cameraAnalyzer.IsAnalyzing;

        private CameraAnalyzer _cameraAnalyzer;
        private bool _surfaceCreated;

        public bool HasTorch => _cameraAnalyzer.Torch.IsSupported;

		protected override void OnAttachedToWindow()
		{
			base.OnAttachedToWindow();

			// Reinit things
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
            // SurfaceCreated/SurfaceChanged are not called on a resume
            await ZXing.Net.Mobile.Android.PermissionsHandler.PermissionRequestTask;

            //only refresh the camera if the surface has already been created. Fixed #569
            if (_surfaceCreated)
                _cameraAnalyzer.RefreshCamera();
        }
    }
}