using Android.App;
using Android.Content.PM;
using Android.Views;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using Android.Support.V7.App;
using SkiaSharp.Views.Android;
using MotoDetector.Helpers;
using System.Collections.Generic;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using MotoDetector.Classes;
using SkiaSharp;
using System.Diagnostics;
using System.Timers;
using Android.Graphics;
using Android.Hardware.Camera2;
using Android.Content;
using Java.Lang;
using s = DWSIMSimulator_Android.Code.Shared;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using System;
using System.Linq;
using System.IO;

namespace MotoDetector
{
    [Activity(
        MainLauncher = true,
        Theme = "@style/MyTheme",
        ConfigurationChanges = ConfigChanges.KeyboardHidden | ConfigChanges.ScreenLayout)]
    public class MainActivity : AppCompatActivity
    {

        // common

        public static int WIDTH = 1080;
        public static int HEIGHT = 1920;
        public static string RESID = "1080p";

        public static int FPS = 30;
        public static int DETFPS = 1;
        public static int MAXFPS = 120;

        public static bool PRO_MODE = true;

        public static float SCORE_THRESHOLD = 0.4f;

        public static SaveData SavedDataStore;

        public static List<string> MotoLabels;

        public static Dictionary<string, Moto> MotosList;

        public static string DetectedMotoModel = "";

        public static string DetectedPlate = "";

        public static int NumberOfCameras = 1;

        // android

        public static Activity context;

        public static Stats PlateAndMotoStats = new Stats();
        public static Stats MotoModelStats = new Stats();

        public static float ctodratio = 1.0f;

        private static SKCanvasView canvasView;

        public static CameraSurfaceView cameraSurface;

        protected override void OnCreate(Bundle bundle)
        {

            base.OnCreate(bundle);

            LoadModelLabels();

            var manager = (CameraManager)this.GetSystemService(Context.CameraService);

            var numCameras = manager.GetCameraIdList().Length;

            if (numCameras == 0)
            {
                s.MessageBox(this, "Erro", "Infelizmente o MotoDetector não funcionará no seu dispositivo, " +
                    "pois ele não suporta a API Camera2.", null);
            }

            var cameraIDs = new List<string>();

            for (var i = 0; i < numCameras; i++)
            {
                var cameraId = manager.GetCameraIdList()[i];
                var characteristics = manager.GetCameraCharacteristics(cameraId);

                // We don't use a front facing camera in this sample.
                var facing = (Integer)characteristics.Get(CameraCharacteristics.LensFacing);
                if (facing != null && facing == (Integer.ValueOf((int)LensFacing.Front)))
                {
                }
                else
                {
                    cameraIDs.Add(cameraId);
                }
            }

            NumberOfCameras = cameraIDs.Count;

            SavedDataStore = SaveData.Load();

            if (SavedDataStore == null) SavedDataStore = new SaveData();

            if (SavedDataStore.Camera == "") SavedDataStore.Camera = cameraIDs[0];

            context = this;

            MotosList = Motos.Get();

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

            var proc = System.Diagnostics.Process.GetCurrentProcess();

            var rb1 = this.FindViewById<RadioButton>(Resource.Id.radioButton1);
            var rb2 = this.FindViewById<RadioButton>(Resource.Id.radioButton2);
            var rb3 = this.FindViewById<RadioButton>(Resource.Id.radioButton3);
            var rb4 = this.FindViewById<RadioButton>(Resource.Id.radioButton4);

            if (NumberOfCameras == 1)
            {
                rb2.Enabled = false;
                rb3.Enabled = false;
                rb4.Enabled = false;
            }
            else if (NumberOfCameras == 2)
            {
                rb3.Enabled = false;
                rb4.Enabled = false;
            }
            else if (NumberOfCameras == 3)
            {
                rb4.Enabled = false;
            }

            switch (cameraIDs.IndexOf(SavedDataStore.Camera))
            {
                case 0:
                    rb1.Selected = true;
                    break;
                case 1:
                    rb2.Selected = true;
                    break;
                case 2:
                    rb3.Selected = true;
                    break;
                case 3:
                    rb4.Selected = true;
                    break;
            }

            var rg = this.FindViewById<RadioGroup>(Resource.Id.radioGroup1);

            rg.CheckedChange += (s, e) =>
            {
                if (rb1.Id.Equals(e.CheckedId))
                {
                    SavedDataStore.Camera = cameraIDs[0];
                }
                else if (rb2.Id.Equals(e.CheckedId))
                {
                    SavedDataStore.Camera = cameraIDs[1];
                }
                else if (rb3.Id.Equals(e.CheckedId))
                {
                    SavedDataStore.Camera = cameraIDs[2];
                }
                else
                {
                    SavedDataStore.Camera = cameraIDs[3];
                }
                SavedDataStore.Save();
                ConfigureCapture();
            };

            var btnInfo = this.FindViewById<ImageButton>(Resource.Id.btnInfo);

            btnInfo.Click += (sender, e) =>
            {
                DisplayMotoDetails();
            };

            var btnSettings = this.FindViewById<ImageButton>(Resource.Id.imageButtonSettings);

            btnSettings.Click += (sender, e) =>
            {
                DisplaySettingsView();
            };

            var imgwarning = MainActivity.context.FindViewById<ImageButton>(Resource.Id.imageButtonWarning);
            imgwarning.Visibility = ViewStates.Gone;

            imgwarning.Click += (sender, e) =>
            {
                this.RunOnUiThread(() =>
                {
                    imgwarning.Visibility = ViewStates.Gone; var layout = MainActivity.context.FindViewById<LinearLayout>(Resource.Id.linearLayoutLabelPanel);
                    layout.SetBackgroundColor(Android.Graphics.Color.ParseColor("#bc000000"));
                    var txtView = MainActivity.context.FindViewById<TextView>(Resource.Id.txtObjID);
                    txtView.SetTextColor(Android.Graphics.Color.White);
                });
            };

            var zoomslider = MainActivity.context.FindViewById<SeekBar>(Resource.Id.seekBarZoom);

            zoomslider.ProgressChanged += (s, e) =>
            {
                var characteristics = manager.GetCameraCharacteristics(SavedDataStore.Camera);
                float maxZoom = (float)characteristics.Get(CameraCharacteristics.ScalerAvailableMaxDigitalZoom);
                float currentZoom = 1.0f;
                if (zoomslider.Progress >= 100)
                {
                    currentZoom = maxZoom;
                }
                else
                {
                    currentZoom = (float)zoomslider.Progress / 100.0f * maxZoom;
                }
                var controller = cameraSurface.cameraAnalyzer.cameraController;
                Rect m = (Rect)characteristics.Get(CameraCharacteristics.SensorInfoActiveArraySize);
                var w = (m.Right - m.Left) / 2.5;
                var h = (m.Bottom - m.Top) / 2.5;
                var factor = currentZoom / maxZoom;
                Rect m1 = new Rect((int)(m.Left + w * factor), (int)(m.Top + h * factor), (int)(m.Right - w * factor), (int)(m.Bottom - h * factor));
                controller.mPreviewRequestBuilder.Set(CaptureRequest.ScalerCropRegion, m1);
                controller.mCameraCaptureSessionCallback.OnConfigured(controller.mCaptureSession);
            };

            var zoomexposure = MainActivity.context.FindViewById<SeekBar>(Resource.Id.seekBarExposure);

            zoomexposure.ProgressChanged += (s, e) =>
            {
                var characteristics = manager.GetCameraCharacteristics(SavedDataStore.Camera);
                float value = (float)zoomexposure.Progress;
                var range1 = (Android.Util.Range)characteristics.Get(CameraCharacteristics.ControlAeCompensationRange);
                int minExposure = (int)range1.Lower;
                int maxExposure = (int)range1.Upper;
                int adjustedvalue = (int)(value * ((float)maxExposure - (float)minExposure) / 200.0f);
                var controller = cameraSurface.cameraAnalyzer.cameraController;
                controller.mPreviewRequestBuilder.Set(CaptureRequest.ControlAeExposureCompensation, adjustedvalue);
                controller.mCameraCaptureSessionCallback.OnConfigured(controller.mCaptureSession);
            };

            var sw = new Stopwatch();

            sw.Start();

            var tim = new Timer();

            tim.Interval = 1000;

            tim.Elapsed += (s, e) =>
            {
                var cpu = (proc.TotalProcessorTime.TotalMilliseconds / sw.ElapsedMilliseconds) * 100;
                var mem = proc.WorkingSet64 / 1024 / 1024;
                this.RunOnUiThread(() =>
                {
                    var txtCPU = this.FindViewById<TextView>(Resource.Id.textViewCPU);
                    var txtMEM = this.FindViewById<TextView>(Resource.Id.textViewMEM);
                    var txtFPS = this.FindViewById<TextView>(Resource.Id.textViewFPS);
                    var txtAFPS = this.FindViewById<TextView>(Resource.Id.textViewAFPS);
                    txtCPU.SetTextColor(Color.White);
                    if (cpu < 40) txtCPU.SetTextColor(Color.Green);
                    if (cpu > 40) txtCPU.SetTextColor(Color.Yellow);
                    if (cpu > 80) txtCPU.SetTextColor(Color.Red);
                    txtCPU.Text = cpu.ToString("N2") + "%";
                    txtMEM.Text = mem.ToString() + " MB";
                    txtAFPS.Text = PlateAndMotoStats.ProcessingFps.ToString("N0") + " (DET) / " + PlateAndMotoStats.CameraFps.ToString("N0") + " (CAPT) FPS";
                    txtFPS.Text = RESID + " / " + FPS + " FPS";
                });
            };

            tim.Start();

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

            canvas.Clear();

            var stats = PlateAndMotoStats;

            //var recHeight = 250;

            //if (cameraSurface.cameraAnalyzer.inputCropped != null)
            //{
            //    canvas.DrawBitmap(cameraSurface.cameraAnalyzer.inputCropped, 0, 300);
            //}

            //DrawingHelper.DrawBackgroundRectangle(canvas, canvasWidth, recHeight, 0, canvasHeight - recHeight);

            if (stats.Scores2 == null)
            {
                for (var i = 0; i < stats.NumDetections; i++)
                {
                    var score = stats.Scores[i];
                    var xmin = stats.BoundingBoxes[i * 4 + 0] * ctodratio;
                    var ymin = stats.BoundingBoxes[i * 4 + 1] * ctodratio;
                    var xmax = stats.BoundingBoxes[i * 4 + 2] * ctodratio;
                    var ymax = stats.BoundingBoxes[i * 4 + 3] * ctodratio;

                    if (score < MainActivity.SCORE_THRESHOLD) continue;

                    var left = ymin * canvasWidth;
                    var top = (canvasHeight - canvasWidth) / 2 + xmin * canvasWidth;
                    var right = ymax * canvasWidth;
                    var bottom = (canvasHeight - canvasWidth) / 2 + xmax * canvasWidth;

                    DrawingHelper.DrawBoundingBox(canvas, left, top, right, bottom);

                }
            }

        }

        protected override void OnPause()
        {
            var controller = cameraSurface.cameraAnalyzer.cameraController;
            controller.CloseCamera();
            controller.StopBackgroundThread();
            base.OnPause();
        }

        protected async override void OnResume()
        {

            base.OnResume();

            if (PermissionsHandler.NeedsPermissionRequest(this)) await PermissionsHandler.RequestPermissionsAsync(this);

            var controller = cameraSurface.cameraAnalyzer.cameraController;

            controller.StartBackgroundThread();

            // When the screen is turned off and turned back on, the SurfaceTexture is already
            // available, and "onSurfaceTextureAvailable" will not be called. In that case, we can open
            // a camera and start preview from here (otherwise, we wait until the surface is ready in
            // the SurfaceTextureListener).
            if (controller.mTextureView.IsAvailable)
            {
                controller.OpenCamera(controller.mTextureView.Width, controller.mTextureView.Height);
            }
            else
            {
                controller.mTextureView.SurfaceTextureListener = controller.mSurfaceTextureListener;
            }

        }

        public override void OnRequestPermissionsResult(
            int requestCode,
            string[] permissions,
            [GeneratedEnum] Permission[] grantResults)
        {
            PermissionsHandler.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private void UpdateCameraSettings()
        {
            switch (SavedDataStore.SelectedResolution)
            {
                case SaveData.CaptureResolution.Res480p:
                    HEIGHT = 720;
                    WIDTH = 480;
                    RESID = "480p";
                    break;
                case SaveData.CaptureResolution.Res720p:
                    HEIGHT = 1280;
                    WIDTH = 720;
                    RESID = "720p";
                    break;
                case SaveData.CaptureResolution.Res1080p:
                    HEIGHT = 1920;
                    WIDTH = 1080;
                    RESID = "1080p (FHD)";
                    break;
                case SaveData.CaptureResolution.Res2160p:
                    HEIGHT = 3840;
                    WIDTH = 2160;
                    RESID = "4K (UHD)";
                    break;
            }

            switch (SavedDataStore.SelectedFPS)
            {
                case SaveData.CaptureFPS.FPS15:
                    FPS = 15;
                    break;
                case SaveData.CaptureFPS.FPS30:
                    FPS = 30;
                    break;
                case SaveData.CaptureFPS.FPS60:
                    FPS = 60;
                    break;
                case SaveData.CaptureFPS.FPS120:
                    FPS = 120;
                    break;
            }

            switch (SavedDataStore.DetectionFPS)
            {
                case SaveData.DetectorFPS.FPS1:
                    DETFPS = 1;
                    break;
                case SaveData.DetectorFPS.FPS5:
                    DETFPS = 5;
                    break;
                case SaveData.DetectorFPS.FPS10:
                    DETFPS = 10;
                    break;
                case SaveData.DetectorFPS.FPS15:
                    DETFPS = 15;
                    break;
                case SaveData.DetectorFPS.FPS30:
                    DETFPS = 30;
                    break;
                case SaveData.DetectorFPS.FPS60:
                    DETFPS = 60;
                    break;
            }
        }

        private void DisplayMotoDetails()
        {

            if (DetectedPlate == "" && DetectedMotoModel == "") return;

            var alert = new AlertDialog.Builder(this);
            var view = new BlankView(this);
            alert.SetView(view);

            var lID = Resource.Id.BlankLayoutContainer;

            if (DetectedPlate != "")
            {

                s.CreateAndAddLabelBoxRow(view, lID, "VERIFICAR PLACA");
                s.CreateAndAddLabelBoxRow(view, lID, "Placa: " + DetectedPlate);
                s.CreateAndAddDescriptionRow2(view, lID, "Insira a placa detectada nos campos abaixo para verificar marca, modelo e outras informações do veículo.");

                s.CreateAndAddDescriptionRow2(view, lID, "");

                var url = "https://cidadao.sinesp.gov.br/sinesp-cidadao/";

                var web = new Android.Webkit.WebView(this);

                web.LoadUrl(url);

                LinearLayout.LayoutParams param2 = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 1.0f);
                param2.LeftMargin = (int)(5 + 0.5f);
                param2.RightMargin = (int)(5 + 0.5f);
                param2.TopMargin = (int)(5 + 0.5f);
                param2.BottomMargin = (int)(5 + 0.5f);
                param2.Weight = 1.0f;

                web.LayoutParameters = param2;

                var layout = view.FindViewById<LinearLayout>(lID);

                layout.AddView(web);

            }
            else
            {
                if (MotosList.ContainsKey(DetectedMotoModel))
                {
                    s.CreateAndAddLabelBoxRow(view, lID, "INFORMAÇÕES");

                    var moto = MotosList[DetectedMotoModel];

                    s.CreateAndAddDescriptionRow(view, lID, "Fabricante", moto.Fabricante);
                    s.CreateAndAddDescriptionRow(view, lID, "Modelo", moto.Modelo);
                    s.CreateAndAddDescriptionRow(view, lID, "Ciclo", moto.Ciclo);
                    s.CreateAndAddDescriptionRow(view, lID, "Cilindrada (cm3)", moto.Volume);
                    s.CreateAndAddDescriptionRow(view, lID, "Potência (cv)", moto.Potencia);
                    s.CreateAndAddDescriptionRow(view, lID, "Peso (kg)", moto.Peso);
                    s.CreateAndAddDescriptionRow(view, lID, "Peso/Potência (kg/cv)", moto.Peso_Pot);
                    s.CreateAndAddDescriptionRow(view, lID, "Comprimento (mm)", moto.Comprimento);
                    s.CreateAndAddDescriptionRow(view, lID, "Largura (mm)", moto.Largura);
                    s.CreateAndAddDescriptionRow(view, lID, "Altura (mm)", moto.Altura);
                    s.CreateAndAddDescriptionRow(view, lID, "Altura do Assento (mm)", moto.Altura_Assento);

                }
            }

            alert.SetCancelable(true);
            alert.SetPositiveButton("OK", (sender, e2) =>
            {
            });
            alert.Create().Show();

        }

        private void DisplaySettingsView()
        {

            var alert = new AlertDialog.Builder(this);
            var view = new BlankView(this);
            alert.SetView(view);

            var lID = Resource.Id.BlankLayoutContainer;

            string[] res, fps, dfps;

            if (PRO_MODE)
            {
                res = new[] { "480p", "720p", "1080p", "4K" };
                fps = new[] { "15", "30", "60", "120" };
                dfps = new[] { "1", "5", "10", "15", "30", "60" };
            }
            else
            {
                res = new[] { "480p", "720p" };
                fps = new[] { "15", "30" };
                dfps = new[] { "1", "5" };
            }

            s.CreateAndAddLabelBoxRow(view, lID, "CONFIGURAÇÕES DE CAPTURA");

            s.CreateAndAddDescriptionRow2(view, lID, "Selecione a resolução e a velocidade de captura de vídeo. A velocidade " +
                "serve apenas como referência - o valor real será determinado pelas capacidades do seu dispositivo.");

            s.CreateAndAddSpinnerRow(view, lID, "Resolução", res,
                (int)SavedDataStore.SelectedResolution, (o, e, re) =>
                {
                    switch (re.SelectedItemPosition)
                    {
                        case 0:
                            SavedDataStore.SelectedResolution = SaveData.CaptureResolution.Res480p;
                            break;
                        case 1:
                            SavedDataStore.SelectedResolution = SaveData.CaptureResolution.Res720p;
                            break;
                        case 2:
                            SavedDataStore.SelectedResolution = SaveData.CaptureResolution.Res1080p;
                            break;
                        case 3:
                            SavedDataStore.SelectedResolution = SaveData.CaptureResolution.Res2160p;
                            break;
                    }
                });

            s.CreateAndAddSpinnerRow(view, lID, "Frames por Segundo (FPS)", fps,
                (int)SavedDataStore.SelectedFPS, (o, e, re) =>
                {
                    switch (re.SelectedItemPosition)
                    {
                        case 0:
                            SavedDataStore.SelectedFPS = SaveData.CaptureFPS.FPS15;
                            break;
                        case 1:
                            SavedDataStore.SelectedFPS = SaveData.CaptureFPS.FPS30;
                            break;
                        case 2:
                            SavedDataStore.SelectedFPS = SaveData.CaptureFPS.FPS60;
                            break;
                        case 3:
                            SavedDataStore.SelectedFPS = SaveData.CaptureFPS.FPS120;
                            break;
                    }
                });


            s.CreateAndAddLabelBoxRow(view, lID, "DETECTORES");

            s.CreateAndAddDescriptionRow2(view, lID, "Selecione o Detector desejado conforme sua necessidade. Você também pode " +
                "habilitar ou desabilitar a reprodução de som e vibração ao encontrar um objeto válido.");

            var options = new[] { "Modelos de Motocicletas", "Placas de Veículos" };

            s.CreateAndAddSpinnerRow(view, lID, "Detector", options, (int)SavedDataStore.SelectedDetector, (o, e, re) =>
            {
                switch (re.SelectedItemPosition)
                {
                    case 0:
                        SavedDataStore.SelectedDetector = SaveData.DetectorType.MotorcycleModels;
                        break;
                    case 1:
                        SavedDataStore.SelectedDetector = SaveData.DetectorType.LicensePlates;
                        break;
                }
            });

            s.CreateAndAddCheckBoxRow(view, lID, "Vibrar", SavedDataStore.Vibrate, (o, e, be) =>
                {
                    SavedDataStore.Vibrate = be.Checked;
                });
            s.CreateAndAddCheckBoxRow(view, lID, "Reproduzir Som", SavedDataStore.PlaySound, (o, e, be) =>
            {
                SavedDataStore.PlaySound = be.Checked;
            });

            s.CreateAndAddSpinnerRow(view, lID, "Velocidade de Análise (FPS)", dfps,
                (int)SavedDataStore.DetectionFPS, (o, e, re) =>
               {
                   switch (re.SelectedItemPosition)
                   {
                       case 0:
                           SavedDataStore.DetectionFPS = SaveData.DetectorFPS.FPS1;
                           break;
                       case 1:
                           SavedDataStore.DetectionFPS = SaveData.DetectorFPS.FPS5;
                           break;
                       case 2:
                           SavedDataStore.DetectionFPS = SaveData.DetectorFPS.FPS10;
                           break;
                       case 3:
                           SavedDataStore.DetectionFPS = SaveData.DetectorFPS.FPS15;
                           break;
                       case 4:
                           SavedDataStore.DetectionFPS = SaveData.DetectorFPS.FPS30;
                           break;
                       case 5:
                           SavedDataStore.DetectionFPS = SaveData.DetectorFPS.FPS60;
                           break;
                   }
               });

            s.CreateAndAddLabelBoxRow(view, lID, "PLACAS ROUBADAS OU CLONADAS");

            s.CreateAndAddButtonRow(view, lID, "Lista de Placas", (o, e, b) =>
            {

                var alert2 = new AlertDialog.Builder(this);
                var view2 = new BlankView(this);
                alert2.SetView(view2);

                string plates = "";

                foreach (var p in SavedDataStore.PlateNumbers)
                {
                    plates += p + System.Environment.NewLine;
                }

                s.CreateAndAddLabelBoxRow(view2, lID, "Lista Negra de Placas");

                var l = new List<string>();

                s.CreateAndAddFullMultilineTextBoxRow(view2, lID, plates, (o2, e2, mee) =>
                {
                    foreach (var line in mee.Text.Split(System.Environment.NewLine))
                    {
                        if (line != "") l.Add(line);
                    }
                });

                alert2.SetCancelable(false);
                alert2.SetPositiveButton("OK", (sender, e2) =>
                {
                    if (l.Count > 0) SavedDataStore.PlateNumbers = l;
                });
                alert2.Create().Show();

            });

            s.CreateAndAddBoldDescriptionRow2(view, lID, "Clique no botão acima para inserir as placas que serão reconhecidas como " +
                "roubadas ou clonadas quando o Detector de Placas de Veículos estiver selecionado.");

            s.CreateAndAddLabelBoxRow(view, lID, "SOBRE");

            s.CreateAndAddButtonRow(view, lID, "Sobre o MotoDetector", (o, e, b) =>
            {

                var alert2 = new AlertDialog.Builder(this);
                var view2 = new AboutView(this);
                alert2.SetView(view2);
                alert2.SetCancelable(false);
                alert2.SetPositiveButton("OK", (sender, e2) =>
                {
                });
                alert2.Create().Show();

            });

            alert.SetCancelable(true);
            alert.SetNegativeButton("CANCELAR", (sender, e2) => { });
            alert.SetPositiveButton("OK", (sender, e2) =>
            {
                SavedDataStore.Save();
                UpdateCameraSettings();
                RunOnUiThread(() =>
                {
                    ConfigureCapture();
                });
            });
            alert.Create().Show();

        }

        private void ConfigureCapture()
        {
            var controller = cameraSurface.cameraAnalyzer.cameraController;
            controller.CloseCamera();
            cameraSurface.Restart();
            controller = cameraSurface.cameraAnalyzer.cameraController;
            controller.StartBackgroundThread();
            if (controller.mTextureView.IsAvailable)
            {
                controller.mTextureView.SurfaceTextureListener = controller.mSurfaceTextureListener;
                controller.OpenCamera(controller.mTextureView.Width, controller.mTextureView.Height);
            }
            else
            {
                controller.mTextureView.SurfaceTextureListener = controller.mSurfaceTextureListener;
            }
        }
    }
}