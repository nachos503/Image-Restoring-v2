using System;

namespace Image_Restoring_v2
{
    /// <summary>
    ///  Класс кэша для хранения объектов типа Triangle.
    ///  Строка идентификатора "T:Image_Restoring_v2.DynamicCache".
    /// </summary> 
    class DynamicCache
    {
        /// <summary>
        /// Массив типа Triangle.
        /// Строка идентификатора "F:Image_Restoring_v2.DynamicCache.cache".
        /// </summary>
        private Triangle[] cache = new Triangle[4];

        /// <summary>
        /// Массив типа UInt32, хранящий текущий размер кэша.
        /// Строка идентификатора "F:Image_Restoring_v2.DynamicCache.cache".
        /// </summary>
        private UInt32 size = 2;

        /// <summary>
        /// Массив типа UInt32, хранящий треугольники в кэше.
        /// Строка идентификатора "F:Image_Restoring_v2.DynamicCache.inCache".
        /// </summary>
        private UInt32 inCache = 0;

        /// <summary>
        /// Поле, хранящее реальные размеры кэшируемого пространства.
        /// Строка идентификатора "F:Image_Restoring_v2.DynamicCache.sizeOfSpace".
        /// </summary>       
        private readonly ToolPoint sizeOfSpace;

        /// <summary>
        /// Поле, хранящее размеры одной ячейки кэша в пересчете на реальное пространство.
        /// Строка идентификатора "F:Image_Restoring_v2.DynamicCache.xSize".
        /// </summary>  
        private double xSize;

        /// <summary>
        /// Поле, хранящее размеры одной ячейки кэша в пересчете на реальное пространство.
        /// Строка идентификатора "F:Image_Restoring_v2.DynamicCache.ySize".
        /// </summary>  
        private double ySize;

        /// <summary>
        /// Конструктор.
        /// Строка идентификатора "M:Image_Restoring_v2.DynamicCache.#ctor(Image_Restoring_v2.ToolPoint)".
        /// </summary>
        /// <param name="_sizeOfSpace">Размер пространства.</param>
        public DynamicCache(ToolPoint _sizeOfSpace)
        {
            sizeOfSpace = _sizeOfSpace;
            xSize = sizeOfSpace.x / (double)size;
            ySize = sizeOfSpace.y / (double)size;
        }

        /// <summary>
        /// Метод для добавления треугольника в кэш.
        /// Строка идентификатора "M:Image_Restoring_v2.DynamicCache.AddTriangle(Image_Restoring_v2.Triangle)".
        /// </summary>
        /// <param name="_T">Треугольник.</param>
        public void AddTriangle(Triangle _T)
        {
            inCache++;

            if (inCache >= cache.Length * 3)
                IncreaseCache();

            cache[GetKey(_T.Centroid)] = _T;
        }

        /// <summary>
        /// Метод для поиска треугольника в кэше по заданной точке.
        /// Строка идентификатора "M:Image_Restoring_v2.DynamicCache.FindTriangle(Image_Restoring_v2.ToolPoint)".
        /// </summary>
        /// <param name="_T">Треугольник.</param>
        /// <returns>Искомый треугольник, иначе null.</returns>
        public Triangle FindTriangle(ToolPoint _Point)
        {
            UInt32 key = GetKey(_Point);
            if (cache[key] != null)
                return cache[key];

            // Дополнительный поиск не null ячейки для ускорения алгоритма.
            for (uint i = key - 25; i < key && i >= 0 && i < cache.Length; i++)
                if (cache[i] != null)
                    return cache[i];

            for (uint i = key + 25; i > key && i >= 0 && i < cache.Length; i--)
                if (cache[i] != null)
                    return cache[i];

            return null;
        }

        /// <summary>
        /// Метод для увеличения размера кэша в 4 раза.
        /// Строка идентификатора "M:Image_Restoring_v2.DynamicCache.IncreaseCache".
        /// </summary>
        private void IncreaseCache()
        {
            Triangle[] NewCache = new Triangle[(size * 2) * (size * 2)];
            UInt32 newIndex;

            // Передача ссылок из старого кэша в новый.
            for (UInt32 i = 0; i < cache.Length; i++)
            {
                newIndex = GetNewIndex(i);
                NewCache[newIndex] = cache[i];
                NewCache[newIndex + 1] = cache[i];
                NewCache[newIndex + size * 2] = cache[i];
                NewCache[newIndex + size * 2 + 1] = cache[i];
            }

            size *= 2;
            xSize = sizeOfSpace.x / (double)size;
            ySize = sizeOfSpace.y / (double)size;

            cache = NewCache;
        }

        /// <summary>
        /// Метод для вычисления ключа для точки.
        /// Строка идентификатора "M:Image_Restoring_v2.DynamicCache.GetKey(Image_Restoring_v2.ToolPoint)".
        /// </summary>
        /// <param name="_point">Точка.</param>
        /// <returns>Значение, равное ключу для точки.</returns>
        private UInt32 GetKey(ToolPoint _point)
        {
            UInt32 i = (UInt32)(_point.y / ySize);
            UInt32 j = (UInt32)(_point.x / xSize);

            if (i == size)
                i--;
            if (j == size)
                j--;

            return i * size + j;
        }

        /// <summary>
        /// Метод для вычисления индекса в новом массиве при увеличении размера кэша.
        /// Строка идентификатора "M:Image_Restoring_v2.DynamicCache.GetNewIndex(System.UInt32)".
        /// </summary>
        /// <param name="_OldIndex">Старый индекс.</param>
        /// <returns>Значение, равное искомому индексу.</returns>
        private UInt32 GetNewIndex(UInt32 _OldIndex)
        {
            UInt32 i = (_OldIndex / size) * 2;
            UInt32 j = (_OldIndex % size) * 2;

            return i * (size * 2) + j;
        }
    }
}
