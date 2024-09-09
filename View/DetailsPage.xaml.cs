using Tasker099.ViewModel;

namespace Tasker099;

public partial class DetailsPage : ContentPage
{
	public DetailsPage(TaskerDetailsViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
    }

    private void OnTypeChanged(object sender, EventArgs e)
    {
        if (BindingContext is TaskerDetailsViewModel viewModel)
        {
            viewModel.UpdateVisibilityProperties(viewModel.EditableTasker.Type);
        }
    }
}