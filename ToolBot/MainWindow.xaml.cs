using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.WebRequestMethods;


namespace ToolBot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);




        Boolean set = false;
        Boolean reset = true;
        private const int WM_HOTKEY = 0x71;
        public MainWindow()
        {
            InitializeComponent();
            this.PreviewKeyDown += MainWindow_PreviewKeyDown;
            myButton.Click += MyButton_Click;
            myButton_off.Click += MyButton_off_Click;
            myButton_clear.Click += MyButton_clear_Click;


            if (!System.IO.File.Exists("keylog.txt"))
            {
                System.IO.File.Create("keylog.txt").Close();
            }
            GlobalHook.EnableHook();
            GlobalHook.KeyEvents += OnKeyPress;

        }


        private void Window_Closed(object sender, EventArgs e)
        {
            GlobalHook.DisableHook();


        }
        private enum Keys : byte
        {
            VK_F6 = 0x75,
            VK_F12 = 0x7B,
        }

        private async void OnKeyPress(int vkCode)
        {
            // F6が押されたら
            if (vkCode == (byte)Keys.VK_F6)
            {

                using (StreamReader sr = new StreamReader("keylog.txt"))
                {
                    //    //string text = suuti.Text;
                    //   // int number = int.Parse(text);


                    if (int.TryParse(suuti.Text, out int number))
                    {


                        for (int i = 0; i < number; i++)
                        {
                            sr.BaseStream.Position = 0;

                            while (!sr.EndOfStream)
                            {
                                Key key = (Key)Enum.Parse(typeof(Key), sr.ReadLine());
                                if (key != Key.None)
                                {
                                    keybd_event((byte)KeyInterop.VirtualKeyFromKey(key), 0, 0, new UIntPtr((uint)0));

                                }
                            }
                        }
                    }
                }

            }
            if (vkCode == (byte)Keys.VK_F12) {
                reset = false;
            }
        }
        private void MainWindow_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

            if (!set)
            {
                return;
            }
            Key key = e.Key;

            using (StreamWriter sw = new StreamWriter("keylog.txt", true))
            {
                sw.WriteLine(key.ToString());
            }

            listBox.Items.Clear();
            using (StreamReader sr = new StreamReader("keylog.txt"))
            {
                while (!sr.EndOfStream)
                {
                    listBox.Items.Add(sr.ReadLine());
                }

            }
        }
        int HOTKEY_ID = WM_HOTKEY;

        private void MyButton_Click(object sender, RoutedEventArgs e)
        {
            set = true;


   
        }
        private void MyButton_off_Click(object sender, RoutedEventArgs e)
        {
            set = false;
        }
        private void MyButton_clear_Click(object sender, RoutedEventArgs e)
        {
            set = false;
            System.IO.File.WriteAllText("keylog.txt", string.Empty);
            listBox.Items.Clear();

        }
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            System.IO.File.Delete("keylog.txt");
        }
    }
}
