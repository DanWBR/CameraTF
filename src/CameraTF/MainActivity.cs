using Android.App;
using Android.Content.PM;
using Android.Views;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using Android.Support.V7.App;
using SkiaSharp.Views.Android;
using MotoDetector.Helpers;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using MotoDetector.Classes;
using SkiaSharp;

namespace MotoDetector
{
    [Activity(
        MainLauncher = true,
        Theme = "@style/AppTheme.NoActionBar",
        ConfigurationChanges = ConfigChanges.KeyboardHidden | ConfigChanges.ScreenLayout)]
    public class MainActivity : AppCompatActivity
    {

        public static Activity context;

        private string[] labels_plate = { "???", "plate" };
        private string[] labels_moto = { "???", "moto" };

        public static Stats PlateAndMotoStats = new Stats();
        public static Stats MotoModelStats = new Stats();

        public static float ctodratio = 1.0f;

        private static SKCanvasView canvasView;

        private static CameraSurfaceView cameraSurface;

        public static Dictionary<string, Moto> MotosList;

        public static List<string> MotoLabels;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            MotosList = Motos.Get();

            LoadModelLabels();

            AppCenter.Start("6ef03cf3-90e1-4478-a070-4446c0e3e78c", typeof(Analytics), typeof(Crashes));

            this.RequestWindowFeature(WindowFeatures.NoTitle);

            this.Window.AddFlags(WindowManagerFlags.Fullscreen);
            this.Window.AddFlags(WindowManagerFlags.KeepScreenOn);

            SetContentView(Resource.Layout.mainwindow);

            cameraSurface = new CameraSurfaceView(this);
            canvasView = new SKCanvasView(this);
            canvasView.PaintSurface += Canvas_PaintSurface;

            var mainView = this.FindViewById<FrameLayout>(Resource.Id.frameLayoutMain);
            mainView.AddView(canvasView);
            mainView.AddView(cameraSurface);

            var mainLayout = this.FindViewById<LinearLayout>(Resource.Id.linearLayoutMain);
            mainLayout.BringToFront();
            canvasView.BringToFront();

            context = this;

        }

        private void LoadModelLabels()
        {
            using (var labelData = Application.Context.Assets.Open("moto_model_detector.txt"))
            {
                using (var reader = new StreamReader(labelData))
                {
                    var text = reader.ReadToEnd();
                    MotoLabels = text
                        .Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .ToList();
                }
            }
        }

        public static void ReloadCanvas()
        {
            canvasView.PostInvalidate();
        }

        private void Canvas_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;

            var canvasWidth = e.Info.Width;
            var canvasHeight = e.Info.Height;

            var cratio = (float)cameraSurface.cameraAnalyzer.cameraController.LastCameraDisplayHeight / (float)cameraSurface.cameraAnalyzer.cameraController.LastCameraDisplayWidth;
            var pratio = cratio / ctodratio;

            canvas.Clear();

            var stats = PlateAndMotoStats;
            var labels = labels_moto;

            var leftMargin = 5;
            var bottomMargin = 5;

            var recHeight = 250;

            if (cameraSurface.cameraAnalyzer.inputRotatedCropped != null)
            {
                canvas.DrawBitmap(cameraSurface.cameraAnalyzer.inputRotatedCropped, 0, 300);
            }

            DrawingHelper.DrawBackgroundRectangle(
                canvas,
                canvasWidth,
                recHeight,
                0,
                canvasHeight - recHeight);

            DrawingHelper.DrawText(
                canvas,
                leftMargin,
                canvasHeight - 0 - bottomMargin,
                $"Camera: {stats.CameraFps} fps ({stats.CameraMs} ms)");

            DrawingHelper.DrawText(
                canvas,
                leftMargin,
                canvasHeight - 50 - bottomMargin,
                $"Processing: {stats.ProcessingFps} fps ({stats.ProcessingMs} ms)");

            DrawingHelper.DrawText(
                canvas,
                leftMargin,
                canvasHeight - 100 - bottomMargin,
                $"YUV2RGB: {stats.YUV2RGBElapsedMs} ms");

            DrawingHelper.DrawText(
                canvas,
                leftMargin,
                canvasHeight - 150 - bottomMargin,
                $"ResizeAndRotate: {stats.ResizeAndRotateElapsedMs} ms");

            DrawingHelper.DrawText(
                canvas,
                leftMargin,
                canvasHeight - 200 - bottomMargin,
                $"TFRecognize: {stats.InterpreterElapsedMs} ms");

            if (stats.Scores2 == null)
            {
                for (var i = 0; i < stats.NumDetections; i++)
                {
                    var score = stats.Scores[i];
                    var labelIndex = (int)stats.Labels[i];
                    var xmin = stats.BoundingBoxes[i * 4 + 0];
                    var ymin = stats.BoundingBoxes[i * 4 + 1];
                    var xmax = stats.BoundingBoxes[i * 4 + 2];
                    var ymax = stats.BoundingBoxes[i * 4 + 3];

                    //if (labelIndex == 0) continue;
                    if (score < 0.5) continue;

                    var left = ymin * canvasWidth;
                    var top = (canvasHeight - canvasWidth) / 2 + xmin * canvasWidth;
                    var right = ymax * canvasWidth;
                    var bottom = (canvasHeight - canvasWidth) / 2 + xmax * canvasWidth;

                    left *= pratio;
                    top *= pratio;
                    right *= pratio;
                    bottom *= pratio;

                    DrawingHelper.DrawBoundingBox(
                        canvas,
                        left,
                        top,
                        right,
                        bottom);

                    var label = labels[1];
                    DrawingHelper.DrawText(canvas, left, bottom, $"{label} - {score}");
                }
            }
            else
            {
                DrawingHelper.DrawText2(canvas, 20, 340, MotoLabels[stats.Scores2.ToList().IndexOf(stats.Scores2.Max())]);
            }


        }

        protected async override void OnResume()
        {
            base.OnResume();

            if (PermissionsHandler.NeedsPermissionRequest(this))
                await PermissionsHandler.RequestPermissionsAsync(this);
        }

        public override void OnRequestPermissionsResult(
            int requestCode,
            string[] permissions,
            [GeneratedEnum] Permission[] grantResults)
        {
            PermissionsHandler.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}