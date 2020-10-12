using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Threading;

namespace handle_globel_exception
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        //TextWriter errorWriter = new StreamWriter(File.Open("error.txt",FileMode.Append,FileAccess.Write,FileShare.ReadWrite));
        public MainWindow()
        {
            InitializeComponent();
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

        }

        /// <summary>
        /// 當非mian thread錯誤被抓到main thread時會觸發
        /// </summary>
        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Console.WriteLine("成功捕捉到");
        }

        /// <summary>
        /// Main Thread的例外都會被他抓到
        /// </summary>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            using (TextWriter errorWriter = new StreamWriter(File.Open("error.txt", FileMode.Append, FileAccess.Write, FileShare.ReadWrite)))
            {
                Exception exception = (Exception)e.ExceptionObject;
                errorWriter.WriteLine($"{DateTime.Now} {exception.Source}: {exception.Message}\n{exception.StackTrace}");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var task =
            Task.Run(() =>
            {
                Thread.Sleep(2000); //模擬線程耗時運作
                var test = Task.Run(() => throw new Exception("測試錯誤"));
                Task.WaitAll(test); //等待會讓例外被帶出來
            });
            ///不阻塞線程
            Task.Run(() =>
            {
                try
                {
                    Task.WaitAll(task); //等待會讓錯誤傳遞出來

                }
                catch (Exception)
                {
                    Console.WriteLine("等待引導出來的錯誤被抓到!!");
                }
                #region 使用GC強制觸發回收 重新引導錯誤到TaskScheduler_UnobservedTaskException
                GC.Collect();
                GC.WaitForPendingFinalizers();
                #endregion
            });
        }
    }
}
