using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Image_Restoring_v2
{
    /// <summary>
    /// Интерфейс, определение метода ApplyInterlace
    /// </summary>
    public interface IImageProcessor
    {
        void ApplyInterlace(Bitmap image);
    }
}
