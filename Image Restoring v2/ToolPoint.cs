using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Image_Restoring_v2
{
    // ToolPoint - вспомогательный класс для работы с координатами точек
    public class ToolPoint
    {
        // координаты точек
        public double x;
        public double y;

        //конструктор
        public ToolPoint(double _x, double _y)
        {
            x = _x;
            y = _y;
        }

        // Переписанные операторы для работы с ToolPoint
        public static ToolPoint operator -(ToolPoint _a, ToolPoint _b)
        {
            if (_a == null)
            {
                // Обработка, если _a равно null
                // Возвращаем ToolPoint с нулевыми координатами
                return new ToolPoint(0, 0);
            }

            if (_b == null)
            { 
                return new ToolPoint(0, 0);
            }

            return new ToolPoint(_a.x - _b.x, _a.y - _b.y);
        }
        public static ToolPoint operator +(ToolPoint _a, ToolPoint _b)
        {
            return new ToolPoint(_a.x + _b.x, _a.y + _b.y);
        }
        public static ToolPoint operator *(ToolPoint _a, double s)
        {
            return new ToolPoint(_a.x * s, _a.y * s);
        }

        // Векторное произведение
        public static double CrossProduct(ToolPoint v1, ToolPoint v2)
        {
            return v1.x * v2.y - v2.x * v1.y;
        }

    }

}
