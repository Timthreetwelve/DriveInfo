// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

using System.Collections.Generic;

namespace DiskDriveInfo
{
    internal class DriveInformation
    {
        public string Name { get; set; }
        public string DriveType { get; set; }
        public string Format { get; set; }
        public string Label { get; set; }
        public double? TotalSize { get; set; }
        public double? GBFree { get; set; }
        public double? PercentFree { get; set; }

        public static List<DriveInformation> DriveInfoList = new List<DriveInformation>();
    }
}
