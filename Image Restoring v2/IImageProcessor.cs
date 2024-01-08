using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Image_Restoring_v2
{
    public interface IImageProcessor
    {
        void ApplyInterlace(Bitmap image);
    }
}
