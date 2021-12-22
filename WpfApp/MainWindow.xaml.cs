using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ModelLibrary;
using Ookii.Dialogs.Wpf;
using Microsoft.EntityFrameworkCore;
//using System.Linq;
using System.Drawing;

namespace WpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        public static ResultContext db = new ResultContext();
        public static ObservableCollection<Results> resultCollection = db.Results.Local.ToObservableCollection();

        private static async Task Consumer()
        {
            while (true)
            {
                string type;
                Bitmap bitmap;
                float[] BBox;

                (type, bitmap, BBox) = await Detector.resultBufferBlock.ReceiveAsync();
                if (type == "end")
                {
                    db.SaveChanges();
                    break;
                }
                else
                {
                    db.AddElem(type, BBox, bitmap);
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = resultCollection;
        }

        private void btnflod_C(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            if ((bool)dialog.ShowDialog())
                TextBox_Path.Text = dialog.SelectedPath;
        }

        private async void btnRun_C(object sender, RoutedEventArgs e)
        {
            btnRun.IsEnabled = false;
            db.Clear();
            Detector.cancelTokenSource = new CancellationTokenSource();
            Detector.token = Detector.cancelTokenSource.Token;

            await Task.WhenAll(Detector.DetectImage(TextBox_Path.Text), Consumer());
            btnRun.IsEnabled = true;
        }

        private void btnCancel_C(object sender, RoutedEventArgs e)
        {
            Detector.cancelTokenSource.Cancel();
            btnRun.IsEnabled = true;
        }

        private void btnDelete_C(object sender, RoutedEventArgs e)
        {
            db.DeleteType(listBox_types.SelectedItem.ToString());
        }
    }
}
