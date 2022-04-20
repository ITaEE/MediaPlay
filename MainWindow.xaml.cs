using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;

namespace MediaPlay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer timer = new DispatcherTimer();
        bool posSliderDragging = false;
        String trackPath = "";
        private string datafile = "default.playlist";
        public MainWindow()
        {
            InitializeComponent();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timer.Tick += new EventHandler(Timer_Tick);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!posSliderDragging)
            {
                position_sld.Value = media_me.Position.TotalMilliseconds;
            }
            
        }


        private void PlayTrack()
        {
            bool ok = true;
            FileInfo fi = null;
            Uri src;
            try
            {
                fi = new FileInfo(trackPath);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                ok = false;
            }
            if (ok)
            {
                // check that the file actually exists
                if (!fi.Exists)
                {
                    System.Windows.Forms.MessageBox.Show("Cannot find " + trackPath);
                }
                else
                {
                    // trackPath += "xxx"; // uncomment to fake a failed file path!
                    // if the MediaElement can find its Source the MediFailed event-handler should take over..
                    src = new Uri(trackPath);
                    media_me.Source = src;
                    // assign the defaults (from slider positions) when a track starts playing
                    media_me.SpeedRatio = speed_sld.Value;
                    media_me.Volume = volume_sld.Value;
                    media_me.Balance = balance_sld.Value;
                    media_me.Play();
                    timer.Start();
                }
            }
        }

        private void start_btn_Click(object sender, RoutedEventArgs e)
        {
            //assign the defaults(from slider position) when a track starts playing
            //media_me.SpeedRatio = speed_sld.Value;
            //media_me.Volume = volume_sld.Value;
            //media_me.Balance = balance_sld.Value;
            //media_me.Play();
            //timer.Start();
            if (play_lbox.Items.Count > 0)
            {
                PlayPlayList();
            }
            else
            {
                PlayTrack();
            }
        }

        private void PlayPlayList()
        {
            int selectedItemIndex = -1;
            if(play_lbox.Items.Count > 0)
            {
                selectedItemIndex = play_lbox.SelectedIndex;
            }
            if(selectedItemIndex > -1)
            {
                trackPath = play_lbox.Items[selectedItemIndex].ToString();
                trackLabel.Content = trackPath;
                PlayTrack();
            }
        }

        private void stop_btn_Click(object sender, RoutedEventArgs e)
        {
            media_me.Stop();
            timer.Stop();
        }

        private void pause_btn_Click(object sender, RoutedEventArgs e)
        {
            media_me.Pause();
        }

        private void volume_sld_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            media_me.Volume = volume_sld.Value;
        }

        private void speed_sld_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            media_me.SpeedRatio = speed_sld.Value;
        }

        private void position_sld_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (posSliderDragging)
                media_me.Position = TimeSpan.FromMilliseconds(position_sld.Value);
        }

        private void media_me_MediaEnded(object sender, RoutedEventArgs e)
        {
            int nextTrackIndex = -1;
            int numberOfTracks = -1;
            media_me.Stop();
            numberOfTracks = play_lbox.Items.Count;
            if(numberOfTracks > 0)
            {
                nextTrackIndex = play_lbox.SelectedIndex + 1;
                if(nextTrackIndex >= numberOfTracks)
                {
                    nextTrackIndex = 0;
                }
                play_lbox.SelectedIndex = nextTrackIndex;
                PlayPlayList();
            }
        }

        private void media_me_MediaOpened(object sender, RoutedEventArgs e)
        {
            position_sld.Maximum = media_me.NaturalDuration.TimeSpan.TotalMilliseconds;
            speed_sld.Value = 1;
        }

    

        private void balance_sld_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            media_me.Balance = balance_sld.Value;
        }

        private void position_sld_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            posSliderDragging = false;
            media_me.Play();
        }

        private void position_sld_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            posSliderDragging = true;
            media_me.Stop();
        }

      
        private void speed_sld_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            media_me.Pause();
        }

        private void speed_sld_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            media_me.Play();
        }

        private void openFile_mitem_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            Nullable<bool> result;
            
            dlg.FileName = "";
            dlg.DefaultExt = ".mp3";
            dlg.Filter = ".mp3|*.mp3|.mpg|*.mpg|.wmv|*.wmv|All Files(*.*)|*.*";
            dlg.CheckFileExists = true;
            result = dlg.ShowDialog();
            if (result == true)
            {
                play_lbox.Items.Clear();
                play_lbox.Visibility = Visibility.Hidden;
                //Open Document
                trackPath = dlg.FileName;
                trackLabel.Content = trackPath;
                PlayTrack();
            }
        }

        private void openFolder_mitem_Click(object sender, RoutedEventArgs e)
        {
            String folderpath = "";
            string[] files;
            // Note: You must browse to add a reference to System.Windows.Forms
            // in Solution Explorer in order to have access to the FolderBrowserDialog			
            FolderBrowserDialog fd = new FolderBrowserDialog();
            DialogResult result = fd.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                folderpath = fd.SelectedPath;
            }
            if (folderpath != "")
            {
                play_lbox.Items.Clear();
                play_lbox.Visibility = Visibility.Visible;
                files = Directory.GetFiles(folderpath, "*.mp3");
                foreach (string fn in files)
                {
                    play_lbox.Items.Add(fn);
                }
                play_lbox.SelectedIndex = 0;
            }
        }

        private void exitApp_mitem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void media_me_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Unable to play " + trackPath + " [" + e.ErrorException.Message + "]");
        }

        private void openPlaylist_mitem_Click(object sender, RoutedEventArgs e)
        {
            XmlDocument xdoc = new XmlDocument();
            XmlNodeList xmlNodes;
            XmlNode pathNode;
            string track;
            bool ok = true;
            try
            {
                xdoc.Load(datafile);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Error Loading XML Data");
                ok = false;
            }
            if(ok)
            {
                play_lbox.Items.Clear();
                play_lbox.Visibility = Visibility.Visible;
                xmlNodes = xdoc.SelectNodes("Playlist/track");
                foreach (XmlNode node in xmlNodes)
                {
                    pathNode = node.SelectSingleNode("path");
                    track = pathNode.InnerText;
                    play_lbox.Items.Add(track);
                }
                play_lbox.SelectedIndex = 0;
            }
        }

        private void savePlaylist_mitem_Click(object sender, RoutedEventArgs e)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            XmlWriter xmlWriter;
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;
            settings.NewLineOnAttributes = true;
            xmlWriter = XmlWriter.Create(datafile, settings);
            ArrayList albumData = PlaylistTracks();
            if (albumData.Count == 0)
            {
                System.Windows.Forms.MessageBox.Show("No tracks to save!", "Error");
            }
            else
            {
                xmlWriter.WriteStartElement("Playlist");
                foreach (string s in albumData)
                {
                    xmlWriter.WriteStartElement("track");
                    xmlWriter.WriteElementString("path", s);
                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteEndElement();
                xmlWriter.Close();
            }
        }

        private ArrayList PlaylistTracks()
        {
            int i = 0;
            string trackname = "";
            int playListSize = play_lbox.Items.Count;
            ArrayList tracks = new ArrayList();
            if(playListSize > 0)
            {
                for ( i = 0; i < playListSize; i++)
                {
                    trackname = play_lbox.Items[i].ToString();
                    tracks.Add(trackname);
                }
            }
            return tracks;
        }
    }
}
