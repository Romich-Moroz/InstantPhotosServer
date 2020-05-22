using System.Windows.Controls;

namespace InstantPhotosServer
{
    /// <summary>
    /// Interaction logic for StatusPage.xaml
    /// </summary>
    public partial class StatusPage : Page
    {
        public StatusPage()
        {
            InitializeComponent();
            this.DataContext = ViewModel.GetInstance();
        }
    }
}
