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
    public class Controller : IImageProcessor
    {
        private Bitmap bitmap;
        private readonly List<Bitmap> bitmapList = new List<Bitmap>();
        private readonly int indexIncrement = 1000;
        private int currentIndex = 0;

        public List<Bitmap> BitmapList => bitmapList;

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

        static Color GetPixel(Bitmap image, int x, int y)
        {
            x = Math.Max(0, Math.Min(x, image.Width - 1));
            y = Math.Max(0, Math.Min(y, image.Height - 1));
            return image.GetPixel(x, y);
        }
    }
}
