using MotoDetector.Helpers;
using Emgu.TF.Lite;
using System;
using System.IO;
using Android.Gms.Vision.Texts;
using Android.Gms.Vision;
using Android.Util;
using System.Text;
using SkiaSharp.Views.Android;
using SkiaSharp;
using System.Linq;
using Android.Widget;

namespace MotoDetector
{
    public unsafe class TensorflowLiteService
    {

        Android.Media.MediaPlayer player;

        public const int ModelInputSize_PlateAndMoto = 300;
        public const int ModelInputSize_MotoModel = 224;

        public int ModelType = 0;

        private FlatBufferModel model;
        private Interpreter interpreter;

        private Android.Graphics.Bitmap previewbmp;

        private Tensor inputTensor;
        private Tensor[] outputTensors;

        public TextRecognizer txtRecognizer;

        private IntPtr imgptr;

        public bool Initialize(Stream modelData, bool useNumThreads)
        {
            using (var builder = new TextRecognizer.Builder(MainActivity.context))
            {
                txtRecognizer = builder.Build();
            }

            using (var ms = new MemoryStream())
            {
                modelData.CopyTo(ms);

                model = new FlatBufferModel(ms.ToArray());
            }

            if (!model.CheckModelIdentifier())
            {
                return false;
            }

            var op = new BuildinOpResolver();
            interpreter = new Interpreter(model, op);

            if (useNumThreads)
            {
                interpreter.SetNumThreads(Environment.ProcessorCount);
            }

            var allocateTensorStatus = interpreter.AllocateTensors();
            if (allocateTensorStatus == Status.Error)
            {
                return false;
            }

            var input = interpreter.GetInput();
            inputTensor = interpreter.GetTensor(input[0]);

            var output = interpreter.GetOutput();
            var outputIndex = output[0];

            outputTensors = new Tensor[output.Length];
            for (var i = 0; i < output.Length; i++)
            {
                outputTensors[i] = interpreter.GetTensor(outputIndex + i);
            }

            return true;
        }

        public void Recognize(IntPtr colors, int colorsCount)
        {
            CopyColorsToTensor(colors, colorsCount, inputTensor.DataPointer);

            interpreter.Invoke();

            if (ModelType == 0)
            {

                //object detector (plate)

                var detectionBoxes = (float[])outputTensors[0].GetData();
                var detectionClasses = (float[])outputTensors[1].GetData();
                var detectionScores = (float[])outputTensors[2].GetData();
                var detectionNumDetections = (float[])outputTensors[3].GetData();

                var numDetections = (int)detectionNumDetections[0];

                MainActivity.PlateAndMotoStats.NumDetections = numDetections;
                MainActivity.PlateAndMotoStats.Labels = detectionClasses;
                MainActivity.PlateAndMotoStats.Scores = detectionScores;
                MainActivity.PlateAndMotoStats.BoundingBoxes = detectionBoxes;

                CropAndParse();

            }
            else if (ModelType == 1)
            {

                //object detector (moto)

                var detectionBoxes = (float[])outputTensors[0].GetData();
                var detectionClasses = (float[])outputTensors[1].GetData();
                var detectionScores = (float[])outputTensors[2].GetData();
                var detectionNumDetections = (float[])outputTensors[3].GetData();

                var numDetections = (int)detectionNumDetections[0];

                MainActivity.PlateAndMotoStats.NumDetections = numDetections;
                MainActivity.PlateAndMotoStats.Labels = detectionClasses;
                MainActivity.PlateAndMotoStats.Scores = detectionScores;
                MainActivity.PlateAndMotoStats.BoundingBoxes = detectionBoxes;

            }
            else
            {

                // image labeling

                var detectionScores = (float[])outputTensors[0].GetData();

                MainActivity.MotoModelStats.Scores2 = detectionScores;

                var maxscore = detectionScores.Max();

                var i0 = detectionScores.ToList().IndexOf(maxscore);

                MainActivity.context.RunOnUiThread(() =>
                {
                    var txtView = MainActivity.context.FindViewById<TextView>(Resource.Id.txtObjID);
                    if (i0 < MainActivity.MotoLabels.Count)
                    {
                        var label = MainActivity.MotoLabels[i0];
                        if (MainActivity.MotosList.ContainsKey(label))
                        {
                            MainActivity.DetectedMotoModel = label;
                            MainActivity.DetectedPlate = "";
                            var fab = MainActivity.MotosList[label].Fabricante;
                            var modelo = MainActivity.MotosList[label].Modelo;
                            txtView.Text = fab + " " + modelo + " (" + (maxscore * 100).ToString("N2") + "%)";
                            if (maxscore > 0.8)
                            {
                                txtView.SetTextColor(Android.Graphics.Color.Green);
                            }
                            else if (maxscore > 0.6)
                            {
                                txtView.SetTextColor(Android.Graphics.Color.Yellow);
                            }
                            else
                            {
                                txtView.SetTextColor(Android.Graphics.Color.Red);
                            }
                        }
                        else
                        {
                            MainActivity.DetectedMotoModel = "";
                            MainActivity.DetectedPlate = "";
                            txtView.Text = "";
                        }
                    }
                    else
                    {
                        MainActivity.DetectedMotoModel = "";
                        MainActivity.DetectedPlate = "";
                        txtView.Text = "";
                    }
                });

            }

        }

        private void CropAndParse()
        {

            MainActivity.DetectedMotoModel = "";
            MainActivity.DetectedPlate = "";

            if (!txtRecognizer.IsOperational)
            {
                // Log.Error("Error", "Detector dependencies are not yet available");
            }
            else
            {

                var maxscore = MainActivity.PlateAndMotoStats.Scores.Max();

                if (maxscore > MainActivity.SCORE_THRESHOLD)
                {
                    var i0 = MainActivity.PlateAndMotoStats.Scores.ToList().IndexOf(maxscore);
                    var bboxes = MainActivity.PlateAndMotoStats.BoundingBoxes.ToList();

                    var ymin = bboxes[i0 * 4 + 0] * 300;
                    var xmin = bboxes[i0 * 4 + 1] * 300;
                    var ymax = bboxes[i0 * 4 + 2] * 300;
                    var xmax = bboxes[i0 * 4 + 3] * 300;

                    if (xmin < 0) xmin = 0;
                    if (ymin < 0) ymin = 0;
                    if (xmax > 300) xmax = 300;
                    if (ymax > 300) ymax = 300;

                    var w = xmax - xmin;
                    var h = ymax - ymin;

                    using (var builder = new Frame.Builder())
                    {
                        var cfg = Android.Graphics.Bitmap.Config.Argb8888;
                        using (var bmp = Android.Graphics.Bitmap.CreateBitmap(CopyColorsToArray(imgptr), 300, 300, cfg))
                        {
                            using (var bmpc = Android.Graphics.Bitmap.CreateBitmap(bmp, (int)xmin, (int)ymin, (int)(w), (int)(h)))
                            {
                                if (previewbmp != null)
                                {
                                    previewbmp.Dispose();
                                    previewbmp = null;
                                }
                                previewbmp = bmpc.ToSKBitmap().ToBitmap();
                                var frame = builder.SetBitmap(bmpc).Build();
                                var items = txtRecognizer.Detect(frame);
                                var strBuilder = new StringBuilder();
                                var plate = "";
                                for (int i = 0; i < items.Size(); i++)
                                {
                                    var item = (TextBlock)items.ValueAt(i);
                                    var str = item.Value.Replace('\n', ' ');
                                    plate = str;
                                    strBuilder.Append("i = " + i + ": " + str + "|");
                                    if (i > 0) break;
                                }
                                plate = ParsePlate(plate);
                                Console.WriteLine(plate);
                                if (CheckIsValidPlate(plate))
                                {
                                    MainActivity.DetectedPlate = plate;
                                    MainActivity.context.RunOnUiThread(() =>
                                    {
                                        var imgView = MainActivity.context.FindViewById<ImageView>(Resource.Id.imageViewPlatePreview);
                                        imgView.SetImageBitmap(previewbmp);
                                        var txtView = MainActivity.context.FindViewById<TextView>(Resource.Id.txtObjID);
                                        txtView.Text = "Placa Detectada: " + plate + " (" + (maxscore * 100).ToString("N2") + "%)";
                                    });
                                    if (MainActivity.SavedDataStore.PlateNumbers.Contains(plate))
                                    {
                                        Xamarin.Essentials.Vibration.Vibrate(2000);
                                        StartMedia("warning.mp3");
                                        MainActivity.context.RunOnUiThread(() =>
                                        {
                                            var layout = MainActivity.context.FindViewById<LinearLayout>(Resource.Id.linearLayoutLabelPanel);
                                            layout.SetBackgroundColor(Android.Graphics.Color.ParseColor("#89ff0000"));
                                            var txtView = MainActivity.context.FindViewById<TextView>(Resource.Id.txtObjID);
                                            txtView.SetTextColor(Android.Graphics.Color.Black);
                                            var imgwarning = MainActivity.context.FindViewById<ImageButton>(Resource.Id.imageButtonWarning);
                                            imgwarning.Visibility = Android.Views.ViewStates.Visible;
                                        });
                                    }
                                    else
                                    {
                                        MainActivity.context.RunOnUiThread(() =>
                                        {
                                            var layout = MainActivity.context.FindViewById<LinearLayout>(Resource.Id.linearLayoutLabelPanel);
                                            layout.SetBackgroundColor(Android.Graphics.Color.ParseColor("#bc000000"));
                                            var txtView = MainActivity.context.FindViewById<TextView>(Resource.Id.txtObjID);
                                            if (maxscore < 0.7)
                                            {
                                                txtView.SetTextColor(Android.Graphics.Color.Red);
                                            }
                                            else if (maxscore < 0.85)
                                            {
                                                txtView.SetTextColor(Android.Graphics.Color.Yellow);
                                            }
                                            else
                                            {
                                                txtView.SetTextColor(Android.Graphics.Color.LightGreen);
                                            }
                                        });
                                        if (MainActivity.SavedDataStore.Vibrate)
                                        {
                                            Xamarin.Essentials.Vibration.Vibrate(200.0);
                                        }
                                        if (MainActivity.SavedDataStore.PlaySound)
                                        {
                                            StartMedia("pop.m4a");
                                        }
                                    }
                                }

                            }
                        }
                    }
                }
            }

        }

        public void StartMedia(string url_string)
        {
            player = new Android.Media.MediaPlayer();
            player.SetDataSource(url_string);
            //player.Prepare ();
            player.Start();
        }

        private static bool CheckIsValidPlate(string fullplate)
        {
            //Console.WriteLine(fullplate);
            if (fullplate.Length != 7) return false;
            var firstpart = fullplate.Substring(0, 3);
            foreach (Char c in firstpart)
            {
                if (!Char.IsLetter(c)) return false;
            }
            var secondpart = fullplate.Substring(3);
            if (!Char.IsNumber(secondpart[0])) return false;
            if (!Char.IsLetterOrDigit(secondpart[1])) return false;
            if (!Char.IsNumber(secondpart[2])) return false;
            if (!Char.IsNumber(secondpart[3])) return false;
            return true;
        }

        private static string ParsePlate(string part1)
        {
            var cplate = part1.Replace(" ", "").Replace("-", "").Replace("_", "").Replace(".", "").Replace("•", "").Replace(":", "").Replace(";", "");
            cplate = cplate.Replace("|", "I");
            if (cplate.Length == 7)
            {
                var cpart1 = cplate.Substring(0, 3);
                var cpart2 = cplate.Substring(3, 1);
                var cpart2a = cplate.Substring(4, 1);
                var cpart3 = cplate.Substring(5);
                cpart1 = cpart1.Replace("0", "O");
                cpart1 = cpart1.Replace("1", "I");
                cpart2 = cpart2.Replace("O", "0");
                cpart2 = cpart2.Replace("I", "1");
                cpart3 = cpart3.Replace("O", "0");
                cpart3 = cpart3.Replace("I", "1");
                return cpart1.ToUpper() + cpart2.ToUpper() + cpart2a.ToUpper() + cpart3.ToUpper();
            }
            else
            {
                return cplate.ToUpper();
            }
        }

        private void CopyColorsToTensor(IntPtr colors, int colorsCount, IntPtr dest)
        {
            imgptr = colors;
            var colorsPtr = (int*)colors;
            var destPtr = (float*)dest;

            for (var i = 0; i < colorsCount; ++i)
            {
                var val = colorsPtr[i];

                //// AA RR GG BB
                var fr = ((float)((float)((val >> 16) & 0xFF) / 255.0f));
                var fg = ((float)((float)((val >> 8) & 0xFF) / 255.0f));
                var fb = ((float)((float)(val & 0xFF) / 255.0f));
                //var fr = (float)((val >> 16) & 0xFF);
                //var fg = (float)((val >> 8) & 0xFF);
                //var fb = (float)(val & 0xFF);

                *(destPtr + (i * 3) + 0) = fr;
                *(destPtr + (i * 3) + 1) = fg;
                *(destPtr + (i * 3) + 2) = fb;
            }
        }

        private static int[] CopyColorsToArray(IntPtr colors)
        {

            var colorsPtr = (int*)colors;

            var values = new int[300 * 300];

            for (var i = 0; i < 300 * 300; ++i)
            {
                values[i] = colorsPtr[i];
            }

            return values;
        }

    }
}
