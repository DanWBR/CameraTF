using MotoDetector.Helpers;
using Emgu.TF.Lite;
using System;
using System.IO;

namespace MotoDetector
{
    public unsafe class TensorflowLiteService
    {
        public const int ModelInputSize_PlateAndMoto = 300;
        public const int ModelInputSize_MotoModel = 224;

        public int ModelType = 0;

        private FlatBufferModel model;
        private Interpreter interpreter;

        private Tensor inputTensor;
        private Tensor[] outputTensors;

        public bool Initialize(Stream modelData, bool useNumThreads)
        {
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
                //object detector

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

            }

        }

        private static void CopyColorsToTensor(IntPtr colors, int colorsCount, IntPtr dest)
        {
            var colorsPtr = (int*)colors;
            var destPtr = (float*)dest;

            for (var i = 0; i < colorsCount; ++i)
            {
                var val = colorsPtr[i];

                //// AA RR GG BB
                var fr = ((float)((float)((val >> 16) & 0xFF) / 255.0f));
                var fg = ((float)((float)((val >> 8) & 0xFF) / 255.0f));
                var fb = ((float)((float)(val & 0xFF) / 255.0f));

                *(destPtr + (i * 3) + 0) = fr;
                *(destPtr + (i * 3) + 1) = fg;
                *(destPtr + (i * 3) + 2) = fb;
            }
        }
    }
}
