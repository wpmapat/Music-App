﻿using KalAcademyMusicApp.Models;
using System;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.IO;
using System.Linq;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;
using Windows.Storage;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace KalAcademyMusicApp
{
    public sealed partial class MainPage : Page
    {
        //   const string FileName = "playlist.json";
        private UIElement[] mainContentWindowVisibility;
        public MainWindowViewModel MainModel { get; }


        public MainPage()
        {
            this.InitializeComponent();
            MainModel = new MainWindowViewModel();
            mainContentWindowVisibility = new UIElement[] { SongCollection, MediaPlayerElement, Searchby };
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (this.MainModel != null && e.NavigationMode != NavigationMode.Back)
            {
                MainModel.InitializeFromFile(@"Playlist.json");
                //        MainModel.InitializeFromFile(FileName);
            }

            base.OnNavigatedTo(e);
        }

        /// <summary>
        /// On clicling Play button this method will get call 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            Song s = b.DataContext as Song;

            var musicFolder = Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Music).AsTask().Result;

            var musicFile = musicFolder.SaveFolder.GetFileAsync(s.SongPath).AsTask().Result;
            MediaPlayerElement.Source = MediaSource.CreateFromStorageFile(musicFile);

            IStorageFile imageFile = null;
            if (!string.IsNullOrWhiteSpace(s.ImagePath))
            {
                var file = musicFolder.SaveFolder.TryGetItemAsync(s.ImagePath).AsTask().Result;
                imageFile = file as IStorageFile;
                if (imageFile != null)
                {
                    MediaPlayerElement.PosterSource = await Helper.ConvertStorageToImage(imageFile);
                }
            }

            if (imageFile == null)
            {
                MediaPlayerElement.PosterSource = Helper.GetDefaultSongImage();
            }

            MediaPlayerElement.MediaPlayer.Play();

            //ToggleMainContentWindow(MediaPlayerElement);
            HomeListBoxItem.IsSelected = false;
            MusicPlayerListBoxItem.IsSelected = true;
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox t = sender as TextBox;
            string searchcontent = t.Text;

            var songs = HomeListBoxItem.IsSelected ? MainModel.SearchAllSongsByNameOrArtist(searchcontent) : MainModel.SearchMySongs(searchcontent);
            //After calling an API we need to rebind GridView with new data(In this case its a collection of songs by name or artist)
            SongCollectionView.ItemsSource = songs;
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
        }

        private async void IconsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If this is called during app startup because default SelectedIndex is set in xaml markup, all XAML controls aren't
            // initialized yet, so we just skip
            if (SongCollectionView != null)
            {
                //After calling an API we need to rebind GridView with new data.In this case we are refreshing the Gridview with new data

                if (HomeListBoxItem.IsSelected)
                {
                    ToggleMainContentWindow(SongCollection, Searchby);
                    SongCollectionView.ItemsSource = MainModel.Songs;
                }
                else if (MusicPlayerListBoxItem.IsSelected)
                {
                    ToggleMainContentWindow(MediaPlayerElement);
                }
                else if (MyCollectionListBoxItem.IsSelected)
                {
                    ToggleMainContentWindow(SongCollection, Searchby);
                    SongCollectionView.ItemsSource = MainModel.GetMySongs();
                }
                else if (AddSongListBoxItem.IsSelected)
                {
                    await AddMp3File();
                    SongCollectionView.ItemsSource = MainModel.Songs;
                }

                // User may have typed a search query when (s)he was on the other UI, so we clear the text since it is not relavent now.
                tbsearch.Text = "";
            }
        }

        private void AddToFavorite_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            SymbolIcon icon = (SymbolIcon)button.Content;
            if (button.DataContext is Song song)
            {
                song.IsFavorite = song.IsFavorite ? false : true;
                icon.Symbol = song.IsFavorite ? Symbol.UnFavorite : Symbol.Favorite;
            }

            if (MyCollectionListBoxItem.IsSelected)
            {
                SongCollectionView.ItemsSource = MainModel.GetMySongs();
            }
        }

        //private void ChkAddtoFavorite_Checked(object sender, RoutedEventArgs e)
        //{
        //    CheckBox c = sender as CheckBox;
        //    Song s = c.DataContext as Song;
        //    if (s != null)
        //    {
        //        s.IsFavorite = true;
        //    }
        //}

        //private void ChkAddtoFavorite_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    CheckBox c = sender as CheckBox;
        //    Song s = c.DataContext as Song;
        //    if (s != null)
        //    {
        //        s.IsFavorite = false;
        //        if (MyCollectionListBoxItem.IsSelected == true)
        //        {
        //            var songs = MainModel.GetMySongs();
        //            SongCollectionView.ItemsSource = songs;
        //        }
        //    }
        //    //private async void Button_ClickSave(object sender, RoutedEventArgs e)
        //    //{
        //    //    var helper = new Helper();
        //    //    await helper.SerializeDataToJson(MainModel.Songs, "Playlist.json");

        //    //    MainModel.Songs = await helper.ReadPlaylist("Playlist.json");

        //    //}
        //}

        private void ToggleMainContentWindow(params UIElement[] currentElements)
        {
            Array.ForEach(mainContentWindowVisibility, e => e.Visibility = currentElements.Any(ce => ce == e) ? Visibility.Visible : Visibility.Collapsed);
        }

        private async Task AddMp3File()
        {
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();
            openPicker.FileTypeFilter.Add(".mp3");

            var file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                var wholePath = file.Path;
                var musicWord = @"Music\";
                string fileName = GetPathFromMusic(wholePath, musicWord);
                string songName = GetSongName(wholePath);

                MainModel.Songs.Add(new Song(songName, "", "", "", fileName, false));
                await Helper.WriteDataToJson(MainModel.Songs, "Playlist.json");
            }
        }

        public static string GetPathFromMusic(string wholePath, string musicWord)
        {
            int startIndex = wholePath.IndexOf(musicWord);
            string path = wholePath.Substring(startIndex + musicWord.Length);
            return path;
        }

        public static string GetSongName(string wholePath)
        {
            var fileInfo = new FileInfo(wholePath);
            return fileInfo.Name.Replace(fileInfo.Extension, string.Empty);
        }
    }
}