using System;
using System.Windows.Forms;

namespace Image_Restoring_v2
{
    /// <summary>
    ///  Класс для входа в приложение.
    ///  Строка идентификатора "T:Image_Restoring_v2.Program".
    /// </summary> 
    internal static class Program
    {
        [STAThread]
        /// <summary>
        /// Метод для входа в приложение.
        /// Строка идентификатора "M:Image_Restoring_v2.Program.Main".
        /// </summary>
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
