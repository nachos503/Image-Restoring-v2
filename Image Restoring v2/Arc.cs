using System;

namespace Image_Restoring_v2
{
    /// <summary>
    ///  Класс для построения ребер между точками.
    ///  Строка идентификатора "T:Image_Restoring_v2.Arc".
    /// </summary>  
    public class Arc
    {
        /// <summary>
        /// Поле, содержащее значение первого конца ребра.
        /// Строка идентификатора "F:Image_Restoring_v2.Arc.A".
        /// </summary>
        public ToolPoint A;

        /// <summary>
        /// Поле, содержащее значение второго конца ребра.
        /// Строка идентификатора "F:Image_Restoring_v2.Arc.B".
        /// </summary>
        public ToolPoint B;

        /// <summary>
        /// Поле, ссылающееся на треугольник trAB, в который входит ребро AB.
        /// Строка идентификатора "F:Image_Restoring_v2.Arc.trAB".
        /// </summary>
        public Triangle trAB;

        /// <summary>
        /// Поле, ссылающееся на треугольник trBA, в который входит ребро BA.
        /// Строка идентификатора "F:Image_Restoring_v2.Arc.trBA".
        /// </summary>
        public Triangle trBA;

        /// <summary>
        /// Конструктор.
        /// Строка идентификатора "M:Image_Restoring_v2.Arc.#ctor(Image_Restoring_v2.ToolPoint,Image_Restoring_v2.ToolPoint)".
        /// </summary>
        /// <param name="_A">Значение конца ребра А.</param>
        /// <param name="_B">Значение конца ребра B.</param>
        public Arc(ToolPoint _A, ToolPoint _B)
        {
            A = _A;
            B = _B;
        }

        /// <summary>
        /// Метод для проверки пересечения двух отрезков.
        /// Строка идентификатора "M:Image_Restoring_v2.Arc.IntersectArc(Image_Restoring_v2.Arc,Image_Restoring_v2.Arc)".
        /// </summary>
        /// <param name="a1">Объект №1.</param>
        /// <param name="a2">Объект №2.</param>
        /// <returns>True, если отрезки пересекаются, иначе bool.</returns>
        public static bool IntersectArc(Arc a1, Arc a2)
        {
            // Обозначение точек концов отрезков.
            ToolPoint p1, p2, p3, p4;
            p1 = a1.A;
            p2 = a1.B;
            p3 = a2.A;
            p4 = a2.B;

            // Поля для определения направления.
            double d1 = DetermineDirection(p3, p4, p1);
            double d2 = DetermineDirection(p3, p4, p2);
            double d3 = DetermineDirection(p1, p2, p3);
            double d4 = DetermineDirection(p1, p2, p4);

            // Векторное произведение этих двух векторов дает значение, которое указывает направление 
            // или ориентацию трех точек. Знак результата определяет, расположены ли точки по часовой 
            // стрелке или против часовой стрелки.
            // Если результат положительный, то точки расположены против часовой стрелки.
            // Если результат отрицательный, то точки расположены по часовой стрелке.
            // Если результат равен нулю, то точки коллинеарны, то есть лежат на одной линии.
            if (p1 == p3 || p1 == p4 || p2 == p3 || p2 == p4)
                return false;

            else if (((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) &
                     ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0)))
                return true;

            else if ((d1 == 0) && LieOnSegment(p3, p4, p1))
                return true;

            else if ((d2 == 0) && LieOnSegment(p3, p4, p2))
                return true;

            else if ((d3 == 0) && LieOnSegment(p1, p2, p3))
                return true;

            else if ((d4 == 0) && LieOnSegment(p1, p2, p4))
                return true;

            else
                return false;
        }

        /// <summary>
        /// Метод для проверки пересечения двух отрезков.
        /// Строка идентификатора "M:Image_Restoring_v2.Arc.IntersectArc(Image_Restoring_v2.Arc,Image_Restoring_v2.Arc)".
        /// </summary>
        /// <param name="a1">Объект №1.</param>
        /// <param name="a2">Объект №2.</param>
        /// <returns>True, если отрезки пересекаются, иначе bool.</returns>
        public static bool IntersectArc(ToolPoint p1, ToolPoint p2, ToolPoint p3, ToolPoint p4)
        {
            // Поля для определения направления.
            double d1 = DetermineDirection(p3, p4, p1);
            double d2 = DetermineDirection(p3, p4, p2);
            double d3 = DetermineDirection(p1, p2, p3);
            double d4 = DetermineDirection(p1, p2, p4);

            if (p1 == p3 || p1 == p4 || p2 == p3 || p2 == p4)
                return false;

            else if (((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) &
                     ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0)))
                return true;

            else if ((d1 == 0) && LieOnSegment(p3, p4, p1))
                return true;

            else if ((d2 == 0) && LieOnSegment(p3, p4, p2))
                return true;

            else if ((d3 == 0) && LieOnSegment(p1, p2, p3))
                return true;

            else if ((d4 == 0) && LieOnSegment(p1, p2, p4))
                return true;

            else
                return false;
        }

        /// <summary>
        /// Метод, возвращающий общую точку двух ребер.
        /// Строка идентификатора "M:Image_Restoring_v2.Arc.GetCommonPoint(Image_Restoring_v2.Arc,Image_Restoring_v2.Arc)".
        /// </summary>
        /// <param name="a1">Объект №1.</param>
        /// <param name="a2">Объект №2.</param>
        /// <returns>Значение типа Arc, обозначающее общую точку, иначе null.</returns>
        public static ToolPoint GetCommonPoint(Arc a1, Arc a2)
        {
            if (a1.A == a2.A)
                return a1.A;

            else if (a1.A == a2.B)
                return a1.A;

            else if (a1.B == a2.A)
                return a1.B;

            else if (a1.B == a2.B)
                return a1.B;

            else
                return null;
        }

        /// <summary>
        /// Метод, определяющий существование связи ребер.
        /// Строка идентификатора "M:Image_Restoring_v2.Arc.IsConnectedWith(Image_Restoring_v2.Arc)".
        /// </summary>
        /// <param name="_a">Точка конца ребра.</param>
        /// <returns>Значение типа bool, равное true, если точки совпадают, иначе false.</returns>
        public bool IsConnectedWith(Arc _a)
        {
            // Провека на совпадение точки конца искомого ребра с конца точкой данного ребра
            if (A == _a.A || A == _a.B || B == _a.A || B == _a.B)
                return true;

            else return false;
        }

        /// <summary>
        /// Метод, возвращающий направление через векторное произведение.
        /// Строка идентификатора "M:Image_Restoring_v2.Arc.Direction(Image_Restoring_v2.ToolPoint,Image_Restoring_v2.ToolPoint,Image_Restoring_v2.ToolPoint)".
        /// </summary>
        /// <param name="pi">Точка pi.</param>
        /// <param name="pj">Точка pj.</param>
        /// <param name="pk">Точка pk.</param>
        /// <returns>Значение типа ToolPoint, обозначающее направление через векторное произведение.</returns>
        private static double DetermineDirection(ToolPoint pi, ToolPoint pj, ToolPoint pk)
        {
            return ToolPoint.CrossProduct((pk - pi), (pj - pi));
        }

        /// <summary>
        /// Метод, который проверяет, лежит ли точка pk на отрезке, образованном двумя другими точками pi и pj.
        /// Строка идентификатора "M:Image_Restoring_v2.Arc.OnSegment(Image_Restoring_v2.ToolPoint,Image_Restoring_v2.ToolPoint,Image_Restoring_v2.ToolPoint)".
        /// </summary>
        /// <param name="pi">Точка pi.</param>
        /// <param name="pj">Точка pj.</param>
        /// <param name="pk">Точка pk.</param>
        /// <returns>Значение типа bool, равное true, если точка лежит на отрезке, иначе false.</returns>
        private static bool LieOnSegment(ToolPoint pi, ToolPoint pj, ToolPoint pk)
        {
            // Обработка исключений.
            if (pk == null)
            {
                throw new Exception("Изображение слишком мало. Попробуйте уменьшить " +
                    "количество точек. \n Либо попробуйте снова.");
            }

            if ((Math.Min(pi.x, pj.x) <= pk.x && pk.x <= Math.Max(pi.x, pj.x)) 
                && (Math.Min(pi.y, pj.y) <= pk.y && pk.y <= Math.Max(pi.y, pj.y)))
                return true;
            else
                return false;
        }
    }
}
