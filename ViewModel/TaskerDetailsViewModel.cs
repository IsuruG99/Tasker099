using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasker099.Model;
using Tasker099.Services;

namespace Tasker099.ViewModel;

[QueryProperty("Tasker", "Tasker")]
public partial class TaskerDetailsViewModel : BaseViewModel
{

    private readonly TaskerService _taskerService;

    public TaskerDetailsViewModel(TaskerService taskerService)
    {
        _taskerService = taskerService;
        if (Tasker != null)
        {
            Tasker.CheckedTime = Tasker.CheckedTime.ToString();
            CheckTypeCommand.Execute(Tasker);
        }
    }

    public TaskerDetailsViewModel() { }

    [ObservableProperty]
    Tasker tasker;

    [ObservableProperty]
    Tasker editableTasker;

    [ObservableProperty]
    bool isDatePickerVisible;

    [ObservableProperty]
    bool isIntervalVisible;

    [ObservableProperty]
    bool isIntervalResetVisible;

    [ObservableProperty]
    bool isResetDayVisible;

    // Binding OnTypeChanged when Type Picker's index changes.

    [RelayCommand]
    async Task Close() => await Shell.Current.GoToAsync("..");

    partial void OnTaskerChanged(Tasker value)
    {
        if (value != null)
        {
            EditableTasker = new Tasker
            {
                Name = value.Name,
                Type = value.Type,
                Interval = value.Interval,
                IntervalReset = value.IntervalReset,
                Date = value.Date,
                ResetDay = value.ResetDay,
                DisplayText = value.DisplayText,
                CheckedTime = value.CheckedTime
            };
            CheckTypeCommand.Execute(EditableTasker);
        }
    }

    /* check task type & update visibility properties */
    [RelayCommand]
    async Task CheckType(Tasker tasker)
    {
        if (tasker.Type is null)
            return;

        try
        {
            UpdateVisibilityProperties(tasker.Type);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            await Shell.Current.DisplayAlert("Error",
                $"Unable to check Task Type", "OK");
        }
    }

    [RelayCommand]
    async Task Submit()
    {
        try
        {
            Tasker.Type = EditableTasker.Type;

            if (EditableTasker.Type != "date") 
            { 
                Tasker.Date = "1990-10-10";
                Tasker.Interval = EditableTasker.Interval;
                Tasker.IntervalReset = EditableTasker.IntervalReset;
                Tasker.ResetDay = EditableTasker.ResetDay;
            }
            else
            {
                Tasker.Date = EditableTasker.Date;
                Tasker.Interval = "None";
                Tasker.IntervalReset = "None";
                Tasker.ResetDay = "None";
            }

            await TaskerService.UpdateTasker(
                name: Tasker.Name,
                type: Tasker.Type,
                interval: Tasker.Interval,
                intervalReset: Tasker.IntervalReset,
                date: Tasker.Date,
                resetDay: Tasker.ResetDay,
                checkedStatus: Tasker.Checked,
                checkedTime: Tasker.CheckedTime
            );
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            await Shell.Current.DisplayAlert("Error",
                $"Failed to update Tasker", "OK");
        }
        finally 
        {
            await Shell.Current.GoToAsync("..");
        }
        
    }

    [RelayCommand]
    async Task Delete()
    {
        if (Tasker.Name is null)
            return;
        bool isConfirmed = await Shell.Current.DisplayAlert("Delete", $"Confirm to delete {Tasker.Name}?", "OK", "Cancel");
        if (isConfirmed)
        {
            try
            {
                await _taskerService.DeleteTasker(Tasker.Name);
                await Shell.Current.DisplayAlert("Success", $"{Tasker.Name} has been deleted.", "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                await Shell.Current.DisplayAlert("Error", $"Failed to delete {Tasker.Name}.", "OK");
            }
            finally
            {
                await Shell.Current.GoToAsync("..");
            }
        }
    }

    partial void OnEditableTaskerChanged(Tasker value)
    {
        if (value != null)
        {
            UpdateVisibilityProperties(value.Type);
        }
    }

    public void UpdateVisibilityProperties(string type)
    {
        switch (type)
        {
            case "date":
                IsDatePickerVisible = true;
                IsIntervalVisible = false;
                IsIntervalResetVisible = false;
                IsResetDayVisible = false;
                break;
            case "interval":
                IsDatePickerVisible = false;
                IsIntervalVisible = true;
                IsIntervalResetVisible = true;
                IsResetDayVisible = true;
                break;
            default:
                IsDatePickerVisible = false;
                IsIntervalVisible = false;
                IsIntervalResetVisible = false;
                IsResetDayVisible = false;
                break;
        }
    }
}
