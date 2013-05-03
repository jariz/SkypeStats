using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SkypeStats
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public string Status
        {
            set
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    status.Content = value;
                }));
            }
        }

        public int Value
        {
            set
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (value > progressBar1.Maximum)
                        progressBar1.IsIndeterminate = true;
                    else
                    {
                        progressBar1.IsIndeterminate = false;
                        progressBar1.Value = value;
                    }
                }));
            }
        }

        public int Maximum
        {
            set
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    progressBar1.Maximum = value;
                }));
            }
        }

        public void AddValue(int value)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                progressBar1.Value += value;
            }));
        }

        public void Kill()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                //bai
                System.Windows.Forms.Application.Exit();
            }));
        }
    }
}
