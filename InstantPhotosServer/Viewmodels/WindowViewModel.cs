using System.Windows.Input;
using System.Windows;
using System.Windows.Controls;

namespace InstantPhotosServer
{
    /// <summary>
    /// View model for custom flat window
    /// </summary>
    class WindowViewModel : BaseViewModel
    {

        #region Private Members

        /// <summary>
        /// Reference to window this viewmodel controls
        /// </summary>
        private Window mWindow;

        /// <summary>
        /// Margin for window drop shadow
        /// </summary>
        private int mOuterMarginSize = 10;

        /// <summary>
        /// Window edges radius
        /// </summary>
        private int mWindowRadius = 10;

        #endregion

        #region Commands

        /// <summary>
        /// Hides the window
        /// </summary>
        public ICommand HideCommand { get; set; }

        /// <summary>
        /// Minimizes/maximizes window
        /// </summary>
        public ICommand MaximizeCommand { get; set; }


        /// <summary>
        /// Closes window
        /// </summary>
        public ICommand CloseCommand { get; set; }

        /// <summary>
        /// NavigationButton click command
        /// </summary>
        public ICommand SwitchPageCommand { get; set; }


        #endregion

        #region Public Properties

        /// <summary>
        /// Content page padding
        /// </summary>
        public Thickness InnerContentPadding { get { return new Thickness(ResizeBorder); } }

        /// <summary>
        /// Minimum allowed width of the window
        /// </summary>
        public double WindowMinimumWidth { get; set; } = 500;

        /// <summary>
        /// Minimum allowed height of the window
        /// </summary>
        public double WindowMinimumHeight { get; set; } = 400;

        /// <summary>
        /// Size of the resize border around the window in pixels
        /// </summary>
        public int ResizeBorder { get; set; } = 6;

        /// <summary>
        /// Border thickness for window
        /// </summary>
        public Thickness ResizeBorderThickness { get { return new Thickness(ResizeBorder + OuterMarginSize); } }

        /// <summary>
        /// OuterMarginSize property for Window which returns result depending on window state
        /// </summary>
        public int OuterMarginSize
        {
            get
            {
                return mWindow.WindowState == WindowState.Maximized ? 0 : mOuterMarginSize;
            }
            set
            {
                mOuterMarginSize = value;
            }
        }

        /// <summary>
        /// OuterMarginSize property for Window which returns result depending on window state
        /// </summary>
        public Thickness OuterMarginSizeThickness { get { return new Thickness(OuterMarginSize); } }

        /// <summary>
        /// Window edges radius
        /// </summary>
        public int WindowRadius
        {
            get
            {
                return mWindow.WindowState == WindowState.Maximized ? 0 : mWindowRadius;
            }
            set
            {
                mOuterMarginSize = value;
            }
        }

        /// <summary>
        /// Window edges radius
        /// </summary>
        public CornerRadius WindowCornerRadius { get { return new CornerRadius(WindowRadius); } }

        /// <summary>
        /// Height of the title bar/caption of the window
        /// </summary>
        public int TitleHeight { get; set; } = 42;

        public GridLength TitleHeightGridLength { get { return new GridLength(TitleHeight + ResizeBorder); } }

        #endregion


        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="window"></param>
        public WindowViewModel(Window window)
        {
            this.mWindow = window;
            mWindow.Content = new StatusPage();
            mWindow.MaxHeight = SystemParameters.WorkArea.Height;
            mWindow.MaxWidth = SystemParameters.WorkArea.Width; 
            //Create commands
            this.HideCommand = new RelayCommand<object>((obj) => mWindow.WindowState = WindowState.Minimized,() => true);
            this.MaximizeCommand = new RelayCommand<object>((obj) => mWindow.WindowState ^= WindowState.Maximized, () => true);
            this.CloseCommand = new RelayCommand<object>((obj) => mWindow.Close(), () => true);

            //Listening for window resizing
            mWindow.StateChanged += (sender, e) =>
            {
                //Firing event for all properties affected by resize
                OnPropertyChanged(nameof(ResizeBorderThickness));
                OnPropertyChanged(nameof(OuterMarginSize));
                OnPropertyChanged(nameof(OuterMarginSizeThickness));
                OnPropertyChanged(nameof(WindowRadius));
                OnPropertyChanged(nameof(WindowCornerRadius));
            };

            //fix window resize issue
            var resizer = new WindowResizer(window);
        }

        #endregion

    }
}
