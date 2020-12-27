The DriveInfo ReadMe file


Introduction
============
DriveInfo will display basic information about the disk drives that are connected to a computer.


Using DriveInfo
===============
You may experience a delay between when you launch the application and when the drives are displayed.
This delay is usually due to one or more of the drives being in sleep or power saving mode. Once the
drive wakes up the window will appear. There also seems to be a delay if a network drive recently
became unavailable.

You can choose to have the disk size and free columns display in either GB (1000^3) or in GiB (1024^3).
The column headers will changes accordingly.

All columns in the grid are sortable by clicking on the column header. Multiple columns may be sorted
by holding Shift while clicking on the column header.

Pressing F5 will refresh the display. Press F7 to toggle the shading of alternate rows in the data grid.
Ctrl+C will copy the details to the Windows clipboard in tab delimited format.

In addition to the above, the menu has selections that let you include or exclude drives that are not
ready, allow the DriveInfo window to stay on top of other windows, and to make the display larger or
smaller. You can also use Ctrl + Mouse Wheel to zoom the display.


Uninstalling DriveInfo
======================
To uninstall, use the regular Windows add/remove programs feature.



Notices and License
===================
DriveInfo was written in C# by Tim Kennedy.

DriveInfo uses the following icons & packages:

Fugue Icons set https://p.yusukekamiyamane.com/

Json.net v12.0.3 from Newtonsoft https://www.newtonsoft.com/json

NLog v4.7.6 https://nlog-project.org/

Inno Setup was used to create the installer. https://jrsoftware.org/isinfo.php


MIT License
Copyright (c) 2020 Tim Kennedy

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
associated documentation files (the "Software"), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject
to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial
portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT
LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.