﻿using KalAcademyMusicApp.Models;
using System;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.IO;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace KalAcademyMusicApp
{
    public sealed partial class MainPage : Page
    {
        private static MediaPlayer mediaPlayer;

        public MainWindowViewModel MainModel { get; }

        public MainPage()
        {
            this.InitializeComponent();
            MainModel = new MainWindowViewModel();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (this.MainModel != null && e.NavigationMode != NavigationMode.Back)
            {
                MainModel.InitializeFromFile(@"Playlist.json");

                mediaPlayer = new MediaPlayer();
            }

            base.OnNavigatedTo(e);
        }

        /// <summary>
        /// On clicling Play button this method will get call 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            Song s = b.DataContext as Song;
            if (b.Content.ToString() == "Play")
            {
                var musicFolder = Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Music).AsTask().Result;
                var file = musicFolder.SaveFolder.GetFileAsync(s.SongPath).AsTask().Result;
                mediaPlayer.Source = MediaSource.CreateFromStorageFile(file);
                mediaPlayer.Play();
                b.Content = "Stop";

            }
            else
            {
                mediaPlayer.Pause();
                b.Content = "Play";
            }

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

        private void IconsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If this is called during app startup because default SelectedIndex is set in xaml markup, all XAML controls aren't
            // initialized yet, so we just skip
            if (SongCollectionView != null)
            {
                //After calling an API we need to rebind GridView with new data.In this case we are refreshing the Gridview with new data

                if (HomeListBoxItem.IsSelected)
                {
                    SongCollectionView.ItemsSource = MainModel.Songs;
                }
                else if (MusicPlayerListBoxItem.IsSelected)
                {
                    // do something
                }
                else if (MyCollectionListBoxItem.IsSelected)
                {
                    SongCollectionView.ItemsSource = MainModel.GetMySongs();
                }

                // User may have typed a search query when (s)he was on the other UI, so we clear the text since it is not relavent now.
                tbsearch.Text = "";
            }
        }

        private void ChkAddtoFavorite_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox c = sender as CheckBox;
            Song s = c.DataContext as Song;
            if (s != null)
            {
                s.IsFavorite = true;
            }
        }

        private void ChkAddtoFavorite_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox c = sender as CheckBox;
            Song s = c.DataContext as Song;
            if (s != null)
            {
                s.IsFavorite = false;
                if (MyCollectionListBoxItem.IsSelected == true)
                {
                    var songs = MainModel.GetMySongs();
                    SongCollectionView.ItemsSource = songs;

                }
            }
            //private async void Button_ClickSave(object sender, RoutedEventArgs e)
            //{
            //    var helper = new Helper();
            //    await helper.SerializeDataToJson(MainModel.Songs, "Playlist.json");

            //    MainModel.Songs = await helper.ReadPlaylist("Playlist.json");

            //}
        }
    }
}