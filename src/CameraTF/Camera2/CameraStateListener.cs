using Android.App;
using Android.Hardware.Camera2;

namespace MotoDetector.Listeners
{
    public class CameraStateListener : CameraDevice.StateCallback
    {
        private readonly CameraController2 owner;

        public CameraStateListener(CameraController2 owner)
        {
            if (owner == null)
                throw new System.ArgumentNullException("owner");
            this.owner = owner;
        }

        public override void OnOpened(CameraDevice camera)
        {
            // This method is called when the camera is opened.  We start camera preview here.
            owner.mCameraOpenCloseLock.Release();
            owner.mCameraDevice = camera;
            owner.CreateCameraPreviewSession();
        }

        public override void OnDisconnected(CameraDevice camera)
        {
            owner.mCameraOpenCloseLock.Release();
            camera.Close();
            owner.mCameraDevice = null;
        }

        public override void OnError(CameraDevice camera, CameraError error)
        {
            owner.mCameraOpenCloseLock.Release();
            camera.Close();
            owner.mCameraDevice = null;
            if (owner == null)
                return;
            var activity = MainActivity.context;
            if (activity != null)
            {
                activity.Finish();
            }
        }
    }
}