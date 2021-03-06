﻿using SkiaSharp;

namespace MotoDetector.Helpers
{
    public static class DrawingHelper
    {
        private static readonly SKPaint boundingBoxPaint = new SKPaint
        {
            StrokeWidth = 5,
            Color = SKColors.White,
            Style = SKPaintStyle.Stroke,
            IsAntialias = false,
        };

        private static readonly SKPaint textPaint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            TextSize = 40,
        };

        private static readonly SKPaint textPaint2 = new SKPaint
        {
            Color = SKColors.White,
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            TextSize = 40,
        };

        private static readonly SKPaint backgroundPaint = new SKPaint
        {
            Color = new SKColor(0xFF, 0xFF, 0xFF, 0x50),
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
        };

        public static void DrawBoundingBox(SKCanvas canvas, float left, float top, float right, float bottom)
        {
            canvas.DrawRoundRect(new SKRect(left, top, right, bottom), new SKSize(23, 23), boundingBoxPaint);
        }

        public static void DrawText(SKCanvas canvas, float x, float y, string text)
        {
            canvas.DrawText(text, x, y, textPaint);
        }

        public static void DrawText2(SKCanvas canvas, float x, float y, string text)
        {
            canvas.DrawText(text, x, y, textPaint2);
        }

        public static void DrawBackgroundRectangle(SKCanvas canvas, float width, float height, float x, float y)
        {
            canvas.DrawRect(x, y, width, height, backgroundPaint);
        }
    }
}