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
    public partial class Form1 : Form
    {
        private bool isProcessButtonPressed = true;
        private string imagePath;
        protected readonly Controller controller;
        private int currentIndex = 0;
        private readonly int indexIncrement = 1000;

        public Form1()
        {
            InitializeComponent();
            buttonForward.Enabled = false;
            buttonBackward.Enabled = false;
            buttonSave.Enabled = false;
            controller = new Controller();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void ButtonLoadImage_Click(object sender, EventArgs e)
        {
            if (controller.BitmapList.Count > 0)
            {
                controller.BitmapList.Clear();
            }
            controller.LoadImage(imagePath, pictureBox1);
            isProcessButtonPressed = false;
        }

        private void TruangulatorButton_Click(object sender, EventArgs e)
        {
            if (!isProcessButtonPressed)
            {
                try
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    controller.Triangulator(numericUpDown1, pictureBox1);
                    isProcessButtonPressed = true;
                    buttonForward.Enabled = true;
                    buttonBackward.Enabled = true;
                    buttonSave.Enabled = true;

                    stopwatch.Stop();
                    Process currentProcess = Process.GetCurrentProcess();

                    MessageBox.Show($"Триангуляция завершена успешно за {stopwatch.ElapsedMilliseconds} миллисекунд(ы).\nИспользование памяти: {currentProcess.WorkingSet64 / (1024 * 1024)} MB \n Для выбора изображений нажимайте на кнопки 'Вперед' и 'Назад'");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
                MessageBox.Show("Для начала загрузите изображение с помощью кнопки 'Загрузить изображение'");
        }

        private void NumericUpDown1_Validating(object sender, CancelEventArgs e)
        {
            decimal roundedValue = Math.Round(numericUpDown1.Value / 1000) * 1000;
            numericUpDown1.Value = roundedValue;
        }

        private void NumericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            controller.BitmapList.Clear();
            isProcessButtonPressed = false;
            currentIndex = 0;
        }

        private void ButtonForward_Click(object sender, EventArgs e)
        {
            if (isProcessButtonPressed)
            {
                controller.ShowNextImage(buttonForward, buttonBackward, numericUpDown1, pictureBox1);
            }
            else
            {
                MessageBox.Show("Сначала нажмите кнопку 'Триангуляция'.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ButtonBackward_Click(object sender, EventArgs e)
        {
            if (isProcessButtonPressed)
            {
                controller.ShowPreviousImage(buttonForward, buttonBackward, pictureBox1);
            }
            else
            {
                MessageBox.Show("Сначала нажмите кнопку 'Триангуляция'.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "JPEG Files (*.jpg)|*.jpg|All files (*.*)|*.*";
                saveFileDialog.FileName = "TriangulatedImage.jpg";

                DialogResult result = saveFileDialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    controller.BitmapList[currentIndex / indexIncrement].Save(saveFileDialog.FileName);
                }
            }
        }
    }
}
