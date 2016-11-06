using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Flh.IO
{
    public interface IImageProcessorArguments
    {
        string ToQuery();
    }

    public interface IImageProcessor
    {
        IImageProcessorArguments Arguments { get; }
        System.IO.Stream Process(IImageProcessorSetting setting, System.IO.Stream imageStream);
    }
}
