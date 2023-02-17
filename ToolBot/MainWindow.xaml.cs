using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;
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


        private System.Timers.Timer _timer;

        Boolean set = false;
        Boolean reset = true;
        Boolean start = false;
        int i = 0;
        int l = 0;
        int s = 0;
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
            InitializeComponent();
    




        }
        private List<string> lines;  // ファイルの内容を保持するリスト
        private int currentLineIndex = 0;  // 現在処理中の行番号
        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (!start)
            {
                return;
            }
            // UIスレッドでリストの内容を処理する
            Dispatcher.Invoke(new Action(() =>
            {
                if (int.TryParse(suuti.Text, out int number))
                {
                    try
                    {
                        // linesにファイルの内容を読み込む
                       List<string> lines = System.IO.File.ReadAllLines("keylog.txt").ToList();
                        
                        // 残りの行数がない場合
                        if (currentLineIndex >= lines.Count)
                        {
                            if (number == i)
                            {
                                _timer.Stop();
                                start = false;
                                
                                return;
                            }
                            i++;
                            currentLineIndex = 0;
                        }

                        // 残りの行数がある場合
                        else if (currentLineIndex < lines.Count)
                        {
                            string line = lines[currentLineIndex];
                            currentLineIndex++;

                            // lineに対する処理
                            Key key = (Key)Enum.Parse(typeof(Key), line);
                            if (key != Key.None)
                            {
                                keybd_event((byte)KeyInterop.VirtualKeyFromKey(key), 0, 0, new UIntPtr((uint)0));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // ファイルの読み込みに失敗した場合はエラーメッセージを表示する
                        MessageBox.Show($"ファイルの読み込みに失敗しました: {ex.Message}");
                    }
                }
            }));
        }

        // ファイルの読み込み処理
        private void LoadFile()
        {
            lines = new List<string>();
            using (StreamReader sr = new StreamReader("keylog.txt"))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    lines.Add(line);
                }
            }
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
              if (int.TryParse(kankaku.Text, out int kankaku_))
            {
                    _timer = new System.Timers.Timer();
                    _timer.Interval = kankaku_; // 1秒ごとに実行する
                    _timer.Elapsed += OnTimerElapsed;
                    _timer.AutoReset = true; // タイマーを繰り返し実行する
                    _timer.Enabled = true; // タイマーを有効にする
                }
                start = true;
                _timer.Start();
                i = 0;


            }
            if (vkCode == (byte)Keys.VK_F12)
            {
                reset = false;
                start = false;
                _timer.Stop();
                i = 0;
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
