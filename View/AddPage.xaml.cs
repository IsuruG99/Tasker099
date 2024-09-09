using Tasker099.ViewModel;

namespace Tasker099;

public partial class AddPage : ContentPage
{
	public AddPage(TaskerAddViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}