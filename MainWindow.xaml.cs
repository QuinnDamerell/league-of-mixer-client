using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;
using System.Collections.ObjectModel;

namespace LeagueOfMixerClient
{
    public class ImageListObject
    {
        public string Name { get; set; }
        public BitmapImage Image { get; set; }
        public BitmapImage CrispyImage { get; set; }
        public string Text { get; set; }
    }


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Timer m_timer;
        public ObservableCollection<ImageListObject> ImagesItems { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            ImagesItems = new ObservableCollection<ImageListObject>();
            this.DataContext = this;
            App.GameStream.OnRawUpdate += GameStream_OnRawUpdate;
        }

        private void GameStream_OnRawUpdate(List<LeagueCore.LeagueTextRegionUpdate> updates)
        {
            Dispatcher.BeginInvoke((Action)delegate ()
            {
                ImagesItems.Clear();
                if (updates == null)
                {
                    ui_gameStateText.Visibility = Visibility.Visible;
                    ui_gameStateText.Text = "Game window not found..";
                    ui_gameStateText.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                }
                else
                {
                    ui_gameStateText.Visibility = Visibility.Collapsed;
                    foreach (var update in updates)
                    {
                        BitmapImage img = null;
                        BitmapImage crispy = null;
                        if (update.Bitmap != null)
                        {
                            using (MemoryStream memory = new MemoryStream())
                            {

                                update.Bitmap.Save(memory, ImageFormat.Png);
                                memory.Position = 0;
                                BitmapImage bitmapImage = new BitmapImage();
                                bitmapImage.BeginInit();
                                bitmapImage.StreamSource = memory;
                                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                                bitmapImage.EndInit();
                                img = bitmapImage;
                            }
                        }
                        if (update.CrispyBitmap != null)
                        {
                            using (MemoryStream memory = new MemoryStream())
                            {

                                update.CrispyBitmap.Save(memory, ImageFormat.Png);
                                memory.Position = 0;
                                BitmapImage bitmapImage = new BitmapImage();
                                bitmapImage.BeginInit();
                                bitmapImage.StreamSource = memory;
                                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                                bitmapImage.EndInit();
                                crispy = bitmapImage;
                            }
                        }
                        ImagesItems.Add(new ImageListObject() { Name = update.TextRegion.Name, Text = (update.Text == null ? "<null>" :update.Text.Trim()), Image = img, CrispyImage = crispy });
                    }
                 }
      
            });
        }
    }
}
