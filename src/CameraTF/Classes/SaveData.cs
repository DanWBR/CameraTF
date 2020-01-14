using System;
using System.Collections.Generic;
using Microsoft.AppCenter.Crashes;

namespace MotoDetector.Classes
{
    public class SaveData
    {

        public string Camera = "";

        public enum CaptureResolution
        {
            Res480p,
            Res720p,
            Res1080p,
            Res2160p
        };

        public enum CaptureFPS
        {
            FPS15,
            FPS30,
            FPS60,
            FPS120
        };

        public enum DetectorFPS
        {
            FPS1,
            FPS5,
            FPS10,
            FPS15,
            FPS30,
            FPS60
        };

        public enum DetectorType
        {
            MotorcycleModels,
            LicensePlates,
            Objects
        }

        public bool DisplayTutorial = true;

        public DetectorType SelectedDetector = DetectorType.LicensePlates;

        public CaptureResolution SelectedResolution = CaptureResolution.Res1080p;

        public CaptureFPS SelectedFPS = CaptureFPS.FPS30;

        public DetectorFPS DetectionFPS = DetectorFPS.FPS5;

        public float CameraFocusValue = -1.0f;

        public float DigitalZoomLevel = 1.0f;

        public float ExposureBias;

        public bool PlaySound = true;

        public bool Vibrate = true;

        public List<string> PlateNumbers = new List<string>();

        public SaveData()
        {

        }

        public void Save()
        {
            try
            {
                var jsondata = Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.None);
                Xamarin.Essentials.Preferences.Set("savedata", jsondata);
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex, new Dictionary<string, string> { { "Issue", "SaveData error" } });
            }

        }

        public static SaveData Load()
        {

            var jsondata = Xamarin.Essentials.Preferences.Get("savedata", "");

            try
            {
                var savedata = Newtonsoft.Json.JsonConvert.DeserializeObject<SaveData>(jsondata);

                if (savedata.PlateNumbers == null) savedata.PlateNumbers = new List<string>();

                return savedata;
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex, new Dictionary<string, string> { { "Issue", "LoadData error" } });
                return null;
            }
        }

    }
}
