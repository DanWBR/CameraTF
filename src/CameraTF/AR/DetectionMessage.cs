﻿namespace CameraTF.AR
{
    public class DetectionMessage
    {
        public long InterpreterElapsedMs { get; set; }
        public int NumDetections { get; set; }

        public float[] Labels { get; set; }
        public float[] Scores { get; set; }
        public float[] BoundingBoxes { get; set; }
    }
}