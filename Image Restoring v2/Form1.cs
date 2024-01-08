using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace Image_Restoring_v2
{
    public partial class Form1 : Form, IImageProcessor
    {
        private string imagePath;
        private Bitmap bitmap;
        private bool isProcessButtonPressed = true;
        List<Bitmap> bitmapList = new List<Bitmap>();
        private int indexIncrement = 1000;
        private int currentIndex = 0;

        public Form1()
        {
            InitializeComponent();
            buttonForward.Enabled = false;
            buttonBackward.Enabled = false;
            buttonSave.Enabled = false;
        }

        private void ButtonLoadImage_Click(object sender, EventArgs e)
        {
            // Удаляем хранилище изображений перед загрузкой нового (если оно не пустое)
            if (bitmapList.Count > 0) 
            {
                bitmapList.Clear();
            }
            //Загружаем новое изображение 
            LoadImage();
            isProcessButtonPressed = false;
        }

        private void ProcessButton_Click(object sender, EventArgs e)
        {
            //Исключения
            if (!isProcessButtonPressed)
            {
                try
                {
                    // Используем Stopwatch для замера времени
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    Triangulator();
                    isProcessButtonPressed = true;
                    buttonForward.Enabled = true;
                    buttonBackward.Enabled = true;
                    buttonSave.Enabled=true;

                    stopwatch.Stop();
                    // Используем Process для получения информации об использовании памяти
                    Process currentProcess = Process.GetCurrentProcess();

                    MessageBox.Show($"Триангуляция завершена успешно за {stopwatch.ElapsedMilliseconds} миллисекунд(ы).\nИспользование памяти: {currentProcess.WorkingSet64 / (1024 * 1024)} MB \n Для выбора изображений нажимайте на кнопки 'Вперед' и 'Назад'");  
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
                MessageBox.Show("Для начала загрузите изорбажение с помощью кнопки 'Загрузить изображение'");
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void NumericUpDown1_Validating(object sender, CancelEventArgs e)
        {
            // Округляем введенное значение к ближайшему кратному 1000
            decimal roundedValue = Math.Round(numericUpDown1.Value / 1000) * 1000;

            // Устанавливаем округленное значение
            numericUpDown1.Value = roundedValue;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            bitmapList.Clear();
            isProcessButtonPressed = false;
            currentIndex = 0;
        }

        private void buttonForward_Click(object sender, EventArgs e)
        {
            // Если триангуляция прошла, можно жмать, иначе нельзя.
            if (isProcessButtonPressed)
            {
                ShowNextImage();
            }
            else
            {
                MessageBox.Show("Сначала нажмите кнопку 'Триангуляция'.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void buttonBackward_Click(object sender, EventArgs e)
        {
        // Если триангуляция прошла, можно жмать, иначе нельзя.
            if (isProcessButtonPressed)
            {
                ShowPreviousImage();
            }
            else
            {
                MessageBox.Show("Сначала нажмите кнопку 'Триангуляция'.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ShowNextImage()
        {
            if (isProcessButtonPressed && currentIndex < (int)numericUpDown1.Value * 2)
            {
                // Увеличиваем индекс для следующего изображения
                currentIndex += indexIncrement;
                buttonBackward.Enabled = true;

                Bitmap currentBitmap = bitmapList[currentIndex / indexIncrement];
                pictureBox1.Image = (Image)currentBitmap.Clone();
                pictureBox1.Invalidate();
            }

            if (currentIndex >= (int)numericUpDown1.Value * 2)
            {
                buttonForward.Enabled = false;
            }
        }

        private void ShowPreviousImage()
        {   

            if (currentIndex <= 1000)
            {
                buttonBackward.Enabled = false;
            }

            // Проверяем, чтобы индекс не становился меньше 0 и что кнопку можно нажмать
            if (isProcessButtonPressed && currentIndex >= indexIncrement)
            {
                currentIndex -= indexIncrement;
                buttonForward.Enabled=true;

                // Показываем обработанное изображение в PictureBox
                pictureBox1.Image = (Image)bitmapList[currentIndex / indexIncrement].Clone();
                pictureBox1.Invalidate();
            }
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

        //Реализаия метода интерфеса
        public void ApplyInterlace(Bitmap image)
        {
            // Пример простого интерлейса - замена каждого пятого пикселя на черный
            for (int y = 1; y < image.Height; y += 5)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    image.SetPixel(x, y, Color.Black);
                }
            }
            bitmapList.Add(image);
        }

        private void Triangulator()
        {
            ApplyInterlace(bitmap);
            Bitmap processedBitmap = new Bitmap(bitmap);

            List<ToolPoint> Points = new List<ToolPoint>
            {
                new ToolPoint(0, 0),
                new ToolPoint(bitmap.Width, 0),
                new ToolPoint(bitmap.Width, bitmap.Height),
                new ToolPoint(0, bitmap.Height)
            };

            Random random = new Random();
            int numberOfRandomPoints = (int)numericUpDown1.Value;
            int minDistance = 1;

            for (int i = 0; i < numberOfRandomPoints; i++)
            {
                ToolPoint randomPoint = GenerateRandomPoint(random, bitmap.Width, bitmap.Height, minDistance, Points);
                Points.Add(randomPoint);
            }

            Triangulation triangulation = new Triangulation(Points);

            object lockObject = new object(); // Объект блокировки для синхронизации доступа к общим данным
            int j = 0;

            //параллельный поток для закрашивания треугольников. каждый треугольник обрабатывается независимо от других
            Parallel.ForEach(triangulation.triangles, triangle =>
            {
                j++;
                Color color1, color2, color3;
                lock (lockObject) // Блокировка доступа к GetPixel
                {
                    color1 = GetPixel(processedBitmap, (int)triangle.points[0].x, (int)triangle.points[0].y);
                    color2 = GetPixel(processedBitmap, (int)triangle.points[1].x, (int)triangle.points[1].y);
                    color3 = GetPixel(processedBitmap, (int)triangle.points[2].x, (int)triangle.points[2].y);
                }

                float alpha = 2f / 3f;
                int avgR = (int)((1 - alpha) * color1.R + alpha * (color2.R + color3.R) / 2);
                int avgG = (int)((1 - alpha) * color1.G + alpha * (color2.G + color3.G) / 2);
                int avgB = (int)((1 - alpha) * color1.B + alpha * (color2.B + color3.B) / 2);
                Color avgColor = Color.FromArgb(avgR, avgG, avgB);

                lock (lockObject) // Блокировка доступа к изображению
                {
                    using (Graphics g = Graphics.FromImage(processedBitmap))
                    {
                        Brush brush = new SolidBrush(avgColor);
                        g.FillPolygon(brush, new Point[] {
                            new Point(Math.Max(0, (int)triangle.points[0].x), Math.Max(0, (int)triangle.points[0].y)),
                            new Point(Math.Max(0, (int)triangle.points[1].x), Math.Max(0, (int)triangle.points[1].y)),
                            new Point(Math.Max(0, (int)triangle.points[2].x), Math.Max(0, (int)triangle.points[2].y))
                        });

                        if (j % indexIncrement == 0 || j == 0)
                        {
                            bitmapList.Add(new Bitmap(processedBitmap));
                        }
                    }
                }
            });
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

        private void saveButton_Click(object sender, EventArgs e)
        {
            // Создаем объект SaveFileDialog
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
                    bitmapList[currentIndex/ indexIncrement].Save(saveFileDialog.FileName);
                }
            }
        }


    }
}
