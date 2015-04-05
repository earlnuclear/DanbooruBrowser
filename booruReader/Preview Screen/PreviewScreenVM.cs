﻿using booruReader.Helpers;
using booruReader.Model;
using dbz.UIComponents;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

// ReSharper disable SpecifyACultureInStringConversionExplicitly

namespace booruReader.Preview_Screen
{
    class PreviewScreenVM : BaseIObservable
    {
        private BasePost _post;
        private string _imageSource;
        private Visibility _showTaglist;
        private bool CopyWhenReady = false;
        private ObservableCollection<BasePost> _downloadList;
        public ObservableCollection<string> TagList { get; set; }
        private FavoriteHandler _favoriteshandler;
        private bool _favoriteWhenReady = false;
        public event EventHandler AddedImageToFavorites;
        public event EventHandler RemovedImageFromFavorites;

        public string ImageSource
        {
            get
            {
                return _imageSource;
            }
            set
            {
                _imageSource = value;
                RaisePropertyChanged("ImageSource");
            }
        }

        public BasePost PreviewPost
        {
            get
            {
                return _post;
            }
            set
            {
                _post = value;
                RaisePropertyChanged("PreviewPost");
            }
        }

        public Visibility ShowTagList
        {
            get
            {
                return _showTaglist;
            }
            set
            {
                _showTaglist = value;
                RaisePropertyChanged("ShowTagList");
            }
        }

        public PreviewScreenVM(BasePost post, ObservableCollection<BasePost> downloadList)
        {
            _post = post;
            _downloadList = downloadList;
            ImageCache cache = new ImageCache();
            _favoriteshandler = new FavoriteHandler();

            // KBR 20150405 Issue #5: extend GetImage to take a progress handler, so the preview download progress is visible
            PreviewPost = post;
            ImageSource = cache.GetImage(post.FileMD, post.FullPictureURL, LateFilePath, _post.Client_DownloadProgressChanged);

            //ImageSource = _post.FullPictureURL;
            ShowTagList = Visibility.Collapsed;

            string[] splitter = { " ", "\n", "\r" };
            TagList = new ObservableCollection<string>(post.Dimensions.Split(splitter, StringSplitOptions.RemoveEmptyEntries));
            //if (!string.IsNullOrEmpty(post.Tags))
            //    _taglist = new ObservableCollection<string>(post.Tags.Split(splitter, StringSplitOptions.RemoveEmptyEntries));
            //else
            //    _taglist = new ObservableCollection<string>();
        }

        private void LateFilePath(object e, AsyncCompletedEventArgs args)
        {
            //Yeah, I know, should really pass the flippin thing as a argument.
            ImageCache cache = new ImageCache();

            ImageSource = cache.GetImage(_post.FileMD, _post.FullPictureURL, LateFilePath);

            if (CopyWhenReady)
            {
                _post.SaveImage(ImageSource);

                if (_downloadList.FirstOrDefault(x => x == _post) == null)
                    _downloadList.Add(_post);
            }

            if (_favoriteWhenReady)
                _favoriteshandler.AddToFavorites(_post, ImageSource);

            // KBR 20150405 Issue #5: hide preview download progress on completion
            PreviewPost.ProgressBarVisible = Visibility.Collapsed;
        }

        internal void Download()
        {
            if (string.IsNullOrEmpty(GlobalSettings.Instance.SavePath))
            {
                new MetroMessagebox("Error", "No save directory specified. \nPlease go to settings and select a folder.").ShowDialog();
            }
            else
            {
                if (ImageSource != null)
                {
                    _post.SaveImage(ImageSource);

                    if (_downloadList.FirstOrDefault(x => x == _post) == null)
                        _downloadList.Add(_post);
                }
                else
                    CopyWhenReady = true;
            }
        }

        internal void ShowTags()
        {
            ShowTagList = (ShowTagList == Visibility.Collapsed) ? Visibility.Visible : Visibility.Collapsed;
        }

        internal void AddToFavorites()
        {
            if (ImageSource != null)
                _favoriteshandler.AddToFavorites(_post, ImageSource);
            else
                _favoriteWhenReady = true;

            if(AddedImageToFavorites != null)
                AddedImageToFavorites(_post, new EventArgs());
        }

        internal void RemoveFromFavorites()
        {
            _favoriteshandler.RemoveFromFavorites(_post);

            if (RemovedImageFromFavorites != null)
                RemovedImageFromFavorites(_post, new EventArgs());
        }
    }
}
