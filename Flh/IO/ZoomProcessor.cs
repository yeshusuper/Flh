using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

namespace Flh.IO
{
    public class ZoomProcessor : IImageProcessor
    {
        private readonly ZoomProcessArguments _Args;

        public ZoomProcessor(ZoomProcessArguments args)
        {
            ExceptionHelper.ThrowIfNull(args, "args");
            _Args = args;
        }
        public IImageProcessorArguments Arguments
        {
            get { return _Args; }
        }

        public System.IO.Stream Process(IImageProcessorSetting setting, System.IO.Stream imageStream)
        {
            ExceptionHelper.ThrowIfNull(imageStream, "imageStream");

            if (!_Args.IsNeetProcess())
                return imageStream.Copy();

            var image = Image.FromStream(imageStream);

            var height = _Args.Height;
            var width = _Args.Width;
            if (_Args.Multiple.HasValue)
            {
                if (height.HasValue) height = height.Value * _Args.Multiple.Value;
                if (width.HasValue) width = width.Value * _Args.Multiple.Value;
            }

            if (height.HasValue && width.HasValue)
            {
                var heightFirst = _Args.Edge == ZoomProcessArguments.EdgeEnum.Long
                                    ? image.Height >= image.Width
                                    : image.Height <= image.Width;
                if (heightFirst)
                {
                    var e = (double)image.Height / (double)height.Value;
                    width = (int)((double)image.Width / e);
                }
                else
                {
                    var e = (double)image.Width / (double)width.Value;
                    height = (int)((double)image.Height / e);
                }
            }
            else
            {
                var e = (double)image.Height / (double)image.Width;
                if (height.HasValue)
                {
                    width = (int)((double)height.Value / e);
                }
                else if (width.HasValue)
                {
                    height = (int)(width.Value * e);
                }
            }

            var format = String.IsNullOrWhiteSpace(_Args.Format) ? ".jpg" : _Args.Format;
            var mineType = Flh.IO.MimeTypeHelper.GetMimeType(format);
            ExceptionHelper.ThrowIfTrue(!mineType.StartsWith("image/"), "_Args", "输出图片格式设置错误");

            if(height.HasValue && width.HasValue && _Args.Large.HasValue && _Args.Large.Value)
            {
                if(height.Value > image.Height || width.Value > image.Width)
                {
                    height = null;
                    width = null;
                }
            }

            if (height.HasValue && width.HasValue)
            {
                using (var zoomImage = new Bitmap(width.Value, height.Value))
                {
                    using (var graphics = Graphics.FromImage(zoomImage))
                    {
                        graphics.DrawImage(image, 0, 0, width.Value, height.Value);
                        return GetQualityZoomStream(zoomImage, mineType, _Args.Quality, _Args.AbsoluteQuality);
                    }
                }
            }
            else if (_Args.AbsoluteQuality.HasValue || _Args.Quality.HasValue)
            {
                return GetQualityZoomStream(image, mineType, _Args.Quality, _Args.AbsoluteQuality);
            }
            else
            {
                var result = new System.IO.MemoryStream();
                image.Save(result, GetImageFormatByMineType(mineType));
                result.Position = 0;
                return result;
            }
        }


        private static bool CanSetQuality(ImageCodecInfo encoder)
        {
            return encoder != null &&
                (encoder.MimeType == "image/png"
                || encoder.MimeType == "image/jpeg");
        }

        private static EncoderParameters GetQualityEncoderParameters(Image image, ImageCodecInfo encoder, int? quality, int? absoluteQuality)
        {
            if ((absoluteQuality.HasValue || quality.HasValue) && CanSetQuality(encoder))
            {
                int? _quality = null;
                var oldQuality = 0;
                try
                {
                    oldQuality = image.GetEncoderParameterList(System.Drawing.Imaging.Encoder.Quality.Guid)
                                        .Param.Select(p => p.NumberOfValues).FirstOrDefault();
                }
                catch { }

                if (absoluteQuality.HasValue)
                {
                    if (oldQuality < absoluteQuality.Value)
                    {
                        _quality = absoluteQuality;
                    }
                }
                else if (quality.HasValue)
                {
                    if (oldQuality != quality.Value)
                    {
                        _quality = quality;
                    }
                }

                if (_quality.HasValue)
                {
                    var parameters = new EncoderParameters(1);
                    parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, _quality.Value);
                    return parameters;
                }
            }
            return null;
        }

        private static ImageFormat GetImageFormatByMineType(string mineType)
        {
            ImageFormat imageFormat = null;
            switch (mineType)
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

        private static System.IO.Stream GetQualityZoomStream(Image image, string mineType, int? quality, int? absoluteQuality)
        {
            var encoder = ImageCodecInfo.GetImageEncoders()
                                    .Where(enc => enc.MimeType == mineType)
                                    .FirstOrDefault();

            ExceptionHelper.ThrowIfNull(encoder, "mineType", "mime类型：" + mineType + "找不到对应的编码器");


            var qualityParameters = GetQualityEncoderParameters(image, encoder, quality, absoluteQuality);

            var stream = new System.IO.MemoryStream();
            if (qualityParameters == null)
            {
                var imageFormat = GetImageFormatByMineType(mineType);
                image.Save(stream, imageFormat);
            }
            else
            {
                image.Save(stream, encoder, qualityParameters);
            }
            stream.Position = 0;
            return stream;
        }
    }
}
