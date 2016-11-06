using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace Flh.IO
{
    public interface IImageProcessorSetting
    {
        Func<string, System.IO.Stream> WatermakStreamProvider { get; set; }
    }

    public class ImageProcessorSetting : IImageProcessorSetting
    {
        public Func<string, System.IO.Stream> WatermakStreamProvider { get; set; }
    }


    public static class ImageProcessor
    {
        public static System.IO.Stream Zoom(this System.IO.Stream imageStream, ZoomProcessArguments args)
        {
            var processor = new ZoomProcessor(args);
            return processor.Process(null, imageStream);
        }

        public static System.IO.Stream Process(this System.IO.Stream imageStream, params IImageProcessor[] processors)
        {
            return Process(imageStream, null, processors);
        }

        public static System.IO.Stream Process(this System.IO.Stream imageStream, IImageProcessorSetting setting, params IImageProcessor[] processors)
        {
            ExceptionHelper.ThrowIfNullOrEmpty(processors, "processors");
            var stream = imageStream;
            if (processors.Length > 1)
            {
                for (int i = 0; i < processors.Length - 1; i++)
                {
                    var temp = stream;
                    if (i > 0)
                        using (temp)
                        {
                            stream = processors[i].Process(setting, temp);
                        }
                    else
                        stream = processors[i].Process(setting, temp);
                }
            }
            return processors[processors.Length - 1].Process(setting, stream);
        }

        public static IImageProcessor[] ParseQuery(string query, out string extension)
        {
            extension = String.Empty;
            if (String.IsNullOrWhiteSpace(query))
                return new IImageProcessor[0];
            else
            {
                query = query.Trim();
                if (query[0] == '@')
                    query = query.Substring(1);

                var querys = query.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (querys.Length == 0)
                    return new IImageProcessor[0];
                else
                {
                    var result = new List<IImageProcessor>();
                    foreach (var item in querys)
                    {
                            var args = ZoomProcessArguments.Parse(item);
                            if (args.IsNeetProcess())
                            {
                                extension = args.Format;
                                result.Add(new ZoomProcessor(args));
                            }
                    }
                    return result.ToArray();
                }
            }
        }
    }
}
