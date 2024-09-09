using System.Diagnostics;
using Tasker099.Model;
using Tasker099.ViewModel;

namespace Tasker099
{
    public partial class MainPage : ContentPage
    {
        private readonly TaskersViewModel _viewModel;

        public MainPage(TaskersViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            BindingContext = viewModel;
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.GetTaskersAsync();
        }
        private void OnCheckChanged(object sender, EventArgs e)
        {
            var checkBox = (sender as Grid)?.FindByName<CheckBox>("checkBoxName");
            if (checkBox.BindingContext is Tasker tasker)
            {
                if (BindingContext is TaskersViewModel viewModel)
                {
                    viewModel.UpdateCheckStatus(DateTime.Now, tasker);
                }
            }
        }
    }
}
