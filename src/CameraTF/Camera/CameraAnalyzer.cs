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

namespace MotoDetector.CameraAccess
{
    public class CameraAnalyzer
    {
        private const string ModelPlate = "plate_detector.tflite";
        private const string ModelMoto = "moto_detector.tflite";
        private const string ModelMotoModel = "moto_model_detector.tflite";

        public readonly CameraController cameraController;
        private readonly CameraEventsListener cameraEventListener;
        private Task processingTask;

        private int width;
        private int height;
        private int cDegrees;

        public SKBitmap input, inputRotated, inputRotatedCropped;

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

        private readonly FPSCounter cameraFPSCounter;
        private readonly FPSCounter processingFPSCounter;

        private readonly Stopwatch stopwatch;

        private bool canAnalyze = true;

        public CameraAnalyzer(CameraSurfaceView surfaceView)
        {
            cameraEventListener = new CameraEventsListener();
            cameraController = new CameraController(surfaceView, cameraEventListener);

            InitTensorflowLineService();

            var outputInfo = new SKImageInfo(
                TensorflowLiteService.ModelInputSize_PlateAndMoto,
                TensorflowLiteService.ModelInputSize_PlateAndMoto,
                SKColorType.Rgba8888);

            inputScaled_PlateAndMoto = new SKBitmap(outputInfo);

            colors_PlateAndMoto = inputScaled_PlateAndMoto.GetPixels();
            colorCount_PlateAndMoto = TensorflowLiteService.ModelInputSize_PlateAndMoto * TensorflowLiteService.ModelInputSize_PlateAndMoto;

            var outputInfo2 = new SKImageInfo(
                TensorflowLiteService.ModelInputSize_MotoModel,
                TensorflowLiteService.ModelInputSize_MotoModel,
                SKColorType.Rgba8888);

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
            cameraEventListener.OnPreviewFrameReady += HandleOnPreviewFrameReady;
            cameraController.SetupCamera();
        }

        public void RefreshCamera()
        {
            cameraController.RefreshCamera();
        }

        public void ShutdownCamera()
        {
            cameraController.ShutdownCamera();
        }

        private bool CanAnalyzeFrame
        {
            get
            {
                if (processingTask != null && !processingTask.IsCompleted)
                    return false;

                return true;
            }
        }

        private void HandleOnPreviewFrameReady(object sender, FastJavaByteArray fastArray)
        {
            cameraFPSCounter.Report();

            if (!CanAnalyzeFrame)
                return;

            processingFPSCounter.Report();

            processingTask = Task.Run(() =>
            {
                try
                {
                    DecodeFrame(fastArray);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }).ContinueWith(task =>
            {
                if (task.IsFaulted)
                    Debug.WriteLine("DecodeFrame exception occurs");
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        private unsafe void DecodeFrame(FastJavaByteArray fastArray)
        {
            if (input == null)
            {
                width = cameraController.LastCameraDisplayWidth;
                height = cameraController.LastCameraDisplayHeight;
                cDegrees = cameraController.LastCameraDisplayOrientationDegree;

                imageData = new int[width * height];
                imageGCHandle = GCHandle.Alloc(imageData, GCHandleType.Pinned);
                imageIntPtr = imageGCHandle.AddrOfPinnedObject();

                input = new SKBitmap(new SKImageInfo(width, height, SKColorType.Bgra8888));
                input.InstallPixels(input.Info, imageIntPtr);
            }

            if (inputRotated == null)
            {
                inputRotated = new SKBitmap(new SKImageInfo(height, width, SKColorType.Bgra8888));
            }

            if (inputRotatedCropped == null)
            {
                inputRotatedCropped = new SKBitmap(new SKImageInfo(height, height, SKColorType.Bgra8888));
            }

            RotateInputBitmap(input, cDegrees);
            CropInputBitmap(inputRotated);

            if (canAnalyze)
            {
                _ = Task.Factory.StartNew(() =>
                {

                    canAnalyze = false;

                    var pY = fastArray.Raw;
                    var pUV = pY + width * height;

                    stopwatch.Restart();

                    YuvHelper.ConvertYUV420SPToARGB8888(pY, pUV, (int*)imageIntPtr, width, height);

                    stopwatch.Stop();

                    MainActivity.PlateAndMotoStats.YUV2RGBElapsedMs = stopwatch.ElapsedMilliseconds;

                    stopwatch.Restart();

                    inputRotatedCropped.ScalePixels(inputScaled_PlateAndMoto, SKFilterQuality.None);
                    inputRotatedCropped.ScalePixels(inputScaled_MotoModel, SKFilterQuality.None);

                    stopwatch.Stop();

                    MainActivity.PlateAndMotoStats.ResizeAndRotateElapsedMs = stopwatch.ElapsedMilliseconds;

                    stopwatch.Restart();

                    tfService_Plate?.Recognize(colors_PlateAndMoto, colorCount_PlateAndMoto);
                    tfService_Moto?.Recognize(colors_PlateAndMoto, colorCount_PlateAndMoto);

                    stopwatch.Stop();

                    MainActivity.PlateAndMotoStats.InterpreterElapsedMs = stopwatch.ElapsedMilliseconds;

                    stopwatch.Restart();

                    tfService_MotoModel?.Recognize(colors_MotoModel, colorCount_MotoModel);

                    stopwatch.Stop();

                    MainActivity.MotoModelStats.InterpreterElapsedMs = stopwatch.ElapsedMilliseconds;

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
            using (var surface = new SKCanvas(inputRotatedCropped))
            {
                var w = input.Width;
                var h = input.Height;
                surface.DrawBitmap(bitmap, new SKRect(0, (h - w) / 2, w, (h - w) / 2 + w), new SKRect(0, 0, w, w));
            }
        }

        private void RotateInputBitmap(SKBitmap bitmap, int degrees)
        {
            using (var surface = new SKCanvas(inputRotated))
            {
                surface.Translate(input.Width / 2, input.Height / 2);
                surface.RotateDegrees(degrees);
                surface.Translate(-input.Width / 2, -input.Height / 2);
                surface.DrawBitmap(bitmap, 0, 0);
            }
        }

        private void InitTensorflowLineService()
        {
            using (var modelData = Application.Context.Assets.Open(ModelPlate))
            {
                tfService_Plate = new TensorflowLiteService { ModelType = 0 };
                tfService_Plate.Initialize(modelData, useNumThreads: true);
            }
            //using (var modelData = Application.Context.Assets.Open(ModelMoto))
            //{
            //    tfService_Moto = new TensorflowLiteService { ModelType = 1 };
            //    tfService_Moto.Initialize(modelData, useNumThreads: true);
            //}
            //using (var modelData = Application.Context.Assets.Open(ModelMotoModel))
            //{
            //    tfService_MotoModel = new TensorflowLiteService { ModelType = 2 };
            //    tfService_MotoModel.Initialize(modelData, useNumThreads: true);
            //}
        }
    }
}