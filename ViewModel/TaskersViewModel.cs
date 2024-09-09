using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Animations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasker099.Model;
using Tasker099.Services;

namespace Tasker099.ViewModel
{
    public partial class TaskersViewModel : BaseViewModel
    {
        TaskerService taskerService;

        //need a variable to store current Filter
        private string _currentFilter = "all";

        public ObservableCollection<Tasker> Taskers { get; } = new();

        public TaskersViewModel(TaskerService taskerService) {
            Title = "Task View";
            this.taskerService = taskerService;
        }

        // For navigating to DetailsPage
        [RelayCommand]
        async Task GoToDetailsAsync(Tasker tasker) {
            if (tasker is null)
                return;

            await Shell.Current.GoToAsync($"{nameof(DetailsPage)}", true,
                new Dictionary<string, object> {
                        {"Tasker", tasker}
                });
        }

        // For initialization of checkboxes
        async Task CheckImageSource(Tasker tasker) {
            if (tasker is null)
                return;

            if (tasker.Checked == true)
                tasker.ImgSource = "chk_on_50.png";
            else
                tasker.ImgSource = "chk_off_50.png";
        }

        // Initializing the Taskers
        [RelayCommand]
        public async Task GetTaskersAsync() {
            if (IsBusy)
                return;

            try {
                var taskers = await taskerService.UpdateDisplayText();
                if (_currentFilter != "all")
                {
                    taskers = await taskerService.FilterTaskers(_currentFilter, taskers);
                }
                var sortedTaskers = await taskerService.SortTaskers(taskers);

                // if Taskers is not empty, clear it
                if (Taskers.Count > 0)
                    Taskers.Clear();

                foreach (var tasker in sortedTaskers) {
                    await CheckImageSource(tasker);
                    Taskers.Add(tasker);
                }
            }
            catch (Exception ex) {
                Debug.WriteLine(ex);
                await Shell.Current.DisplayAlert("Error",
                    $"Unable to fetch Taskers", "OK");
            }
            finally {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task FilterByInterval()
        {
            if (IsBusy) return;

            try {
                _currentFilter = "interval";
                await GetTaskersAsync();
            }
            catch (Exception ex) {
                Debug.WriteLine(ex);
                await Shell.Current.DisplayAlert("Error",
                    $"Filtering Failed", "OK");
            }
        }

        [RelayCommand]
        public async Task FilterByDate()
        {
            if (IsBusy) return;

            try
            {
                _currentFilter = "date";
                await GetTaskersAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                await Shell.Current.DisplayAlert("Error",
                    $"Filtering Failed", "OK");
            }
        }

        [RelayCommand]
        public async Task FilterOff()
        {
            if (IsBusy) return;

            try
            {
                _currentFilter = "all";
                await GetTaskersAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                await Shell.Current.DisplayAlert("Error",
                    $"Filtering Failed", "OK");
            }
        }

        // For updating checkboxes
        public async void UpdateCheckStatus(DateTime a, Tasker tasker) {
            //Shell.Current.DisplayAlert("DateCheck", $"{a} & {tasker.Name}", "OK");
            if (tasker.Name is null)
                return;

            string chkTime = a.ToString("yyyy-MM-dd HH:mm:ss");
            if (tasker.Checked == true) { 
                tasker.Checked = false;
                tasker.ImgSource = "chk_off_50.png";
                chkTime = "None";
            }
            else { 
                tasker.Checked = true;
                tasker.ImgSource = "chk_on_50.png";
            }
            try {
                await TaskerService.UpdateTasker(tasker.Name, null, null, null, null, null, tasker.Checked, chkTime);
            }
            catch (Exception ex) {
                Debug.WriteLine(ex);
                await Shell.Current.DisplayAlert("Error",
                    $"Error", "OK");
            }
            finally {
                await GetTaskersAsync();
            }
        }

        [RelayCommand]
        async Task AddTasker() {
            await Shell.Current.GoToAsync($"{nameof(AddPage)}");
        }
    }
}
