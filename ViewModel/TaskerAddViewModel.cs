using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasker099.Model;
using Tasker099.Services;

namespace Tasker099.ViewModel
{
    public partial class TaskerAddViewModel : BaseViewModel
    {
        private readonly TaskerService _taskerService;

        [ObservableProperty]
        Tasker tasker;

        public TaskerAddViewModel(TaskerService taskerService)
        {
            _taskerService = taskerService;
            tasker = new Tasker();
            tasker.Date = "1990-10-10";
        }

        [RelayCommand]
        async Task Save()
        {
            if (tasker is null)
                return;
            //check nullorwhitespace for Name & Type.
            if (string.IsNullOrWhiteSpace(tasker.Name) || string.IsNullOrWhiteSpace(tasker.Type))
            {
                await Shell.Current.DisplayAlert("Error", "Name and Type are required", "Ok");
                return;
            }
            if (tasker.Date is null)
                tasker.Date = "1990-10-10";
            try
            {
                await _taskerService.AddTasker(tasker.Name, tasker.Type, tasker.Interval, tasker.IntervalReset, tasker.Date, tasker.ResetDay);
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                await Shell.Current.DisplayAlert("Error", "An error occurred while saving the tasker", "Ok");
            }
        }

        [RelayCommand]
        async Task Close() => await Shell.Current.GoToAsync("..");
    }
}
