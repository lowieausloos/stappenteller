using mobielecommunicatie.ViewModel;

namespace mobielecommunicatie
{
    public partial class MainPage : ContentPage
    {

        public MainPage(MainPageViewModel viewModel)
        {
            InitializeComponent();

            BindingContext = viewModel;
        }

    }
}