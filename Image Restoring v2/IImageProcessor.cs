using System.Drawing;

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
