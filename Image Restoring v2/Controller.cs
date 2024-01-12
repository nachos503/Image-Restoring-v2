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
using System.IO;

namespace Image_Restoring_v2
{
    /// <summary>
    /// Класс для обработки изображения
    /// Строка идентификатора "T:Image_Restoring_v2.Controller"
    /// </summary>
    public class Controller : IImageProcessor
    {
        /// <summary>
        /// private Bitmap bitmap; - создаётся переменная bitmap типа Bitmap
        /// List<Bitmap> - список который содержит объекты типа Bitmap только для чтения
        /// indexIncrement - поле, содержащие индекс прироста, только для чтения
        /// currentIndex - поле, содержащие изменяющийся индекс
        /// Строка идентификатора "F:Image_Restoring_v2.Controller.bitmapList".
        /// Строка идентификатора "F:Image_Restoring_v2.Controller.bitmap".
        /// </summary>
        private Bitmap bitmap;
        private readonly List<Bitmap> bitmapList = new List<Bitmap>();
        private readonly int indexIncrement = 1000;
        private int currentIndex = 0;

        /// <summary>
        /// Определение свойства BitmapList
        /// Строка идентификатора "F:Image_Restoring_v2.Controller.BitmapList".
        /// </summary>
        public List<Bitmap> BitmapList => bitmapList;

        /// <summary>
        /// Метод, загружающий изображение из выбранного файла и его отображания его в указанном PictureBox
        /// Строка идентификатора "M:Image_Restoring_v2.Controller.LoadImage(Image_Restoring_v2.string,Image_Restoring_v2.PictureBox)".
        /// </summary>
        /// <param name="imagePath"></param>
        /// <param name="pictureBox"></param>
        public void LoadImage(string imagePath, PictureBox pictureBox)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.bmp;*.jpg;*.jpeg;*.png;*.gif;*.tif;*.tiff|All Files|*.*";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFileName = openFileDialog.FileName;
                    string selectedFileExtension = Path.GetExtension(selectedFileName).ToLower();

                    string[] allowedExtensions = { ".bmp", ".jpg", ".jpeg", ".png", ".gif", ".tif", ".tiff" };

                    if (allowedExtensions.Contains(selectedFileExtension))
                    {
                        imagePath = selectedFileName;
                        bitmap = new Bitmap(imagePath);
                        pictureBox.Image = (Image)bitmap.Clone();
                        pictureBox.Invalidate();
                    }
                    else
                    {
                        MessageBox.Show("Выберите файл с правильным форматом изображения.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Метод, выполняющий интерлейсинг изображения, те изменяющий цвет определенных пикселей изображения
        /// Строка идентификатора "M:Image_Restoring_v2.Controller.ApplyInterlace(Image_Restoring_v2.Bitmap)".
        /// </summary>
        /// <param name="image"></param>
        public void ApplyInterlace(Bitmap image)
        {
            for (int y = 1; y < image.Height; y += 5)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    image.SetPixel(x, y, Color.Black);
                }
            }
            bitmapList.Add(new Bitmap(image));
        }

        /// <summary>
        /// Метод, выполняющий триангуляцию изображения и создающий последовательность измененных изображений, сохраняя их в список bitmapList
        /// Строка идентификатора "M:Image_Restoring_v2.Controller.Triangulator(Image_Restoring_v2.NumericUpDown, Image_Restoring_v2.PictureBox)".
        /// </summary>
        /// <param name="numericUpDown"></param>
        /// <param name="pictureBox"></param>
        public void Triangulator(NumericUpDown numericUpDown, PictureBox pictureBox)
        {
            // Создаем копию оригинальной картинки
            Bitmap originalBitmap = new Bitmap(bitmap);
            currentIndex = 0;

            ApplyInterlace(originalBitmap);

            List<ToolPoint> Points = new List<ToolPoint>
            {
                new ToolPoint(0, 0),
                new ToolPoint(originalBitmap.Width, 0),
                new ToolPoint(originalBitmap.Width, originalBitmap.Height),
                new ToolPoint(0, originalBitmap.Height)
            };

            Random random = new Random();
            int numberOfRandomPoints = (int)numericUpDown.Value;
            int minDistance = 1;

            for (int i = 0; i < numberOfRandomPoints; i++)
            {
                ToolPoint randomPoint = GenerateRandomPoint(random, originalBitmap.Width, originalBitmap.Height, minDistance, Points);
                Points.Add(randomPoint);
            }

            Triangulation triangulation = new Triangulation(Points);

            object lockObject = new object();
            int j = 0;

            Parallel.ForEach(triangulation.triangles, triangle =>
            {
                j++;
                Color color1, color2, color3;
                lock (lockObject)
                {
                    color1 = GetPixel(originalBitmap, (int)triangle.points[0].x, (int)triangle.points[0].y);
                    color2 = GetPixel(originalBitmap, (int)triangle.points[1].x, (int)triangle.points[1].y);
                    color3 = GetPixel(originalBitmap, (int)triangle.points[2].x, (int)triangle.points[2].y);
                }

                float alpha = 2f / 3f;
                int avgR = (int)((1 - alpha) * color1.R + alpha * (color2.R + color3.R) / 2);
                int avgG = (int)((1 - alpha) * color1.G + alpha * (color2.G + color3.G) / 2);
                int avgB = (int)((1 - alpha) * color1.B + alpha * (color2.B + color3.B) / 2);
                Color avgColor = Color.FromArgb(avgR, avgG, avgB);

                lock (lockObject)
                {
                    using (Graphics g = Graphics.FromImage(originalBitmap))
                    {
                        Brush brush = new SolidBrush(avgColor);
                        g.FillPolygon(brush, new Point[] {
                            new Point(Math.Max(0, (int)triangle.points[0].x), Math.Max(0, (int)triangle.points[0].y)),
                            new Point(Math.Max(0, (int)triangle.points[1].x), Math.Max(0, (int)triangle.points[1].y)),
                            new Point(Math.Max(0, (int)triangle.points[2].x), Math.Max(0, (int)triangle.points[2].y))
                        });

                        if (j % indexIncrement == 0 || j == 0)
                        {
                            bitmapList.Add(new Bitmap(originalBitmap));
                        }
                    }
                }
            });

            pictureBox.Image = (Image)bitmapList[currentIndex / indexIncrement].Clone();
            pictureBox.Invalidate();
        }

        /// <summary>
        /// Метод, обрабатывающий показ следующего изображения из списка и управляющий доступностью кнопок "Вперед" и "Назад"
        /// Строка идентификатора "M:Image_Restoring_v2.Controller.ShowNextImage(Image_Restoring_v2.Button, Image_Restoring_v2.Button, Image_Restoring_v2.PictureBox, Image_Restoring_v2.NumericUpDown)"
        /// </summary>
        /// <param name="buttonForward"></param>
        /// <param name="buttonBackward"></param>
        /// <param name="numericUpDown"></param>
        /// <param name="pictureBox"></param>
        public void ShowNextImage(Button buttonForward, Button buttonBackward, NumericUpDown numericUpDown, PictureBox pictureBox)
        {
            if (currentIndex < (int)numericUpDown.Value * 2)
            {
                currentIndex += indexIncrement;
                buttonBackward.Enabled = true;

                Bitmap currentBitmap = bitmapList[currentIndex / indexIncrement];
                pictureBox.Image = (Image)currentBitmap.Clone();
                pictureBox.Invalidate();
            }

            if (currentIndex >= (int)numericUpDown.Value * 2)
            {
                buttonForward.Enabled = false;
            }
        }

        /// <summary>
        /// Метод, обрабатывающий отображение предыдущего изображения из списка и управляющий доступностью кнопок "Вперед" и "Назад"
        /// Строка идентификатора "M:Image_Restoring_v2.Controller.ShowPreviousImage(Image_Restoring_v2.Button, Image_Restoring_v2.Button, Image_Restoring_v2.PictureBox)".
        /// </summary>
        /// <param name="buttonForward"></param>
        /// <param name="buttonBackward"></param>
        /// <param name="pictureBox"></param>
        public void ShowPreviousImage(Button buttonForward, Button buttonBackward, PictureBox pictureBox)
        {
            if (currentIndex <= 1000)
            {
                buttonBackward.Enabled = false;
            }

            if (currentIndex >= indexIncrement)
            {
                currentIndex -= indexIncrement;
                buttonForward.Enabled = true;

                pictureBox.Image = (Image)bitmapList[currentIndex / indexIncrement].Clone();
                pictureBox.Invalidate();
            }
        }

        /// <summary>
        /// Метод, генерирующий случайную точку в пределах указанных размеров, при условии, что она находится на приемлемом расстоянии от существующих точек
        /// Строка идентификатора "M:Image_Restoring_v2.Controller.GenerateRandomPoint(Image_Restoring_v2.Random, Image_Restoring_v2.Int, Image_Restoring_v2.Int, Image_Restoring_v2.Int, Image_Restoring_v2.ToolPoint)"
        /// </summary>
        /// <param name="random"></param>
        /// <param name="maxWidth"></param>
        /// <param name="maxHeight"></param>
        /// <param name="minDistance"></param>
        /// <param name="existingPoints"></param>
        /// <returns></returns>
        static ToolPoint GenerateRandomPoint(Random random, int maxWidth, int maxHeight, int minDistance, List<ToolPoint> existingPoints)
        {
            while (true)
            {
                int randomX = random.Next(minDistance, maxWidth - minDistance);
                int randomY = random.Next(minDistance, maxHeight - minDistance);

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

        /// <summary>
        /// Метод, возвращающий цвет пикселя изображения по указанным координатам
        /// Строка идентификатора "M:Image_Restoring_v2.Controller.GetPixel(Image_Restoring_v2.Bitmap, Image_Restoring_v2.Int, Image_Restoring_v2.Int)".
        /// </summary>
        /// <param name="image"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        static Color GetPixel(Bitmap image, int x, int y)
        {
            x = Math.Max(0, Math.Min(x, image.Width - 1));
            y = Math.Max(0, Math.Min(y, image.Height - 1));
            return image.GetPixel(x, y);
        }
    }
}
