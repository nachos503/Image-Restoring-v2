using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Image_Restoring_v2
{
    // Triangle - класс для построения треугольников
    public class Triangle
    {
        // точки образающие треугольник
        public ToolPoint[] points = new ToolPoint[3];
        //ребра треугольника
        public Arc[] arcs = new Arc[3];
        //какой-то цвет для картинки
        public System.Drawing.Color color;

        // Centroid - метод возвращающйи точку пересечения медиан треугольника (центроид)
        public ToolPoint Centroid
        {
            /*
             * points[0] и points[1] представляют первые две вершины треугольника.
                вычисляет вектор от первой вершины ко второй вершин
                вычисляет половину вектора между первой и второй вершинами
                вычисляет середину между первой и второй вершинами
                вычисляет вектор от середины к третьей вершине
                масштабирует вектор на 0.6666666 (приблизительно 2/3)
                вычитает масштабированный вектор из третьей вершины, получая центроид
             */
            get
            {
                return points[2] - ((points[2] - (points[0] + ((points[1] - points[0]) * 0.5))) * 0.6666666);
            }

            // свойство доступно только для чтения
            set { }
        }

        //Построение треугольника по трем точкам
        public Triangle(ToolPoint _a, ToolPoint _b, ToolPoint _c)
        {
            points[0] = _a;
            points[1] = _b;
            points[2] = _c;

            arcs[0] = new Arc(_a, _b);
            arcs[1] = new Arc(_b, _c);
            arcs[2] = new Arc(_c, _a);
        }

        // Построение треугольника по ребру и точке
        public Triangle(Arc _arc, ToolPoint _a)
        {
            points[0] = _arc.A;
            points[1] = _arc.B;
            points[2] = _a;

            arcs[0] = _arc;
            arcs[1] = new Arc(points[1], points[2]);
            arcs[2] = new Arc(points[2], points[0]);
        }

        // Построение треугольника по трем ребрам
        public Triangle(Arc _arc0, Arc _arc1, Arc _arc2)
        {
            arcs[0] = _arc0;
            arcs[1] = _arc1;
            arcs[2] = _arc2;

            points[0] = _arc0.A;
            points[1] = _arc0.B;

            if (_arc1.A == _arc0.A || _arc1.A == _arc0.B)
                points[2] = _arc1.B;
            else if (_arc1.B == _arc0.A || _arc1.B == _arc0.B)
                points[2] = _arc1.A;
            else if (points[2] != _arc2.A && points[2] != _arc2.B)
            {
                //Исключения
                throw new Exception("Попытка создать треугольник из трех непересекающихся ребер");
            }

        }

        //GetThirdPoint - метод получения третий точки треугольника, зная ребро
        public ToolPoint GetThirdPoint(Arc _arc)
        {
            for (int i = 0; i < 3; i++)
                if (_arc.A != points[i] && _arc.B != points[i])
                    return points[i];

            return null;
        }

        //GetArcBeatwen2Points - метод поиска ребра по двум заданным точкам
        public Arc GetArcBeatwen2Points(ToolPoint _a, ToolPoint _b)
        {
            for (int i = 0; i < 3; i++)
                if (arcs[i].A == _a && arcs[i].B == _b || arcs[i].A == _b && arcs[i].B == _a)
                    return arcs[i];

            return null;
        }

        //GetTwoOtherArcs - метод поиска всех ребер по одному заданному
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
