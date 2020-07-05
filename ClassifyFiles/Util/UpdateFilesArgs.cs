﻿using ClassifyFiles.Data;
using System;

namespace ClassifyFiles.Util
{
    public class UpdateFilesArgs
    {
        public bool Research { get; set; }
        public Project Project { get; set; }
        public bool IncludeThumbnails { get; set; }
        /// <summary>
        /// 第一个参数是百分比（0~1），第二个参数是当前的文件，返回值为是否继续
        /// </summary>
        public Func<double, File, bool> Callback { get; set; }
        public bool Reclassify { get; set; }
    }
}