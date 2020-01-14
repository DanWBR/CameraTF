using System.IO;
using SkiaSharp;
using SkiaSharp.Views.Android;
using Android.Graphics;
using Android.Views;
using System;
using System.Threading.Tasks;
using MotoDetector.CameraAccess;

namespace MotoDetector.Listeners
{
    public class Camera2BasicSurfaceTextureListener : Java.Lang.Object, TextureView.ISurfaceTextureListener
    {
        private readonly CameraController2 owner;

        public event Action<IntPtr, int, int> OnPreviewFrameReady;

        DateTime lastAnalysis = DateTime.Now;  // controlling the pace of the machine vision analysis
        DateTime lastCall = DateTime.Now;  // controlling the pace of the machine vision analysis

        public Camera2BasicSurfaceTextureListener(CameraController2 owner)
        {
            if (owner == null)
                throw new System.ArgumentNullException("owner");
            this.owner = owner;
        }

        public void ClearHandlers()
        {
            OnPreviewFrameReady = null;
        }

        public void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
        {
            owner.OpenCamera(width, height);
        }

        public bool OnSurfaceTextureDestroyed(SurfaceTexture surface)
        {
            return true;
        }

        public void OnSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height)
        {
            owner.ConfigureTransform(width, height);
        }

        public void OnSurfaceTextureUpdated(SurfaceTexture surface)
        {

            if (OnPreviewFrameReady == null) return;

            CameraAnalyzer.cameraFPSCounter.Report();

            var dfps = 1;

            switch (MainActivity.SavedDataStore.DetectionFPS)
            {
                case Classes.SaveData.DetectorFPS.FPS1:
                    dfps = 1;
                    break;
                case Classes.SaveData.DetectorFPS.FPS5:
                    dfps = 5;
                    break;
                case Classes.SaveData.DetectorFPS.FPS10:
                    dfps = 10;
                    break;
                case Classes.SaveData.DetectorFPS.FPS15:
                    dfps = 15;
                    break;
                case Classes.SaveData.DetectorFPS.FPS30:
                    dfps = 30;
                    break;
                case Classes.SaveData.DetectorFPS.FPS60:
                    dfps = 60;
                    break;
            }

            TimeSpan pace = new TimeSpan(0, 0, 0, 0, 1000 / dfps); // in milliseconds, classification will not repeat faster than this value

            var currentDate = DateTime.Now;
            var interval = currentDate - lastAnalysis;

            // control the pace of the machine vision to protect battery life

            if (interval >= pace)
            {
                lastAnalysis = currentDate;
            }
            else
            {
                return; // don't run the classifier more often than we need
            }

            if (CameraAnalyzer.canAnalyze)
            {
                _ = Task.Factory.StartNew(() =>
                {
                    using (var frame = Bitmap.CreateBitmap(owner.mTextureView.Width,
                            owner.mTextureView.Height,
                            Bitmap.Config.Argb8888))
                    {
                        owner.mTextureView.GetBitmap(frame);
                        using (var bmpframe = frame.ToSKBitmap())
                        {
                            var info = new SKImageInfo((int)(bmpframe.Width * 0.5), (int)(bmpframe.Height * 0.5));
                            using (var resized = bmpframe.Resize(info, SKFilterQuality.Low))
                            {
                                OnPreviewFrameReady.Invoke(resized.GetPixels(), info.Width, info.Height);
                            }
                        }
                    }
                });
            }


        }

    }
}