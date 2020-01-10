using System;
using System.Collections.Generic;
using Microsoft.AppCenter.Crashes;

namespace MotoDetector.Classes
{
    public class SaveData
    {

        public enum CameraType
        {
            UltraWide,
            Wide,
            Tele
        };

        public enum CaptureResolution
        {
            Res540p,
            Res720p,
            Res1080p,
            Res4K
        };

        public enum CaptureFPS
        {
            FPS15,
            FPS30,
            FPS60,
            FPS120,
            FPS240
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
            Objects,
            LicensePlates
        }

        public bool DisplayTutorial = true;

        public DetectorType SelectedDetector = DetectorType.MotorcycleModels;

        public CameraType SelectedCamera = CameraType.Wide;

        public CaptureResolution SelectedResolution = CaptureResolution.Res540p;

        public CaptureFPS SelectedFPS = CaptureFPS.FPS15;

        public DetectorFPS DetectionFPS = DetectorFPS.FPS1;

        public float CameraFocusValue = -1.0f;

        public float DigitalZoomLevel = 1.0f;

        public float ExposureBias;

        public bool PlaySound = true;

        public bool Vibrate = true;

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

                if (savedata.SelectedDetector == DetectorType.LicensePlates)
                {
                    savedata.SelectedDetector = DetectorType.MotorcycleModels;
                }

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
