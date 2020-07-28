﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ClassifyFiles.UI.Util
{
    /// <summary>
    /// 平滑滚动的帮助类
    /// </summary>
    /// <remarks>
    /// 本来自然是想到用DoubleAnimation的，但是发现Offset并不能动画。那就干脆手动一下，来个循环。
    /// 为了能够让速度叠加，加了一个remainsDelta。没想到，这一加，直接把阻尼感都写出来了
    /// 可把我牛逼坏了
    /// </remarks>
    public static class SmoothScrollViewerHelper
    {
        private static Dictionary<ScrollViewer, double> remainsDeltas = new Dictionary<ScrollViewer, double>();
        public static void Regist(ScrollViewer scr)
        {
            scr.PreviewMouseWheel += async (p1, p2) =>
             {
                 await HandleMouseWheel(p1 as ScrollViewer, p2.Delta);
             };
        }
        public static async Task HandleMouseWheel(ScrollViewer scr, int delta)
        {
            Debug.Assert(scr != null);
            if (!remainsDeltas.ContainsKey(scr))
            {
                remainsDeltas.Add(scr, 0);
            }

            remainsDeltas[scr] = remainsDeltas[scr] + delta;
            if (remainsDeltas[scr] != delta)
            {

                return;
            }
            while (remainsDeltas[scr] != 0)
            {

                scr.ScrollToVerticalOffset(scr.VerticalOffset - remainsDeltas[scr] / 30d * System.Windows.Forms.SystemInformation.MouseWheelScrollLines);
                remainsDeltas[scr] /= 1.2;
                await Task.Delay(1);
                if (Math.Abs(remainsDeltas[scr]) < 10)
                {
                    remainsDeltas[scr] = 0;
                }
            }
        }
    }
}