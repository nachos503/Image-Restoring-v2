using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Image_Restoring_v2
{
    public partial class Form1 : Form
    {
        private string imagePath;
        private Bitmap bitmap;

        public Form1()
        {
            InitializeComponent();
        }

        private void ButtonLoadImage_Click(object sender, EventArgs e)
        {
            LoadImage();
        }

        private void ProcessButton_Click_1(object sender, EventArgs e)
        {
            Process();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private int currentIndex = 1000;

        private void button1_Click(object sender, EventArgs e)
        {
            string imageName = $"Triangulation{currentIndex}.jpg";

            Bitmap currentBitmap = new Bitmap(imageName);

            // Покажите обработанное изображение в PictureBox
            pictureBox1.Image = (Image)currentBitmap.Clone();
            pictureBox1.Invalidate();

            // Increment the index for the next image
            currentIndex += 1000;
        }

        private void LoadImage()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.bmp;*.jpg;*.jpeg;*.png;*.gif;*.tif;*.tiff|All Files|*.*";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    imagePath = openFileDialog.FileName;
                    bitmap = new Bitmap(imagePath);
                    pictureBox1.Image = (Image)bitmap.Clone();
                    pictureBox1.Invalidate();
                }
            }
        }

        private void Process()
        {
            ApplyInterlace(bitmap);

            // Создание списка точек для триангуляции
            List<ToolPoint> Points = new List<ToolPoint>
            {

                // Добавление "рамочных" точек
                new ToolPoint(0, 0),
                new ToolPoint(bitmap.Width, 0),
                new ToolPoint(bitmap.Width, bitmap.Height),
                new ToolPoint(0, bitmap.Height)
            };

            // Добавление случайных точек с минимальным расстоянием в один пиксель
            Random random = new Random();
            int numberOfRandomPoints = 10000; // Установите количество случайных точек
            int minDistance = 1; // Минимальное расстояние между точками в пикселях

            for (int i = 0; i < numberOfRandomPoints; i++)
            {
                ToolPoint randomPoint = GenerateRandomPoint(random, bitmap.Width, bitmap.Height, minDistance, Points);
                Points.Add(randomPoint);
            }

            // Создание объекта триангуляции
            Triangulation triangulation = new Triangulation(Points);

            // Рисование и закрашивание треугольников на изображении
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                int i = 0;
                foreach (var triangle in triangulation.triangles)
                {
                    i++;
                    Color color1 = GetPixel(bitmap, (int)triangle.points[0].x, (int)triangle.points[0].y);
                    Color color2 = GetPixel(bitmap, (int)triangle.points[1].x, (int)triangle.points[1].y);
                    Color color3 = GetPixel(bitmap, (int)triangle.points[2].x, (int)triangle.points[2].y);

                    float alpha = 2f / 3f; // коэффициент для линейной интерполяции

                    int avgR = (int)((1 - alpha) * color1.R + alpha * (color2.R + color3.R) / 2);
                    int avgG = (int)((1 - alpha) * color1.G + alpha * (color2.G + color3.G) / 2);
                    int avgB = (int)((1 - alpha) * color1.B + alpha * (color2.B + color3.B) / 2);

                    Color avgColor = Color.FromArgb(avgR, avgG, avgB);

                    // Закрашивание треугольника средним значением цвета
                    Brush brush = new SolidBrush(avgColor);
                    g.FillPolygon(brush, new Point[] {
                    new Point(Math.Max(0, (int)triangle.points[0].x), Math.Max(0, (int)triangle.points[0].y)),
                    new Point(Math.Max(0, (int)triangle.points[1].x), Math.Max(0, (int)triangle.points[1].y)),
                    new Point(Math.Max(0, (int)triangle.points[2].x), Math.Max(0, (int)triangle.points[2].y))
                });

                // Сохраняем каждую 1000-ую итерацию изображения с треугольниками
                // Нужно для наглядности процесса
                // Для прикола можно потом сделать, чтобы это заносилось в ??? (массив мб какой-нибудь если так можно)
                // ПОДУМАТЬ НАДО.
                    if (i % 1000 == 0)
                    {
                        bitmap.Save($"Triangulation{i}.jpg");
                    }
                }
            }

        }

        // Функция для генерации случайной точки с минимальным расстоянием от существующих точек
        static ToolPoint GenerateRandomPoint(Random random, int maxWidth, int maxHeight, int minDistance, List<ToolPoint> existingPoints)
        {
            while (true)
            {
                int randomX = random.Next(minDistance, maxWidth - minDistance);
                int randomY = random.Next(minDistance, maxHeight - minDistance);

                // Проверка расстояния от новой точки до существующих точек
                bool isValid = true;
                foreach (var existingPoint in existingPoints)
                {
                    int distanceSquared = (randomX - (int)existingPoint.x) * (randomX - (int)existingPoint.x) +
                                          (randomY - (int)existingPoint.y) * (randomY - (int)existingPoint.y);

                    if (distanceSquared < minDistance * minDistance)
                    {
                        isValid = false;
                        break;
                    }
                }

                if (isValid)
                    return new ToolPoint(randomX, randomY);
            }
        }

        // Функция для получения цвета пикселя с проверкой на границы изображения
        static Color GetPixel(Bitmap image, int x, int y)
        {
            x = Math.Max(0, Math.Min(x, image.Width - 1));
            y = Math.Max(0, Math.Min(y, image.Height - 1));
            return image.GetPixel(x, y);
        }

        static void ApplyInterlace(Bitmap image)
        {
            // Пример простого интерлейса - замена каждого пятого пикселя на черный
            for (int y = 1; y < image.Height; y += 5)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    image.SetPixel(x, y, Color.Black);
                }
            }
            image.Save("InterlacedImage.jpg");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Создаем объект OpenFileDialog
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                // Устанавливаем фильтр для файлов изображений
                saveFileDialog.Filter = "JPEG Files (*.jpg)|*.jpg|All files (*.*)|*.*";

                // Устанавливаем начальное имя файла и расширение
                saveFileDialog.FileName = "TriangulatedImage.jpg";

                // Открываем диалоговое окно сохранения файла
                DialogResult result = saveFileDialog.ShowDialog();

                // Проверяем, был ли файл выбран и нажата кнопка "Сохранить"
                if (result == DialogResult.OK)
                {
                    // Сохраняем изображение в выбранном месте
                    bitmap.Save(saveFileDialog.FileName);
                }
            }
        }
    }
}
