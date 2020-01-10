namespace MotoDetector.Helpers
{
    public class Stats
    {
        public int type = 0;

        public float CameraFps { get; set; }
        public float CameraMs { get; set; }

        public float ProcessingFps { get; set; }
        public float ProcessingMs { get; set; }

        public long YUV2RGBElapsedMs { get; set; }

        public long ResizeAndRotateElapsedMs { get; set; }

        public long InterpreterElapsedMs { get; set; }
        public int NumDetections { get; set; }

        public float[] Labels { get; set; }
        public float[] Scores { get; set; }
        public float[] BoundingBoxes { get; set; }

        public float[] Scores2 { get; set; }
    }

}