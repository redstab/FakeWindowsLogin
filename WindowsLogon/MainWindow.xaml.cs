using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.System.UserProfile;

namespace WindowsLogon
{
	public enum LockscreenType
	{
		Spotlight,
		Picture,
		Slideshow
	}
	public class LockScreenImage
	{

		[DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool DeleteObject([In] IntPtr hObject);

		public ImageSource ImageSourceFromBitmap(Bitmap bmp)
		{
			var handle = bmp.GetHbitmap();
			try
			{
				return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
			}
			finally { DeleteObject(handle); }
		}

		private Random rng = new Random();

		public string GetWallpaperOrder()
		{
			string SID = WindowsIdentity.GetCurrent().User.Value;
			return (string)GetRegValue(RegistryHive.LocalMachine, $@"SOFTWARE\Microsoft\Windows\CurrentVersion\SystemProtectedUserData\{SID}\AnyoneRead\LockScreen", "");
		}

		public char GetWallpaperChar()
		{
			return GetWallpaperOrder().First();
		}

		public string GetPictureWallpaperPath()
		{
			return $@"C:\ProgramData\Microsoft\Windows\SystemData\{SID}\ReadOnly\LockScreen_{GetWallpaperChar()}\LockScreen.jpg";
		}

		public string GetSlideshowPath()
		{
			string SlideShowLibrary = File.ReadAllText(@"C:\Users\02jli01.ADMFALUN\AppData\Local\Microsoft\Windows\LockScreenSlideshow\Slideshow.library-ms");
			var PathMatch = System.Text.RegularExpressions.Regex.Match(SlideShowLibrary, @"(?<=<url>)(.*)(?=<\/url>)");
			return PathMatch.Value;
		}
		public object GetRegValue(RegistryHive Hive, string SubKey, string Value)
		{
			var HiveKey = RegistryKey.OpenBaseKey(Hive, RegistryView.Registry64);
			var Key = HiveKey.OpenSubKey(SubKey);
			if (Key == null)
			{
				return null;
			}
			return Key.GetValue(Value);
		}

		private bool SlideShowEnabled()
		{
			return Convert.ToBoolean((int)GetRegValue(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Lock Screen", "SlideshowEnabled"));
		}

		private bool SpotlightWallpaper()
		{
			return Convert.ToBoolean((int)GetRegValue(RegistryHive.LocalMachine, $@"SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\LogonUI\Creative\{SID}", "RotatingLockScreenEnabled"));
		}

		private bool DefaultWallpaper()
		{
			return GetRegValue(RegistryHive.LocalMachine, $@"SOFTWARE\Microsoft\Windows\CurrentVersion\SystemProtectedUserData\{SID}\AnyoneRead\LockScreen", "") == null;
		}

		public System.Windows.Media.Brush GetAccent()
		{
			int hexcolor = (int)GetRegValue(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\DWM", "ColorizationColor");
			string hex = hexcolor.ToString("X");
			var converter = new BrushConverter();
			return (System.Windows.Media.Brush)converter.ConvertFromString("#" + hex);
		}

		private string SID { get { return WindowsIdentity.GetCurrent().User.Value; } }

		public LockscreenType WallpaperType { get; }

		public Bitmap GetWallpaper()
		{
			if (WallpaperType == LockscreenType.Spotlight)
			{
				return Properties.Resources.sample1;
			}
			else if (WallpaperType == LockscreenType.Picture)
			{
				if (DefaultWallpaper())
				{
					return new Bitmap(@"C:\Windows\Web\Screen\img100.jpg");
				}
				else
				{
					return new Bitmap(GetPictureWallpaperPath());
				}
			}
			else if (WallpaperType == LockscreenType.Slideshow)
			{
				return new Bitmap(Directory.GetFiles(GetSlideshowPath()).Last());
			}
			else
			{
				return null;
			}
		}

		public LockScreenImage()
		{
			if (SlideShowEnabled())
			{
				WallpaperType = LockscreenType.Slideshow;
			}
			else if (SpotlightWallpaper())
			{
				WallpaperType = LockscreenType.Spotlight;
			}
			else
			{
				WallpaperType = LockscreenType.Picture;
			}
		}
	}

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public LockScreenImage lsi = new LockScreenImage();

		private Storyboard myStoryboard;

		public MainWindow()
		{
			InitializeComponent();

			DoubleAnimation DateOpacity = new DoubleAnimation();
			DateOpacity.From = 1.0;
			DateOpacity.To = 0.0;
			DateOpacity.Duration = new Duration(TimeSpan.FromMilliseconds(100));

			DoubleAnimation DimmerOpacity = new DoubleAnimation();
			DimmerOpacity.From = 0.23;
			DimmerOpacity.To = 0.4;
			DimmerOpacity.Duration = new Duration(TimeSpan.FromMilliseconds(300));

			DoubleAnimation ClockOpacity = new DoubleAnimation();
			ClockOpacity.From = 1.0;
			ClockOpacity.To = 0.0;
			ClockOpacity.Duration = new Duration(TimeSpan.FromMilliseconds(100));

			ThicknessAnimation ClockMargin = new ThicknessAnimation();
			ClockMargin.From = new Thickness(30, 0, 0, 173);
			ClockMargin.To = new Thickness(30, 0, 0, 600);
			ClockMargin.Duration = new Duration(TimeSpan.FromMilliseconds(100));

			ThicknessAnimation DateMargin = new ThicknessAnimation();
			DateMargin.From = new Thickness(30, 0, 0, 96);
			DateMargin.To = new Thickness(30, 0, 0, 600);
			DateMargin.Duration = new Duration(TimeSpan.FromMilliseconds(100));

			myStoryboard = new Storyboard();
			myStoryboard.Children.Add(ClockOpacity);
			myStoryboard.Children.Add(ClockMargin);
			myStoryboard.Children.Add(DateOpacity);
			myStoryboard.Children.Add(DateMargin);
			myStoryboard.Children.Add(DimmerOpacity);

			Storyboard.SetTargetName(ClockOpacity, ClockLabel.Name);
			Storyboard.SetTargetName(ClockMargin, ClockLabel.Name);
			Storyboard.SetTargetName(DateOpacity, DateLabel.Name);
			Storyboard.SetTargetName(DateMargin, DateLabel.Name);

			Storyboard.SetTargetName(DimmerOpacity, Dimmer.Name);

			Storyboard.SetTargetProperty(ClockOpacity, new PropertyPath(OpacityProperty));
			Storyboard.SetTargetProperty(ClockMargin, new PropertyPath(MarginProperty));
			Storyboard.SetTargetProperty(DateOpacity, new PropertyPath(OpacityProperty));
			Storyboard.SetTargetProperty(DateMargin, new PropertyPath(MarginProperty));

			Storyboard.SetTargetProperty(DimmerOpacity, new PropertyPath(OpacityProperty));
		}

		public void datetime_tick(object sender, EventArgs e)
		{
			ClockLabel.Content = DateTime.Now.ToString("HH:mm");
			DateLabel.Content = DateTime.Now.ToString("dddd dd MMMM", CultureInfo.CurrentCulture);
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			Background = lsi.GetAccent();
			LockBackground.Source = lsi.ImageSourceFromBitmap(lsi.GetWallpaper());
			System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
			dispatcherTimer.Tick += new EventHandler(datetime_tick);
			dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
			dispatcherTimer.Start();
		}

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{

			//myStoryboard.Begin(ClockLabel);
			myStoryboard.Begin(this);
			//DateLabel.Visibility = Visibility.Hidden;
			//ClockLabel.Visibility = Visibility.Hidden;
		}
	}
}
