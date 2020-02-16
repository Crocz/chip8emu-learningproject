using Chip8Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WPFFrontend
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IEmulatorHost
    {
        private int hits;
        private readonly Chip8Core.Chip8Emu emu;
        private readonly HashSet<Key> Chip8Keys = new HashSet<Key> { Key.NumPad0, Key.NumPad1, Key.NumPad2, Key.NumPad3, Key.NumPad4, Key.NumPad5, Key.NumPad6, Key.NumPad7, Key.NumPad8, Key.NumPad9, Key.Divide, Key.Multiply, Key.Subtract, Key.Add, Key.OemComma, Key.Enter };
        private static readonly int xsize = 64;
        private static readonly int ysize = 32;
        private static readonly int pixelside = 20;
        private SolidColorBrush Black = new SolidColorBrush(Colors.Black);
        private SolidColorBrush White = new SolidColorBrush(Colors.White);
        private SolidColorBrush Pink = new SolidColorBrush(Colors.HotPink);
        private readonly Rectangle[] screen = new Rectangle[xsize * ysize];
        private readonly Random r = new Random(1);
        public MainWindow()
        {
            InitializeComponent();
            InitializeScreen();
            emu = new Chip8Emu(this);
            //string path = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\Documents\chip8roms\Tetris [Fran Dachille, 1991].ch8");
            string path = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\Documents\chip8roms\test_opcode.ch8");
            byte[] rom = System.IO.File.ReadAllBytes(path);            
            this.KeyDown += new KeyEventHandler(OnButtonKeyDown);
            this.KeyUp += new KeyEventHandler(OnButtonKeyUp);            
            new Task(() => emu.Run(rom)).Start();            
        }

        public void InitializeScreen()
        {            
            for (int ypos = 0; ypos < ysize; ++ypos)
            {
                for (int xpos = 0; xpos < xsize; ++xpos)
                {
                    var color = new Color() { R = (byte)r.Next(byte.MaxValue), G = (byte)r.Next(byte.MaxValue), B = (byte)r.Next(byte.MaxValue), A = 128 };                                        
                    var rect = new Rectangle() { Width = pixelside, Height = pixelside, Fill = new SolidColorBrush(color) };
                    Canvas.SetLeft(rect, pixelside * xpos);
                    Canvas.SetTop(rect, pixelside * ypos);                    
                    screen[ypos * xsize + xpos] = rect;
                    Screen.Children.Add(rect);
                }
            }
        }

        private BitArray[] GetRandomScreenData()
        {
            var lines = new BitArray[32];
            for (int linenum = 0; linenum < lines.Length; ++linenum)
            {
                var line = new BitArray(64);
                for (int x = 0; x < line.Length; ++x)
                {
                    line[x] = r.Next(0, 2) == 0 ? false : true;
                }
                lines[linenum] = line;
            }
            return lines;
        }

        private void OnButtonKeyDown(object sender, KeyEventArgs e)
        {
            if (Chip8Keys.Contains(e.Key))
            {
                emu.OnKeyPress(KeyToInt(e.Key));
                //var lines = GetRandomScreenData();
                //UpdateDisplay(lines);
            }
        }

        private void OnButtonKeyUp(object sender, KeyEventArgs e)
        {
            if (Chip8Keys.Contains(e.Key))
            {
                emu.OnKeyRelease(KeyToInt(e.Key));
            }
        }

        private KeyMasks KeyToInt(Key key)
        {
            switch (key)
            {
                case Key.NumPad0: return KeyMasks.Zero;
                case Key.NumPad1: return KeyMasks.One;
                case Key.NumPad2: return KeyMasks.Two;
                case Key.NumPad3: return KeyMasks.Three;
                case Key.NumPad4: return KeyMasks.Four;
                case Key.NumPad5: return KeyMasks.Five;
                case Key.NumPad6: return KeyMasks.Six;
                case Key.NumPad7: return KeyMasks.Seven;
                case Key.NumPad8: return KeyMasks.Eigth;
                case Key.NumPad9: return KeyMasks.Nine;
                case Key.OemComma: return KeyMasks.A;
                case Key.Enter: return KeyMasks.B;
                case Key.Add: return KeyMasks.C;
                case Key.Subtract: return KeyMasks.D;
                case Key.Multiply: return KeyMasks.E;
                case Key.Divide: return KeyMasks.F;
                default: throw new Exception("Illegal key");
            }
        }

        public void UpdateDisplay(BitArray[] pixels)
        {
            if (screen[0].Dispatcher.Thread == Thread.CurrentThread)
            {
                if (hits < int.MaxValue)
                {
                    hits++;
                }
                int i = 0;
                foreach (var bitarray in pixels)
                {
                    int x = 0;
                    while (x < bitarray.Length)
                    {
                        screen[i + x].Fill = bitarray[x] ? White : Black;
                        ++x;
                    }
                    i += x;
                }
            }
            else
            {
                screen[0].Dispatcher.BeginInvoke((Action)(() => UpdateDisplay(pixels)));
            }
        }
    }
}
