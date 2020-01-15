using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Android.App;
using Android.Views;
using ApxLabs.FastAndroidCamera;
using MotoDetector.Helpers;
using SkiaSharp;
using Microsoft.AppCenter.Crashes;

namespace MotoDetector.CameraAccess
{
    public class CameraAnalyzer
    {
        private const string ModelPlate = "plate_detector.tflite";
        private const string ModelMoto = "moto_detector.tflite";
        private const string ModelMotoModel = "moto_model_detector.tflite";

        public readonly CameraController2 cameraController;
        //private readonly CameraEventsListener cameraEventListener;

        private int width;
        private int height;
        private int cDegrees;

        public SKBitmap input, inputCropped;

        private IntPtr colors_PlateAndMoto;
        private int colorCount_PlateAndMoto;

        private IntPtr colors_MotoModel;
        private int colorCount_MotoModel;

        public SKBitmap inputScaled_PlateAndMoto;

        public SKBitmap inputScaled_MotoModel;

        private int[] imageData;
        private GCHandle imageGCHandle;
        private IntPtr imageIntPtr;

        private TensorflowLiteService tfService_Plate;
        private TensorflowLiteService tfService_Moto;
        private TensorflowLiteService tfService_MotoModel;

        public static FPSCounter cameraFPSCounter;
        public static FPSCounter processingFPSCounter;

        private readonly Stopwatch stopwatch;

        public static bool canAnalyze = true;

        private bool STOP = false;

        public void Stop()
        {
            STOP = true;
        }

        public CameraAnalyzer(CameraSurfaceView surfaceView)
        {

            cameraController = new CameraController2();
            cameraController.Init(surfaceView);

            cameraController.mSurfaceTextureListener.OnPreviewFrameReady += HandleOnPreviewFrameReady;

            InitTensorflowLineService();

            var outputInfo = new SKImageInfo(
                TensorflowLiteService.ModelInputSize_PlateAndMoto,
                TensorflowLiteService.ModelInputSize_PlateAndMoto,
                SKColorType.Bgra8888);

            inputScaled_PlateAndMoto = new SKBitmap(outputInfo);

            colors_PlateAndMoto = inputScaled_PlateAndMoto.GetPixels();
            colorCount_PlateAndMoto = TensorflowLiteService.ModelInputSize_PlateAndMoto * TensorflowLiteService.ModelInputSize_PlateAndMoto;

            var outputInfo2 = new SKImageInfo(
                TensorflowLiteService.ModelInputSize_MotoModel,
                TensorflowLiteService.ModelInputSize_MotoModel,
                SKColorType.Bgra8888);

            inputScaled_MotoModel = new SKBitmap(outputInfo2);

            colors_MotoModel = inputScaled_MotoModel.GetPixels();
            colorCount_MotoModel = TensorflowLiteService.ModelInputSize_MotoModel * TensorflowLiteService.ModelInputSize_MotoModel;

            stopwatch = new Stopwatch();

            cameraFPSCounter = new FPSCounter((x) =>
            {
                MainActivity.PlateAndMotoStats.CameraFps = x.fps;
                MainActivity.PlateAndMotoStats.CameraMs = x.ms;
                MainActivity.MotoModelStats.CameraFps = x.fps;
                MainActivity.MotoModelStats.CameraMs = x.ms;
            });

            processingFPSCounter = new FPSCounter((x) =>
            {
                MainActivity.PlateAndMotoStats.ProcessingFps = x.fps;
                MainActivity.PlateAndMotoStats.ProcessingMs = x.ms;
                MainActivity.MotoModelStats.ProcessingFps = x.fps;
                MainActivity.MotoModelStats.ProcessingMs = x.ms;
            });

        }

        public void SetupCamera()
        {
            cameraController.OpenCamera();
        }

        public void ShutdownCamera()
        {
            cameraController.CloseCamera();
        }

        private void HandleOnPreviewFrameReady(IntPtr address, int w, int h)
        {

            if (!canAnalyze) return;

            processingFPSCounter.Report();
            try
            {
                DecodeFrame(address, w, h);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private unsafe void DecodeFrame(IntPtr address, int w, int h)
        {
            if (input == null)
            {
                width = w;
                height = h;
                cDegrees = cameraController.mSensorOrientation;

                imageData = new int[width * height];
                imageGCHandle = GCHandle.Alloc(imageData, GCHandleType.Pinned);
                imageIntPtr = imageGCHandle.AddrOfPinnedObject();

                input = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul));
            }

            input.InstallPixels(input.Info, address);

            if (inputCropped == null)
            {
                inputCropped = new SKBitmap(new SKImageInfo(width, width, SKColorType.Rgba8888, SKAlphaType.Premul));
            }

            CropInputBitmap(input);

            if (canAnalyze)
            {
                Task.Factory.StartNew(() =>
                {

                    canAnalyze = false;

                    stopwatch.Restart();

                    inputCropped.ScalePixels(inputScaled_PlateAndMoto, SKFilterQuality.None);
                    inputCropped.ScalePixels(inputScaled_MotoModel, SKFilterQuality.None);

                    stopwatch.Stop();

                    MainActivity.PlateAndMotoStats.ResizeAndRotateElapsedMs = stopwatch.ElapsedMilliseconds;

                    stopwatch.Restart();

                    colors_PlateAndMoto = inputScaled_PlateAndMoto.GetPixels();
                    colors_MotoModel = inputScaled_MotoModel.GetPixels();

                    tfService_Plate?.Recognize(colors_PlateAndMoto, colorCount_PlateAndMoto);
                    //tfService_Moto?.Recognize(colors_PlateAndMoto, colorCount_PlateAndMoto);

                    stopwatch.Stop();

                    MainActivity.PlateAndMotoStats.InterpreterElapsedMs = stopwatch.ElapsedMilliseconds;

                    stopwatch.Restart();

                    tfService_MotoModel?.Recognize(colors_MotoModel, colorCount_MotoModel);

                    stopwatch.Stop();

                    MainActivity.MotoModelStats.InterpreterElapsedMs = stopwatch.ElapsedMilliseconds;

                    //canAnalyze = true;

                    //MainActivity.context.RunOnUiThread(() =>
                    //{
                    //    MainActivity.ReloadCanvas();
                    //});

                }).ContinueWith((t) =>
                {

                    if (t.Exception != null) Crashes.TrackError(t.Exception);

                    canAnalyze = true;

                    MainActivity.context.RunOnUiThread(() =>
                    {
                        MainActivity.ReloadCanvas();
                    });
                });

            }

        }

        private void CropInputBitmap(SKBitmap bitmap)
        {
            using (var surface = new SKCanvas(inputCropped))
            {
                surface.Clear(SKColors.LightGreen);
                var w = input.Width;
                var h = input.Height;
                surface.DrawBitmap(bitmap, new SKRect(0, (h - w) / 2, w, (h - w) / 2 + w), new SKRect(0, 0, w, w));
            }
        }

        private void InitTensorflowLineService()
        {

            switch (MainActivity.SavedDataStore.SelectedDetector)
            {
                case Classes.SaveData.DetectorType.LicensePlates:
                    using (var modelData = Application.Context.Assets.Open(ModelPlate))
                    {
                        tfService_Plate = new TensorflowLiteService { ModelType = 0 };
                        tfService_Plate.Initialize(modelData, useNumThreads: true);
                    }
                    break;
                case Classes.SaveData.DetectorType.MotorcycleModels:
                    using (var modelData = Application.Context.Assets.Open(ModelMotoModel))
                    {
                        tfService_MotoModel = new TensorflowLiteService { ModelType = 2 };
                        tfService_MotoModel.Initialize(modelData, useNumThreads: true);
                    }
                    break;
            }
            //using (var modelData = Application.Context.Assets.Open(ModelMoto))
            //{
            //    tfService_Moto = new TensorflowLiteService { ModelType = 1 };
            //    tfService_Moto.Initialize(modelData, useNumThreads: true);
            //}
        }
    }
}