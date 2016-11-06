using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Flh.IO
{
	public class GetImageType
	{
		public static ImageFormat GetTypeFromSting(string strFormat)
		{
			ImageFormat imageFormat = null;
			switch (strFormat)
			{
				case "image/bmp":
					imageFormat = ImageFormat.Bmp;
					break;
				case "image/gif":
					imageFormat = ImageFormat.Gif;
					break;
				case "image/x-icon":
					imageFormat = ImageFormat.Icon;
					break;
				case "image/png":
					imageFormat = ImageFormat.Png;
					break;
				default:
					imageFormat = ImageFormat.Jpeg;
					break;
			}
			return imageFormat;
		}
	}
}
