using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Image_Restoring_v2
{
    // Triangulation - класс дял построения триангуляции на изображении
    class Triangulation
    {
        // Список точек, на основе которых строятся треугольники
        public List<ToolPoint> points = new List<ToolPoint>();
        // Список треугольников
        public List<Triangle> triangles = new List<Triangle>();

        private readonly DynamicCache Cache = null;

        // Triangulation - очень странный и ебанутый конструктор 
        public Triangulation(List<ToolPoint> _points)
        {
            points = _points;

            // Инициализация кэша
            Cache = new DynamicCache(points[2]);

            // Добавление супер структуры (что бы то ни значило) (скорей всего это ничего не значит кто бы что не говорил)
            // По сути здесь добавялется в лист один треугольник по трем точкам и к нему добавляется по смежному ребру и третьей точке вторйо треугольник
            // По сути так называемая супер структура - два смежных треугольника
            triangles.Add(new Triangle(points[0], points[1], points[2]));
            triangles.Add(new Triangle(triangles[0].arcs[2], points[3]));

            // Добавление ссылок в ребра на смежные треугольники супер структуры
            triangles[0].arcs[2].trAB = triangles[1];
            triangles[1].arcs[0].trBA = triangles[0];

            // Добавление супер структуры в кэш
            // Добавление двух смежных треугольников в кэш
            Cache.Add(triangles[0]);
            Cache.Add(triangles[1]);

            Triangle CurentTriangle;
            Triangle NewTriangle0;
            Triangle NewTriangle1;
            Triangle NewTriangle2;

            Arc NewArc0;
            Arc NewArc1;
            Arc NewArc2;

            Arc OldArc0;
            Arc OldArc1;
            Arc OldArc2;

            // Проход по всем данным точкам
            for (int i = 4; i < _points.Count; i++)
            {
                // текущему треугольнику присваивается тот треугольник, в котором находится текущая точка
                CurentTriangle = GetTriangleForPoint(_points[i]);

                System.Console.Write("Текущий треугольник не 0?  ");
                // Если текущий треугольник существует
                if (CurentTriangle != null)
                {
                    //Создание новых ребер, которые совместно с ребрами преобразуемого треугольника образуют новые три треугольника 
                    NewArc0 = new Arc(CurentTriangle.points[0], _points[i]);
                    NewArc1 = new Arc(CurentTriangle.points[1], _points[i]);
                    NewArc2 = new Arc(CurentTriangle.points[2], _points[i]);

                    //Сохранение ребер преобразуемого треугольника
                    OldArc0 = CurentTriangle.GetArcBeatwen2Points(CurentTriangle.points[0], CurentTriangle.points[1]);
                    OldArc1 = CurentTriangle.GetArcBeatwen2Points(CurentTriangle.points[1], CurentTriangle.points[2]);
                    OldArc2 = CurentTriangle.GetArcBeatwen2Points(CurentTriangle.points[2], CurentTriangle.points[0]);

                    //Преобразование текущего треугольника в один из новых трех
                    NewTriangle0 = CurentTriangle;
                    NewTriangle0.arcs[0] = OldArc0;
                    NewTriangle0.arcs[1] = NewArc1;
                    NewTriangle0.arcs[2] = NewArc0;
                    NewTriangle0.points[2] = _points[i];

                    //Дополнительно создаются два треугольника
                    NewTriangle1 = new Triangle(OldArc1, NewArc2, NewArc1);
                    NewTriangle2 = new Triangle(OldArc2, NewArc0, NewArc2);

                    //Новым ребрам передаются ссылки на образующие их треугольники
                    NewArc0.trAB = NewTriangle0;
                    NewArc0.trBA = NewTriangle2;
                    NewArc1.trAB = NewTriangle1;
                    NewArc1.trBA = NewTriangle0;
                    NewArc2.trAB = NewTriangle2;
                    NewArc2.trBA = NewTriangle1;

                    //Передача ссылок на старые ребра
                    if (OldArc0.trAB == CurentTriangle)
                        OldArc0.trAB = NewTriangle0;
                    if (OldArc0.trBA == CurentTriangle)
                        OldArc0.trBA = NewTriangle0;

                    if (OldArc1.trAB == CurentTriangle)
                        OldArc1.trAB = NewTriangle1;
                    if (OldArc1.trBA == CurentTriangle)
                        OldArc1.trBA = NewTriangle1;

                    if (OldArc2.trAB == CurentTriangle)
                        OldArc2.trAB = NewTriangle2;
                    if (OldArc2.trBA == CurentTriangle)
                        OldArc2.trBA = NewTriangle2;

                    // Добавление в список новых треугольников
                    triangles.Add(NewTriangle1);
                    triangles.Add(NewTriangle2);

                    //Добавление в кэш новых треугольников
                    Cache.Add(NewTriangle0);
                    Cache.Add(NewTriangle1);
                    Cache.Add(NewTriangle2);

                    CheckDelaunayAndRebuild(OldArc0);
                    CheckDelaunayAndRebuild(OldArc1);
                    CheckDelaunayAndRebuild(OldArc2);
                    System.Console.Write("Проверка Делоне  ");
                }
                else
                {
                    continue;
                }
                System.Console.WriteLine("Пройдена " + i + " точка");
            }

            //Дополнительный проход для проверки на критерий Делоне
            for (int i = 0; i < triangles.Count; i++)
            {
                CheckDelaunayAndRebuild(triangles[i].arcs[0]);
                CheckDelaunayAndRebuild(triangles[i].arcs[1]);
                CheckDelaunayAndRebuild(triangles[i].arcs[2]);
            }
        }
        int iterationCount = 0;
        // GetTriangleForPoint - метод, возвращающий треугольник в котором находится данная точка
        private Triangle GetTriangleForPoint(ToolPoint _point)
        {
            //    System.Console.Write("100");
            // link - передача ссылки из кэша
            Triangle link = Cache.FindTriangle(_point);
            //   System.Console.Write("1");
            // если ссылка пустая - возврат первого треугольника
            if (link == null)
            {
                link = triangles[0];
            }
            //   System.Console.Write("2");
            // если по ссылке передали верный треугольник - возврат ссылки на треугольник
            if (IsPointInTriangle(link, _point))
            {
                return link;
                //       System.Console.Write("3");
            }
            // если найденный треугольник не подошел
            else
            {
                //  System.Console.Write("4");
                //Путь от центроида найденного треугольника до искомой точки
                Arc wayToTriangle = new Arc(_point, link.Centroid);
                //    System.Console.Write("5");
                Arc CurentArc;
                //    System.Console.Write("6");
                // Пока точка не окажется внутри треугольника
                while (!IsPointInTriangle(link, _point) && iterationCount <= 50)
                {
                    //       System.Console.Write("7");
                    // находим ребро, которое пересекается с найденным треугольником и некоторой прямой от искомой точки
                    CurentArc = GetIntersectedArc(wayToTriangle, link);
                    if (CurentArc == null)
                    {
                        return link;
                    }
                    //     System.Console.Write("8");

                    // присваиваем треугольник, в которое входит это ребро
                    // ТУТ ЕБУЧАЯ ОШИБКА ПОТОМУ ЧТО КАКОГО-ТО ХУЯ РЕБРО НЕ ПЕРЕСЕКАЕТСЯ
                    // ЧЕГО НАХУЙ НЕ МОЖЕТ БЫТЬ, ПОТОМУ ЧТО МЫ РИСУЕМ ИЗ ЦЕНТРА ТРЕУГОЛЬНИКА ДО ТОЧКИ
                    // ХОТЯ МОЖЕТ
                    // ЕСЛИ ЕБУЧАЯ ТОЧКА НАХОДИТСЯ ВНУТРИ ТРЕУГОЛЬНИКА
                    // ТОГДА КАКОГО ХУЯ ПРОСКАКАЛО ПРЕДЫДУЩИЕ ЭТАПЫ МУДИЛА
                    if (link == null) return link;
                    if (link == CurentArc.trAB)
                        link = CurentArc.trBA;
                    else
                        link = CurentArc.trAB;
                    //    System.Console.Write("9");

                    // если треугольник не найден, то переопределяем путь от точки до центроида нвоого треугольника
                    wayToTriangle = new Arc(_point, link.Centroid);
                    //    System.Console.Write("10");

                    iterationCount++;
                }
                // Возврат ссылки на треугольник
                iterationCount = 0;
                return link;
            }
        }

        // GetIntersectedArc - метод, возвращающий ребро треугольника которое пересекается с линией
        private static Arc GetIntersectedArc(Arc line, Triangle triangle)
        {
            if (Arc.ArcIntersect(triangle.arcs[0], line))
                return triangle.arcs[0];

            else if (Arc.ArcIntersect(triangle.arcs[1], line))
                return triangle.arcs[1];

            else if (Arc.ArcIntersect(triangle.arcs[2], line))
                return triangle.arcs[2];

            else
                return null;
        }

        // IsPointInTriangle - метод, возвращающий true если заданная точка находится в заданном треугольнике
        private static bool IsPointInTriangle(Triangle _triangle, ToolPoint _point)
        {
            // Для удобства присвоим всем точкам треугольника переменные
            ToolPoint P1 = _triangle.points[0];
            ToolPoint P2 = _triangle.points[1];
            ToolPoint P3 = _triangle.points[2];
            ToolPoint P4 = _point;

            /* Формула вычисляет определитель трех 2x2 матриц, образованных путем вычитания координат x и y точек
                a представляет определитель матрицы, образованной путем вычитания координат x и y точки P4 из P1 и P2 соответственно.
                b представляет определитель матрицы, образованной путем вычитания координат x и y точки P4 из P2 и P3 соответственно.
                c представляет определитель матрицы, образованной путем вычитания координат x и y точки P4 из P3 и P1 соответственно.
               Эта формула происходит из концепции барицентрических координат и широко используется в вычислительной геометрии для определения положения точки относительно многоугольника */
            double a = (P1.x - P4.x) * (P2.y - P1.y) - (P2.x - P1.x) * (P1.y - P4.y);
            double b = (P2.x - P4.x) * (P3.y - P2.y) - (P3.x - P2.x) * (P2.y - P4.y);
            double c = (P3.x - P4.x) * (P1.y - P3.y) - (P1.x - P3.x) * (P3.y - P4.y);

            /* Знак результирующих значений a, b и c может использоваться для определения ориентации точки P4 относительно треугольника:
                Если a, b и c все положительные или все отрицательные, то P4 находится внутри треугольника.
                Если любое из значений a, b или c равно нулю, то P4 находится на одной из сторон треугольника.
                Если a, b и c имеют разные знаки, то P4 находится вне треугольника.
            */
            if ((a > 0 && b > 0 && c > 0) || (a < 0 && b < 0 && c < 0) || (a == 0) || (b == 0) || (c == 0))
                return true;
            else
                return false;
        }

        //IsDelaunay - метод, вычисляющий принадлежность к критерию Делоне по описанной окружности
        //РАЗОБРАТЬ ПОТОМ ПОТОМУ ЧТО ЗДЕСЬ ОТКРОВЕННОЕ НЕПОНЯТНОЕ ДЕРЬМО
        private static bool IsDelaunay(ToolPoint A, ToolPoint B, ToolPoint C, ToolPoint _CheckNode)
        {

            if (_CheckNode == null)
            {
                throw new Exception("Изображение слишком мало. Попробуйте уменьшить количество точек.\nЛибо попробуйте снова.");
            }

            double x0 = _CheckNode.x;
            double y0 = _CheckNode.y;
            double x1 = A.x;
            double y1 = A.y;
            double x2 = B.x;
            double y2 = B.y;
            double x3 = C.x;
            double y3 = C.y;

            double[] matrix = { (x1 - x0)*(x1 - x0) + (y1 - y0)*(y1 - y0), x1 - x0, y1 - y0,
                                 (x2 - x0)*(x2 - x0) + (y2 - y0)*(y2 - y0), x2 - x0, y2 - y0,
                                 (x3 - x0)*(x3 - x0) + (y3 - y0)*(y3 - y0), x3 - x0, y3 - y0};

            double matrixDeterminant = matrix[0] * matrix[4] * matrix[8] + matrix[1] * matrix[5] * matrix[6] + matrix[2] * matrix[3] * matrix[7] -
                                        matrix[2] * matrix[4] * matrix[6] - matrix[0] * matrix[5] * matrix[7] - matrix[1] * matrix[3] * matrix[8];

            double a = x1 * y2 * 1 + y1 * 1 * x3 + 1 * x2 * y3
                     - 1 * y2 * x3 - y1 * x2 * 1 - 1 * y3 * x1;

            //Sgn(a)
            if (a < 0)
                matrixDeterminant *= -1d;

            if (matrixDeterminant < 0d)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //CheckDelaunayAndRebuild - метод который тожепроверяет принадлежность к критерию и перестраивает треугольник
        //АНАЛОГИЧНО
        private void CheckDelaunayAndRebuild(Arc arc)
        {
            Triangle T1;
            Triangle T2;
            //   System.Console.WriteLine("1");
            if (arc.trAB != null && arc.trBA != null)
            {
                T1 = arc.trAB;
                T2 = arc.trBA;
            }
            else
                return;
            //   System.Console.WriteLine("2");

            ToolPoint[] CurentPoints = new ToolPoint[4];
            //   System.Console.WriteLine("3");

            Arc NewArcT1A2;
            Arc NewArcT2A1;
            Arc NewArcT2A2;

            CurentPoints[0] = T1.GetThirdPoint(arc);
            CurentPoints[1] = arc.A;
            CurentPoints[2] = arc.B;
            CurentPoints[3] = T2.GetThirdPoint(arc);
            //    System.Console.WriteLine("4");

            //Дополнительная проверка, увеличивает скорость алгоритма на 10%
            if (Arc.ArcIntersect(CurentPoints[0], CurentPoints[3], CurentPoints[1], CurentPoints[2]))
                if (!IsDelaunay(CurentPoints[0], CurentPoints[1], CurentPoints[2], CurentPoints[3]))
                {
                    //      System.Console.WriteLine("5");

                    T1.GetTwoOtherArcs(arc, out Arc OldArcT1A1, out Arc OldArcT1A2);
                    T2.GetTwoOtherArcs(arc, out Arc OldArcT2A1, out Arc OldArcT2A2);

                    Arc NewArcT1A1;
                    //     System.Console.WriteLine("6");

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
                    //     System.Console.WriteLine("7");

                    //Изменение ребра
                    arc.A = CurentPoints[0];
                    arc.B = CurentPoints[3];

                    //переопределение ребер треугольников
                    T1.arcs[0] = arc;
                    T1.arcs[1] = NewArcT1A1;
                    T1.arcs[2] = NewArcT1A2;

                    T2.arcs[0] = arc;
                    T2.arcs[1] = NewArcT2A1;
                    T2.arcs[2] = NewArcT2A2;
                    //     System.Console.WriteLine("8");

                    //перезапись точек треугольников
                    T1.points[0] = arc.A;
                    T1.points[1] = arc.B;
                    T1.points[2] = Arc.GetCommonPoint(NewArcT1A1, NewArcT1A2);

                    T2.points[0] = arc.A;
                    T2.points[1] = arc.B;
                    T2.points[2] = Arc.GetCommonPoint(NewArcT2A1, NewArcT2A2);
                    //     System.Console.WriteLine("9");

                    //Переопределение ссылок в ребрах
                    if (NewArcT1A2.trAB == T2)
                        NewArcT1A2.trAB = T1;
                    else if (NewArcT1A2.trBA == T2)
                        NewArcT1A2.trBA = T1;

                    if (NewArcT2A1.trAB == T1)
                        NewArcT2A1.trAB = T2;
                    else if (NewArcT2A1.trBA == T1)
                        NewArcT2A1.trBA = T2;
                    //   System.Console.WriteLine("10");

                    //Добавление треугольников в кэш
                    Cache.Add(T1);
                    Cache.Add(T2);
                    //        System.Console.WriteLine("11");

                }
        }
    }
}
