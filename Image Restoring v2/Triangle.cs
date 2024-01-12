using System;
using System.Linq;

namespace Image_Restoring_v2
{
    /// <summary>
    ///  Класс для для построения треугольников.
    ///  Строка идентификатора "T:Image_Restoring_v2.Triangle".
    /// </summary>  
    public class Triangle
    {
        /// <summary>
        /// Массив типа ToolPoint, содержащий значение точек, образующие треугольник.
        /// Строка идентификатора "F:Image_Restoring_v2.Triangle.points".
        /// </summary>
        public ToolPoint[] points = new ToolPoint[3];

        /// <summary>
        /// Массив типа Arc, содержащий ребра треугольника.
        /// Строка идентификатора "F:Image_Restoring_v2.Triangle.arcs".
        /// </summary>
        public Arc[] arcs = new Arc[3];

        //какой-то цвет для картинки
        public System.Drawing.Color color;

        /// <summary>
        /// Метод, возвращающий точку пересечения медиан треугольника (центроид).
        /// Строка идентификатора "M:Image_Restoring_v2.Triangle.Centroid".
        /// </summary>
        /// <returns>Значение, равное точке пересечения медиан треугольника.</returns>
        public ToolPoint Centroid
        {
            // points[0] и points[1] представляют первые две вершины треугольника.
            // вычисляет вектор от первой вершины ко второй вершин
            // вычисляет половину вектора между первой и второй вершинами
            // вычисляет середину между первой и второй вершинами
            // вычисляет вектор от середины к третьей вершине
            // масштабирует вектор на 0.6666666 (приблизительно 2/3)
            // вычитает масштабированный вектор из третьей вершины, получая центроид
            get
            {
                return points[2] - ((points[2] - (points[0] + ((points[1] - points[0]) * 0.5))) * 0.6666666);
            }

            // Cвойство, доступное только для чтения.
            set { }
        }

        /// <summary>
        /// Конструктор для построения треугольника по трем точкам.
        /// Строка идентификатора "M:Image_Restoring_v2.Triangle.#ctor(Image_Restoring_v2.ToolPoint,Image_Restoring_v2.ToolPoint,Image_Restoring_v2.ToolPoint)".
        /// </summary>
        /// <param name="_a">Точка №1.</param>
        /// <param name="_b">Точка №2.</param>
        /// <param name="_c">Точка №3.</param>
        public Triangle(ToolPoint _a, ToolPoint _b, ToolPoint _c)
        {
            points[0] = _a;
            points[1] = _b;
            points[2] = _c;

            arcs[0] = new Arc(_a, _b);
            arcs[1] = new Arc(_b, _c);
            arcs[2] = new Arc(_c, _a);
        }

        /// <summary>
        /// Конструктор для построения треугольника по ребру и точке.
        /// Строка идентификатора "M:Image_Restoring_v2.Triangle.#ctor(Image_Restoring_v2.Arc,Image_Restoring_v2.ToolPoint)".
        /// </summary>
        /// <param name="_arc">Ребро.</param>
        /// <param name="_a">Точка.</param>
        public Triangle(Arc _arc, ToolPoint _a)
        {
            points[0] = _arc.A;
            points[1] = _arc.B;
            points[2] = _a;

            arcs[0] = _arc;
            arcs[1] = new Arc(points[1], points[2]);
            arcs[2] = new Arc(points[2], points[0]);
        }

        /// <summary>
        /// Конструктор для построения треугольника по трем ребрам.
        /// Строка идентификатора "M:Image_Restoring_v2.Triangle.#ctor(Image_Restoring_v2.Arc,Image_Restoring_v2.Arc,Image_Restoring_v2.Arc)".
        /// </summary>
        /// <param name="_arc0">Ребро №1.</param>
        /// <param name="_arc1">Ребро №2.</param>
        /// <param name="_arc2">Ребро №3.</param>
        public Triangle(Arc _arc0, Arc _arc1, Arc _arc2)
        {
            arcs[0] = _arc0;
            arcs[1] = _arc1;
            arcs[2] = _arc2;

            points[0] = _arc0.A;
            points[1] = _arc0.B;

            // Массивы, содержащие точки из ребер.
            ToolPoint[] arc1Points = new ToolPoint[] { _arc1.A, _arc1.B };
            ToolPoint[] arc2Points = new ToolPoint[] { _arc2.A, _arc2.B };

            // С помощью метода Intersect из LINQ находится пересечение
            // между arc1Points и arc2Points. Затем с помощью метода
            // FirstOrDefault получается первая общая точка или null,
            // если общей точки нет.
            ToolPoint point2 = arc1Points.Intersect(arc2Points).FirstOrDefault();

            if (point2 != null)
            {
                points[2] = point2;
            }
            else
            {
                throw new Exception("Попытка создать треугольник из трех непересекающихся ребер");
            }
        }

        /// <summary>
        /// Метод для получения третий точки треугольника по известному ребру.
        /// Строка идентификатора "M:Image_Restoring_v2.Triangle.GetThirdPoint(Image_Restoring_v2.Arc)".
        /// </summary>
        /// <param name="_arc">Ребро.</param>
        /// <returns>Значение, равное третьей точке треугольника, иначе false.</returns>
        public ToolPoint GetThirdPoint(Arc _arc)
        {
            for (int i = 0; i < 3; i++)
                if (_arc.A != points[i] && _arc.B != points[i])
                    return points[i];

            return null;
        }

        /// <summary>
        /// Метод для поиска ребра по двум заданным точкам.
        /// Строка идентификатора "M:Image_Restoring_v2.Triangle.GetArcBetween2Points(Image_Restoring_v2.ToolPoint,Image_Restoring_v2.ToolPoint)".
        /// </summary>
        /// <param name="_a">Точка №1.</param>
        /// <param name="_b">Точка №2.</param>
        /// <returns>Значение, равное искомому ребру треугольника, иначе null.</returns>
        public Arc GetArcBetween2Points(ToolPoint _a, ToolPoint _b)
        {
            for (int i = 0; i < 3; i++)
                if (arcs[i].A == _a && arcs[i].B == _b || arcs[i].A == _b && arcs[i].B == _a)
                    return arcs[i];

            return null;
        }

        /// <summary>
        /// Метод для поиска двух неизвестных ребер по одному заданному ребру.
        /// Строка идентификатора "M:Image_Restoring_v2.Triangle.GetTwoOtherArcs(Image_Restoring_v2.Arc,Image_Restoring_v2.Arc,Image_Restoring_v2.Arc)".
        /// </summary>
        /// <param name="_a0">Ребро №1.</param>
        /// <param name="_a1">Ребро №2.</param>
        /// <param name="_a2">Ребро №3.</param>
        /// <returns>Значения, равные искомым ребрам треугольника, иначе null.</returns>
        public void GetTwoOtherArcs(Arc _a0, out Arc _a1, out Arc _a2)
        {
            //ну тупой перебор епта
            if (arcs[0] == _a0)
            {
                _a1 = arcs[1];
                _a2 = arcs[2];
            }

            else if (arcs[1] == _a0)
            {
                _a1 = arcs[0];
                _a2 = arcs[2];
            }

            else if (arcs[2] == _a0)
            {
                _a1 = arcs[0];
                _a2 = arcs[1];
            }

            else
            {
                _a1 = null;
                _a2 = null;
            }
        }
    }
}
