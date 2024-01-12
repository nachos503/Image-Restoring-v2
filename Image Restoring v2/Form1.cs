using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;

namespace Image_Restoring_v2
{
    /// <summary>
    ///  Класс для осуществления интерфеса программы.
    ///  Строка идентификатора "T:Image_Restoring_v2.Form1".
    /// </summary> 
    public partial class Form1 : Form
    {
        private bool isProcessButtonPressed = true;
        private string imagePath;
        protected readonly Controller controller;
        private int currentIndex = 0;
        private readonly int indexIncrement = 1000;

        /// <summary>
        /// Конструктор.
        /// Строка идентификатора "M:Image_Restoring_v2.Form1.#ctor".
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            buttonForward.Enabled = false;
            buttonBackward.Enabled = false;
            buttonSave.Enabled = false;
            controller = new Controller();
        }

        /// <summary>
        /// Обработка события 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Метод для нажатия кнопки "Загрузить изображение".
        /// Строка идентификатора "M:Image_Restoring_v2.Form1.ClickButtonLoadImage(System.Object,System.EventArgs)".
        /// </summary>
        /// <param name="sender">Загрузка.</param>
        /// <param name="e">Загрузка.</param>
        private void ClickButtonLoadImage(object sender, EventArgs e)
        {
            if (controller.BitmapList.Count > 0)
            {
                controller.BitmapList.Clear();
            }
            controller.LoadImage(imagePath, pictureBox1);
            isProcessButtonPressed = false;
        }

        /// <summary>
        /// Метод для нажатия кнопки "Триангулировать".
        /// Строка идентификатора "M:Image_Restoring_v2.Form1.ClickTruangulatorButton(System.Object,System.EventArgs)".
        /// </summary>
        /// <param name="sender">Загрузка.</param>
        /// <param name="e">Загрузка.</param>
        private void ClickTruangulatorButton(object sender, EventArgs e)
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

                    MessageBox.Show($"Триангуляция завершена успешно за {stopwatch.ElapsedMilliseconds} " +
                        $"миллисекунд(ы).\nИспользование памяти: {currentProcess.WorkingSet64 / (1024 * 1024)} " +
                        $"MB \n Для выбора изображений нажимайте на кнопки 'Вперед' и 'Назад'");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
                MessageBox.Show("Для начала загрузите изображение с помощью кнопки 'Загрузить изображение'");
        }

        /// <summary>
        /// Выбор количества точек.
        /// Строка идентификатора "M:Image_Restoring_v2.Form1.NumericUpDown1_Validating(System.Object,System.CancelEventArgs)".
        /// </summary>
        /// <param name="sender">Загрузка.</param>
        /// <param name="e">Загрузка.</param>
        private void NumericUpDown1_Validating(object sender, CancelEventArgs e)
        {
            decimal roundedValue = Math.Round(numericUpDown1.Value / 1000) * 1000;
            numericUpDown1.Value = roundedValue;
        }

        /// <summary>
        /// Изменение количества точек.
        /// Строка идентификатора "M:Image_Restoring_v2.Form1.NumericUpDown1_ValueChanged(System.Object,System.EventArgs)".
        /// </summary>
        /// <param name="sender">Загрузка.</param>
        /// <param name="e">Загрузка.</param>
        private void NumericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            controller.BitmapList.Clear();
            isProcessButtonPressed = false;
            currentIndex = 0;
        }

        /// <summary>
        /// Метод для нажатия кнопки "Вперед".
        /// Строка идентификатора "M:Image_Restoring_v2.Form1.ClickButtonForward(System.Object,System.EventArgs)".
        /// </summary>
        /// <param name="sender">Загрузка.</param>
        /// <param name="e">Загрузка.</param>
        private void ClickButtonForward(object sender, EventArgs e)
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

        /// <summary>
        /// Метод для нажатия кнопки "Назад".
        /// Строка идентификатора "M:Image_Restoring_v2.Form1.ClickButtonBackward(System.Object,System.EventArgs)".
        /// </summary>
        /// <param name="sender">Загрузка.</param>
        /// <param name="e">Загрузка.</param>
        private void ClickButtonBackward(object sender, EventArgs e)
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

        /// <summary>
        /// Метод для нажатия кнопки "Сохранить изображение".
        /// Строка идентификатора "M:Image_Restoring_v2.Form1.ClickSaveButton(System.Object,System.EventArgs)".
        /// </summary>
        /// <param name="sender">Загрузка.</param>
        /// <param name="e">Загрузка.</param>
        private void ClickSaveButton(object sender, EventArgs e)
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
