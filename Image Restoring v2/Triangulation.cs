using System;
using System.Collections.Generic;

namespace Image_Restoring_v2
{
    /// <summary>
    ///  Класс для построения триангуляции на изображении.
    ///  Строка идентификатора "T:Image_Restoring_v2.Triangulation".
    /// </summary>  
    class Triangulation
    {
        /// <summary>
        /// Список точек, на основе которых строятся треугольники.
        /// Строка идентификатора "F:Image_Restoring_v2.Triangulation.points".
        /// </summary>      
        public List<ToolPoint> points = new List<ToolPoint>();

        /// <summary>
        /// Список треугольников.
        /// Строка идентификатора "F:Image_Restoring_v2.Triangulation.triangles".
        /// </summary>  
        public List<Triangle> triangles = new List<Triangle>();

        private readonly DynamicCache Cache = null;

        /// <summary>
        /// Конструктор.
        /// Строка идентификатора "M:Image_Restoring_v2.Triangulation.#ctor(Image_Restoring_v2.ToolPoint)".
        /// </summary>
        /// <param name="_points">Ребро.</param>
        public Triangulation(List<ToolPoint> _points)
        {
            points = _points;

            // Инициализация кэша.
            Cache = new DynamicCache(points[2]);

            // Добавление супер структуры.
            // Добавление в лист треугольника по трем точкам,
            // и к нему добавляется по смежному ребру и третьей точке второй треугольник.
            triangles.Add(new Triangle(points[0], points[1], points[2]));
            triangles.Add(new Triangle(triangles[0].arcs[2], points[3]));

            // Добавление ссылок в ребра на смежные треугольники супер структуры.
            triangles[0].arcs[2].trAB = triangles[1];
            triangles[1].arcs[0].trBA = triangles[0];

            // Добавление супер структуры в кэш.
            // Добавление двух смежных треугольников в кэш.
            Cache.Add(triangles[0]);
            Cache.Add(triangles[1]);

            Triangle CurrentTriangle;
            Triangle NewTriangle0;
            Triangle NewTriangle1;
            Triangle NewTriangle2;

            Arc NewArc0;
            Arc NewArc1;
            Arc NewArc2;

            Arc OldArc0;
            Arc OldArc1;
            Arc OldArc2;

            // Проход по всем данным точкам.
            for (int i = 4; i < _points.Count; i++)
            {
                // Присваивание текущему треугольнику тот треугольник,
                // в котором находится текущая точка.
                CurrentTriangle = GetTriangleForPoint(_points[i]);

                if (CurrentTriangle != null)
                {
                    // Создание новых ребер, которые совместно с ребрами
                    // преобразуемого треугольника образуют новые три треугольника.
                    NewArc0 = new Arc(CurrentTriangle.points[0], _points[i]);
                    NewArc1 = new Arc(CurrentTriangle.points[1], _points[i]);
                    NewArc2 = new Arc(CurrentTriangle.points[2], _points[i]);

                    // Сохранение ребер преобразуемого треугольника.
                    OldArc0 = CurrentTriangle.GetArcBetween2Points(CurrentTriangle.points[0], CurrentTriangle.points[1]);
                    OldArc1 = CurrentTriangle.GetArcBetween2Points(CurrentTriangle.points[1], CurrentTriangle.points[2]);
                    OldArc2 = CurrentTriangle.GetArcBetween2Points(CurrentTriangle.points[2], CurrentTriangle.points[0]);

                    // Преобразование текущего треугольника в один из новых трех.
                    NewTriangle0 = CurrentTriangle;
                    NewTriangle0.arcs[0] = OldArc0;
                    NewTriangle0.arcs[1] = NewArc1;
                    NewTriangle0.arcs[2] = NewArc0;
                    NewTriangle0.points[2] = _points[i];

                    // Создание двух треугольников.
                    NewTriangle1 = new Triangle(OldArc1, NewArc2, NewArc1);
                    NewTriangle2 = new Triangle(OldArc2, NewArc0, NewArc2);

                    // Передача новым ребрам ссылок на образующие их треугольники.
                    NewArc0.trAB = NewTriangle0;
                    NewArc0.trBA = NewTriangle2;
                    NewArc1.trAB = NewTriangle1;
                    NewArc1.trBA = NewTriangle0;
                    NewArc2.trAB = NewTriangle2;
                    NewArc2.trBA = NewTriangle1;

                    // Передача ссылок на старые ребра.
                    if (OldArc0.trAB == CurrentTriangle)
                        OldArc0.trAB = NewTriangle0;
                    if (OldArc0.trBA == CurrentTriangle)
                        OldArc0.trBA = NewTriangle0;

                    if (OldArc1.trAB == CurrentTriangle)
                        OldArc1.trAB = NewTriangle1;
                    if (OldArc1.trBA == CurrentTriangle)
                        OldArc1.trBA = NewTriangle1;

                    if (OldArc2.trAB == CurrentTriangle)
                        OldArc2.trAB = NewTriangle2;
                    if (OldArc2.trBA == CurrentTriangle)
                        OldArc2.trBA = NewTriangle2;

                    // Добавление в список новых треугольников.
                    triangles.Add(NewTriangle1);
                    triangles.Add(NewTriangle2);

                    // Добавление в кэш новых треугольников.
                    Cache.Add(NewTriangle0);
                    Cache.Add(NewTriangle1);
                    Cache.Add(NewTriangle2);

                    CheckDelaunayAndRebuild(OldArc0);
                    CheckDelaunayAndRebuild(OldArc1);
                    CheckDelaunayAndRebuild(OldArc2);
                }
                else
                {
                    continue;
                }
            }

            // Дополнительный проход для проверки на критерий Делоне.
            for (int i = 0; i < triangles.Count; i++)
            {
                CheckDelaunayAndRebuild(triangles[i].arcs[0]);
                CheckDelaunayAndRebuild(triangles[i].arcs[1]);
                CheckDelaunayAndRebuild(triangles[i].arcs[2]);
            }
        }

        int iterationCount = 0;

        /// <summary>
        /// Делегат.
        /// Строка идентификатора "T:Image_Restoring_v2.Triangulation.PointCondition".
        /// </summary>
        /// <param name="_triangle">Треугольник.</param>
        /// <param name="_point">Точка.</param>
        delegate bool PointCondition(Triangle _triangle, ToolPoint _point);

        /// <summary>
        /// Метод, возвращающий треугольник в котором находится данная точка.
        /// Строка идентификатора "M:Image_Restoring_v2.Triangulation.GetTriangleForPoint(Image_Restoring_v2.ToolPoint)".
        /// </summary>
        /// <param name="_point">Точка.</param>
        /// <returns>Ссылка из кэша.</returns>
        private Triangle GetTriangleForPoint(ToolPoint _point)
        {
            PointCondition pointInTriangle = IsPointInTriangle;

            // Передача ссылки из кэша.
            // Если ссылка пустая, то возврат первого треугольника.
            Triangle link = Cache.FindTriangle(_point) ?? triangles[0];

            // Если по ссылке передали верный треугольник,
            // то возврат ссылки на треугольник.
            if (pointInTriangle(link, _point))
            {
                return link;
            }
            // Если найденный треугольник не подошел.
            else
            {
                // Путь от центроида найденного треугольника до искомой точки.
                Arc wayToTriangle = new Arc(_point, link.Centroid);
                Arc CurentArc;
                while (!pointInTriangle(link, _point) && iterationCount <= 50)
                {
                    // Нахождение ребра, которое пересекается с найденным
                    // треугольником и некоторой прямой от искомой точки.
                    CurentArc = GetIntersectedArc(wayToTriangle, link);
                    if (CurentArc == null)
                    {
                        return link;
                    }

                    // Присваивание треугольника, в который входит это ребро.
                    if (link == null) return link;
                    if (link == CurentArc.trAB)
                        link = CurentArc.trBA;
                    else
                        link = CurentArc.trAB;

                    // Если треугольник не найден, то переопределяем
                    // путь от точки до центроида нвоого треугольника.
                    wayToTriangle = new Arc(_point, link.Centroid);

                    iterationCount++;
                }

                // Возврат ссылки на треугольник.
                iterationCount = 0;
                return link;
            }
        }

        /// <summary>
        /// Метод, возвращающий ребро треугольника, которое пересекается с линией.
        /// Строка идентификатора "M:Image_Restoring_v2.Triangulation.GetIntersectedArc(Image_Restoring_v2.Arc,Image_Restoring_v2.Triangle)".
        /// </summary>
        /// <param name="line">Ребро.</param>
        /// <param name="triangle">Линия.</param>
        /// <returns>Искомое ребро, иначе null.</returns>
        private static Arc GetIntersectedArc(Arc line, Triangle triangle)
        {
            if (Arc.IntersectArc(triangle.arcs[0], line))
                return triangle.arcs[0];

            else if (Arc.IntersectArc(triangle.arcs[1], line))
                return triangle.arcs[1];

            else if (Arc.IntersectArc(triangle.arcs[2], line))
                return triangle.arcs[2];

            else
                return null;
        }

        /// <summary>
        /// Метод для проверки нахождения заданной точки в заданном треугольнике.
        /// Строка идентификатора "M:Image_Restoring_v2.Triangulation.IsPointInTriangle(Image_Restoring_v2.Triangle,Image_Restoring_v2.ToolPoint)".
        /// </summary>
        /// <param name="_triangle">Треугольник.</param>
        /// <param name="_point">Точка.</param>
        /// <returns>Искомое ребро, иначе null.</returns>
        private static bool IsPointInTriangle(Triangle _triangle, ToolPoint _point)
        {
            // Для удобства присвоим всем точкам треугольника переменные.
            ToolPoint P1 = _triangle.points[0];
            ToolPoint P2 = _triangle.points[1];
            ToolPoint P3 = _triangle.points[2];
            ToolPoint P4 = _point;

            // Формула вычисляет определитель трех 2x2 матриц,
            // образованных путем вычитания координат x и y точек
            // a представляет определитель матрицы, образованной
            // путем вычитания координат x и y точки P4 из P1
            // и P2 соответственно.
            // b представляет определитель матрицы, образованной
            // путем вычитания координат x и y точки P4 из P2 и P3 соответственно.
            // c представляет определитель матрицы, образованной путем
            // вычитания координат x и y точки P4 из P3 и P1 соответственно.
            // Эта формула происходит из концепции барицентрических координат
            // и широко используется в вычислительной геометрии для определения
            // положения точки относительно многоугольника.
            double a = (P1.x - P4.x) * (P2.y - P1.y) - (P2.x - P1.x) * (P1.y - P4.y);
            double b = (P2.x - P4.x) * (P3.y - P2.y) - (P3.x - P2.x) * (P2.y - P4.y);
            double c = (P3.x - P4.x) * (P1.y - P3.y) - (P1.x - P3.x) * (P3.y - P4.y);

            // Знак результирующих значений a, b и c может использоваться
            // для определения ориентации точки P4 относительно треугольника:
            // Если a, b и c все положительные или все отрицательные,
            // то P4 находится внутри треугольника.
            // Если любое из значений a, b или c равно нулю, то P4 находится
            // на одной из сторон треугольника.
            // Если a, b и c имеют разные знаки, то P4 находится
            // вне треугольника.
            if ((a > 0 && b > 0 && c > 0) || (a < 0 && b < 0 && c < 0) || (a == 0) || (b == 0) || (c == 0))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Метод, вычисляющий принадлежность к критерию Делоне по описанной окружности.
        /// Строка идентификатора "M:Image_Restoring_v2.Triangulation.IsDelaunay(Image_Restoring_v2.ToolPoint,Image_Restoring_v2.ToolPoint,Image_Restoring_v2.ToolPoint,Image_Restoring_v2.ToolPoint)".
        /// </summary>
        /// <param name="A">Точка A.</param>
        /// <param name="B">Точка B.</param>
        /// <param name="C">Точка C.</param>
        /// <param name="_CheckNode">Проверка.</param>
        /// <returns>Логическое выражение в случае принадлежности к критерию Делоне.</returns>
        private static bool IsDelaunay(ToolPoint A, ToolPoint B, ToolPoint C, ToolPoint _CheckNode)
        {
            if (_CheckNode == null)
            {
                throw new Exception("Изображение слишком мало. Попробуйте уменьшить количество точек.\nЛибо попробуйте снова.");
            }

            double x0 = _CheckNode.x;
            double y0 = _CheckNode.y;
            double[] matrix = new double[]
            {
                (A.x - x0) * (A.x - x0) + (A.y - y0) * (A.y - y0),
                A.x - x0,
                A.y - y0,
                (B.x - x0) * (B.x - x0) + (B.y - y0) * (B.y - y0),
                B.x - x0,
                B.y - y0,
                (C.x - x0) * (C.x - x0) + (C.y - y0) * (C.y - y0),
                C.x - x0,
                C.y - y0
            };

            double matrixDeterminant = matrix[0] * matrix[4] * matrix[8] + matrix[1] * matrix[5] * matrix[6] + matrix[2] * matrix[3] * matrix[7] -
                                       matrix[2] * matrix[4] * matrix[6] - matrix[0] * matrix[5] * matrix[7] - matrix[1] * matrix[3] * matrix[8];

            double a = A.x * B.y * 1 + A.y * 1 * C.x + 1 * B.x * C.y - 1 * B.y * C.x - A.y * B.x * 1 - 1 * C.y * A.x;

            if (a < 0)
                matrixDeterminant *= -1d;

            return matrixDeterminant < 0d;
        }


        //CheckDelaunayAndRebuild - метод который тожепроверяет принадлежность к критерию и перестраивает треугольник
        /// <summary>
        /// Метод, проверяющий принадлежность к критерию и перестраивает треугольник.
        /// Строка идентификатора "M:Image_Restoring_v2.Triangulation.CheckDelaunayAndRebuild(Image_Restoring_v2.Arc)".
        /// </summary>
        /// <param name="arc">Ребро.</param>
        private void CheckDelaunayAndRebuild(Arc arc)
        {
            Triangle T1;
            Triangle T2;

            if (arc.trAB != null && arc.trBA != null)
            {
                T1 = arc.trAB;
                T2 = arc.trBA;
            }
            else
                return;

            ToolPoint[] CurentPoints = new ToolPoint[4];

            Arc NewArcT1A2;
            Arc NewArcT2A1;
            Arc NewArcT2A2;

            CurentPoints[0] = T1.GetThirdPoint(arc);
            CurentPoints[1] = arc.A;
            CurentPoints[2] = arc.B;
            CurentPoints[3] = T2.GetThirdPoint(arc);

            // Дополнительная проверка, увеличивает скорость алгоритма на 10%.
            if (Arc.IntersectArc(CurentPoints[0], CurentPoints[3], CurentPoints[1], CurentPoints[2]))
                if (!IsDelaunay(CurentPoints[0], CurentPoints[1], CurentPoints[2], CurentPoints[3]))
                {
                    T1.GetTwoOtherArcs(arc, out Arc OldArcT1A1, out Arc OldArcT1A2);
                    T2.GetTwoOtherArcs(arc, out Arc OldArcT2A1, out Arc OldArcT2A2);

                    Arc NewArcT1A1;

                    if (OldArcT1A1.IsConnectedWith(OldArcT2A1))
                    {
                        NewArcT1A1 = OldArcT1A1; NewArcT1A2 = OldArcT2A1;
                        NewArcT2A1 = OldArcT1A2; NewArcT2A2 = OldArcT2A2;
                    }
                    else
                    {
                        NewArcT1A1 = OldArcT1A1; NewArcT1A2 = OldArcT2A2;
                        NewArcT2A1 = OldArcT1A2; NewArcT2A2 = OldArcT2A1;
                    }

                    // Изменение ребра.
                    arc.A = CurentPoints[0];
                    arc.B = CurentPoints[3];

                    // Переопределение ребер треугольников.
                    T1.arcs[0] = arc;
                    T1.arcs[1] = NewArcT1A1;
                    T1.arcs[2] = NewArcT1A2;

                    T2.arcs[0] = arc;
                    T2.arcs[1] = NewArcT2A1;
                    T2.arcs[2] = NewArcT2A2;

                    // Перезапись точек треугольников.
                    T1.points[0] = arc.A;
                    T1.points[1] = arc.B;
                    T1.points[2] = Arc.GetCommonPoint(NewArcT1A1, NewArcT1A2);

                    T2.points[0] = arc.A;
                    T2.points[1] = arc.B;
                    T2.points[2] = Arc.GetCommonPoint(NewArcT2A1, NewArcT2A2);

                    // Переопределение ссылок в ребрах.
                    if (NewArcT1A2.trAB == T2)
                        NewArcT1A2.trAB = T1;
                    else if (NewArcT1A2.trBA == T2)
                        NewArcT1A2.trBA = T1;

                    if (NewArcT2A1.trAB == T1)
                        NewArcT2A1.trAB = T2;
                    else if (NewArcT2A1.trBA == T1)
                        NewArcT2A1.trBA = T2;

                    // Добавление треугольников в кэш.
                    Cache.Add(T1);
                    Cache.Add(T2);
                }
        }
    }
}
