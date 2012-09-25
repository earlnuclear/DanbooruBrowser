﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using booruReader.Helpers;
using booruReader.Model;

namespace booruReader.Preview_Screen
{
    class PreviewScreenVM : INotifyPropertyChanged
    {
        private BasePost _post;
        private string _imageSource;
        private ObservableCollection<string> _taglist;
        private Visibility _showTaglist;
        private bool CopyWhenReady = false;

        public ObservableCollection<string> TagList { get { return _taglist; } }

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

        public PreviewScreenVM(BasePost post)
        {
            _post = post;

            ImageCache cache = new ImageCache();

            ImageSource = cache.GetImage(post.FileMD, post.FullPictureURL, LateFilePath);

            PreviewPost = post;
            //ImageSource = _post.FullPictureURL;
            ShowTagList = Visibility.Collapsed;
            char[] splitter = { ' ' };

            if (!string.IsNullOrEmpty(post.Tags))
                _taglist = new ObservableCollection<string>(post.Tags.Split(splitter, StringSplitOptions.RemoveEmptyEntries));
            else
                _taglist = new ObservableCollection<string>();
        }

        private void LateFilePath(object e, AsyncCompletedEventArgs args)
        {
            //Yeah, I know, should really pass the flippin thing as a argument.
            ImageCache cache = new ImageCache();

            ImageSource = cache.GetImage(_post.FileMD, _post.FullPictureURL, LateFilePath);

            if(CopyWhenReady)
                _post.SaveImage(ImageSource);
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        #endregion

        internal void Download()
        {
            if (string.IsNullOrEmpty(GlobalSettings.Instance.SavePath))
            {
                new MetroMessagebox("Error", "No save directory specified. \nPlease go to settings and select a folder.").ShowDialog();
            }
            else
            {
                if (ImageSource != null)
                    _post.SaveImage(ImageSource);
                else
                    CopyWhenReady = true;
            }
        }

        internal void ShowTags()
        {
            if(ShowTagList == Visibility.Collapsed)
                ShowTagList = Visibility.Visible;
            else
                ShowTagList = Visibility.Collapsed;
        }
    }
}
