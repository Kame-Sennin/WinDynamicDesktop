﻿// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace WinDynamicDesktop
{
    class ThemeThumbLoader
    {
        public static Size GetThumbnailSize(System.Windows.Forms.Control control)
        {
            int scaledWidth;

            using (Graphics g = control.CreateGraphics())
            {
                scaledWidth = (int)(192 * g.DpiX / 96);
            }

            return new Size(scaledWidth, scaledWidth * 9 / 16);
        }

        public static Image ScaleImage(Image tempImage, Size size)
        {
            if (tempImage.Size == size)
            {
                return tempImage;
            }

            // Image scaling code from https://stackoverflow.com/a/7677163/5504760
            using (tempImage)
            {
                Bitmap bmp = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);

                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.DrawImage(tempImage, new Rectangle(0, 0, bmp.Width, bmp.Height));
                }

                return bmp;
            }
        }

        public static Image ScaleImage(string filename, Size size)
        {
            return ScaleImage(Image.FromFile(filename), size);
        }

        public static Image GetThumbnailImage(ThemeConfig theme, Size size, bool useCache)
        {
            string themePath = Path.Combine("themes", theme.themeId);
            string thumbnailPath = Path.Combine(themePath, "thumbnail.png");

            if (useCache)
            {
                if (File.Exists(thumbnailPath))
                {
                    Image cachedImage = Image.FromFile(thumbnailPath);

                    if (cachedImage.Size == size)
                    {
                        return cachedImage;
                    }
                    else
                    {
                        cachedImage.Dispose();
                        File.Delete(thumbnailPath);
                    }
                }
                else if (ThemeManager.defaultThemes.Contains(theme.themeId))
                {
                    Image cachedImage = (Image)Properties.Resources.ResourceManager.GetObject(
                        theme.themeId + "_thumbnail");
                    return ScaleImage(cachedImage, size);
                }
            }

            int imageId1 = theme.dayHighlight ?? theme.dayImageList[theme.dayImageList.Length / 2];
            int imageId2 = theme.nightHighlight ?? theme.nightImageList[theme.nightImageList.Length / 2];
            string imageFilename1 = theme.imageFilename.Replace("*", imageId1.ToString());
            string imageFilename2 = theme.imageFilename.Replace("*", imageId2.ToString());

            using (var bmp1 = ScaleImage(Path.Combine(themePath, imageFilename1), size))
            {
                Bitmap bmp2 = (Bitmap)ScaleImage(Path.Combine(themePath, imageFilename2), size);

                using (Graphics g = Graphics.FromImage(bmp2))
                {
                    g.DrawImage(bmp1, 0, 0, new Rectangle(0, 0, bmp1.Width / 2, bmp1.Height), GraphicsUnit.Pixel);
                }

                bmp2.Save(thumbnailPath, ImageFormat.Png);

                return bmp2;
            }
        }
    }
}
