using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using BezyFB_UWP.Annotations;

namespace BezyFB_UWP
{
    public class ProgressBarDC : INotifyPropertyChanged
    {
        private static readonly Lazy<ProgressBarDC> _current = new Lazy<ProgressBarDC>();

        public static ProgressBarDC Current => _current.Value;

        private bool _isProgress;

        public bool IsProgress
        {
            get { return _isProgress; }
            set
            {
                _isProgress = value; 
                OnPropertyChanged(); 
                // ReSharper disable once ExplicitCallerInfoArgument
                OnPropertyChanged(nameof(Visibility));
            }
        }

        public Visibility Visibility => IsProgress ? Visibility.Visible : Visibility.Collapsed;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}