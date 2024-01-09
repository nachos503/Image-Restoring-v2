using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Image_Restoring_v2
{
    // Arc - класс для построения ребер
    public class Arc
    {
        // точки конца ребра
        public ToolPoint A;
        public ToolPoint B;

        //Ссылка на треугольники в которые входит ребро
        public Triangle trAB;
        public Triangle trBA;

        //конструктор
        public Arc(ToolPoint _A, ToolPoint _B)
        {
            A = _A;
            B = _B;
        }

        // ArcIntersect - метод, возвращающий true усли два отрезка пересекаются
        public static bool ArcIntersect(Arc a1, Arc a2)
        {
            //обозначим для удобности точки концов отрезков
            ToolPoint p1, p2, p3, p4;
            p1 = a1.A;
            p2 = a1.B;
            p3 = a2.A;
            p4 = a2.B;

            //определение направления
            //ХУЙ ЗНАЕТ ЗАЧЕМ НАДО ГЕОМЕТРИЮ ПЕРЕЧИТАТЬ
            double d1 = Direction(p3, p4, p1);
            double d2 = Direction(p3, p4, p2);
            double d3 = Direction(p1, p2, p3);
            double d4 = Direction(p1, p2, p4);

            /*
             Векторное произведение этих двух векторов дает значение, которое указывает направление или ориентацию трех точек. Знак результата определяет, расположены ли точки по часовой стрелке или против часовой стрелки.
                Если результат положительный, то точки расположены против часовой стрелки.
                Если результат отрицательный, то точки расположены по часовой стрелке.
                Если результат равен нулю, то точки коллинеарны, то есть лежат на одной линии.
             */
            if (p1 == p3 || p1 == p4 || p2 == p3 || p2 == p4)
                return false;

            else if (((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) &
                     ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0)))
                return true;

            else if ((d1 == 0) && OnSegment(p3, p4, p1))
                return true;

            else if ((d2 == 0) && OnSegment(p3, p4, p2))
                return true;

            else if ((d3 == 0) && OnSegment(p1, p2, p3))
                return true;

            else if ((d4 == 0) && OnSegment(p1, p2, p4))
                return true;

            else
                return false;
        }

        // ArcIntersect - метод, возвращающий true усли два отрезка, заданные точками, пересекаются
        //МНЕ НЕ НРАВИТСЯ ЧТО В ДВУХ МЕТОДА ОДИНАКОВЫЙ КОД
        //НУЖНО ОСТАВИТЬ НИЖНИЙ МЕТОД КАК ЕСТЬ, А В ЕГО ПЕРЕГРУЗКЕ ВЫЗВАТЬ ЕГО ЖЕ
        public static bool ArcIntersect(ToolPoint p1, ToolPoint p2, ToolPoint p3, ToolPoint p4)
        {
            //определение направления
            double d1 = Direction(p3, p4, p1);
            double d2 = Direction(p3, p4, p2);
            double d3 = Direction(p1, p2, p3);
            double d4 = Direction(p1, p2, p4);

            if (p1 == p3 || p1 == p4 || p2 == p3 || p2 == p4)
                return false;

            else if (((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) &
                     ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0)))
                return true;

            else if ((d1 == 0) && OnSegment(p3, p4, p1))
                return true;

            else if ((d2 == 0) && OnSegment(p3, p4, p2))
                return true;

            else if ((d3 == 0) && OnSegment(p1, p2, p3))
                return true;

            else if ((d4 == 0) && OnSegment(p1, p2, p4))
                return true;

            else
                return false;
        }

        //GetCommonPoint - метод, возвращающий общую точку двух ребер
        public static ToolPoint GetCommonPoint(Arc a1, Arc a2)
        {
            //тупой перебор
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

        //IsConnectedWith - определяет, связаны ли ребра
        public bool IsConnectedWith(Arc _a)
        {
            // если точка иискомого ребра совпадает с точкой данного ребра
            if (A == _a.A || A == _a.B || B == _a.A || B == _a.B)
                return true;

            else return false;
        }

        //Direction - метод, возвращающий направление через векторное произведение
        private static double Direction(ToolPoint pi, ToolPoint pj, ToolPoint pk)
        {
            return ToolPoint.CrossProduct((pk - pi), (pj - pi));
        }

        // OnSegment - метод, который проверяет, лежит ли точка pk на отрезке, образованном двумя другими точками pi и pj
        private static bool OnSegment(ToolPoint pi, ToolPoint pj, ToolPoint pk)
        {
            //Исключения
            if (pk == null)
            {
                throw new Exception("Изображение слишком мало. Попробуйте уменьшить количество точек. \n Либо попробуйте снова.");
            }

            if ((Math.Min(pi.x, pj.x) <= pk.x && pk.x <= Math.Max(pi.x, pj.x)) && (Math.Min(pi.y, pj.y) <= pk.y && pk.y <= Math.Max(pi.y, pj.y)))
                return true;
            else
                return false;
        }
    }
}
