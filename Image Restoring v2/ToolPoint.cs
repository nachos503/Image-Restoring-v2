namespace Image_Restoring_v2
{
    /// <summary>
    ///  Вспомогательный класс для работы с координатами точек.
    ///  Строка идентификатора "T:Image_Restoring_v2.ToolPoint".
    /// </summary>    
    public class ToolPoint
    {
        /// <summary>
        /// Поле, содержащее значение абсциссы точки.
        /// Строка идентификатора "F:Image_Restoring_v2.ToolPoint.x".
        /// </summary>
        public double x;

        /// <summary>
        /// Поле, содержащее значение ординаты точки.
        /// Строка идентификатора "F:Image_Restoring_v2.ToolPoint.y".
        /// </summary>
        public double y;

        /// <summary>
        /// Конструктор.
        /// Строка идентификатора "M:Image_Restoring_v2.ToolPoint.#ctor(System.Double,System.Double)".
        /// </summary>
        /// <param name="_x">Значение координаты x.</param>
        /// <param name="_y">Значение координаты y.</param>
        public ToolPoint(double _x, double _y)
        {
            x = _x;
            y = _y;
        }

        /// <summary>
        /// Перегрузка оператора вычитания для класса ToolPoint.
        /// Строка идентификатора "M:Image_Restoring_v2.ToolPoint.op_Difference(Image_Restoring_v2.ToolPoint,Image_Restoring_v2.ToolPoint)".
        /// </summary>
        /// <param name="_a">Вектор №1.</param>
        /// <param name="_b">Вектор №2.</param>
        /// <returns>Разность.</returns>  
        public static ToolPoint operator -(ToolPoint _a, ToolPoint _b)
        {
            if (_a == null)
            {
                // Обработка, если _a равно null, возвращаем с нулевыми координатами.
                return new ToolPoint(0, 0);
            }

            if (_b == null)
            { 
                return new ToolPoint(0, 0);
            }

            return new ToolPoint(_a.x - _b.x, _a.y - _b.y);
        }

        /// <summary>
        /// Перегрузка оператора сложения для класса ToolPoint.
        /// Строка идентификатора "M:Image_Restoring_v2.ToolPoint.op_Addition(Image_Restoring_v2.ToolPoint,Image_Restoring_v2.ToolPoint)".
        /// </summary>
        /// <param name="_a">Вектор №1.</param>
        /// <param name="_b">Вектор №2.</param>
        /// <returns>Сумма.</returns>     
        public static ToolPoint operator +(ToolPoint _a, ToolPoint _b)
        {
            return new ToolPoint(_a.x + _b.x, _a.y + _b.y);
        }

        /// <summary>
        /// Перегрузка оператора умножения для класса ToolPoint.
        /// Строка идентификатора "M:Image_Restoring_v2.ToolPoint.op_Multiplication(Image_Restoring_v2.ToolPoint,Image_Restoring_v2.ToolPoint)".
        /// </summary>
        /// <param name="_a">Вектор №1.</param>
        /// <param name="s">Вектор №2.</param>
        /// <returns>Произведение.</returns>
        public static ToolPoint operator *(ToolPoint _a, double s)
        {
            return new ToolPoint(_a.x * s, _a.y * s);
        }

        /// <summary>
        /// Метод для вычисления векторного произведения двух векторов.
        /// Строка идентификатора "M:Image_Restoring_v2.ToolPoint.CrossProduct(Image_Restoring_v2.ToolPoint,Image_Restoring_v2.ToolPoint)".
        /// </summary>
        /// <param name="v1">Вектор №1.</param>
        /// <param name="v2">Вектор №2.</param>
        /// <returns>Значение типа double, равное векторному произведению.</returns>
        public static double CrossProduct(ToolPoint v1, ToolPoint v2)
        {
            return v1.x * v2.y - v2.x * v1.y;
        }
    }
}
