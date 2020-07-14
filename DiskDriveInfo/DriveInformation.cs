// Copyright (c) Tim Kennedy. All Rights Reserved. Licensed under the MIT License.

namespace DiskDriveInfo
{
    internal class DriveInformation
    {
        public string Name { get; set; }
        public string DriveType { get; set; }
        public string Format { get; set; }
        public string Label { get; set; }
        public double TotalSize { get; set; }
        public double GBFree { get; set; }
        public double PercentFree { get; set; }

        public DriveInformation(string name,
                                string driveType,
                                string driveformat,
                                string volumelabel,
                                double totalsize,
                                double gbFree,
                                double percentFree)
        {
            Name = name;
            DriveType = driveType;
            Format = driveformat;
            Label = volumelabel;
            TotalSize = totalsize;
            GBFree = gbFree;
            PercentFree = percentFree;
        }
    }
}
