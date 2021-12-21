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
using YOLOv4MLNet;
using Ookii.Dialogs.Wpf;

namespace WpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        public static ObservableCollection<Results> result = new ObservableCollection<Results>();

        private static async Task Consumer()
        {
            
            string image;
            string t;
            while (true)
            {
                (t, image) = await Detector.resultBufferBlock.ReceiveAsync();
                if (t == "end")
                {
                    break;
                }

                bool flag = true;
                foreach (Results r in result)
                {
                    if (r.TYPE == t)
                    {
                        r.img.Add(image);
                        flag = false;
                        break;
                    }
                }
                if (flag==true)
                {
                    result.Add(new Results(t, image));
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = result;
        }

        private void btnflod_C(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            if ((bool)dialog.ShowDialog())
                TextBox_Path.Text = dialog.SelectedPath;
        }

        private async void btnRun_C(object sender, RoutedEventArgs e)
        {
          

            await Task.WhenAll(Detector.DetectImage(TextBox_Path.Text), Consumer());
           
        }


    }
}