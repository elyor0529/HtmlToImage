﻿using System;
using System.Collections.Generic;
using System.Text;

namespace itext.pdfimage.Extensions
{
    public static class MeasuringExtensions
    {

        public static int Dpi { get; set; } = 300;

        public static float PixelsToPoints(this float value, int? dpi = null)
        {
            return value / (dpi ?? Dpi) * 72;
        }

        public static int PointsToPixels(this int value, int? dpi = null)
        {
            return PointsToPixels((float)value);
        }

        public static int PointsToPixels(this float value, int? dpi = null)
        {
            return (int)(value * (dpi ?? Dpi) / 72);
        }

    }
}
