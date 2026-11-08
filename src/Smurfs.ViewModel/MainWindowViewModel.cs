using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Smurfs.Application.Smurfs;

namespace Smurfs.ViewModel;

/// <summary>
/// Loads the first page of Smurfs and reports load failures to the view.
/// </summary>
public class MainWindowViewModel : ViewModelBase
{
    private readonly IServiceScopeFactory _scopeFactory;

    private bool _isLoading;

    private string? _errorMessage;

    /// <summary>
    /// Loads the first page when no other load is running.
    /// </summary>
    public RelayCommand LoadSmurfsCommand { get; }

    /// <summary>
    /// Contains the rows from the most recent successful load.
    /// </summary>
    public ObservableCollection<SmurfDto> Smurfs { get; } = new();

    /// <summary>
    /// Gets the latest load error, or null when no error is displayed.
    /// </summary>
    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => Set(ref _errorMessage, value);
    }

    /// <summary>
    /// Uses the scope factory to resolve a fresh sender for each load.
    /// </summary>
    public MainWindowViewModel(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        LoadSmurfsCommand = new RelayCommand(async () => await LoadSmurfsAsync(), () => !_isLoading);
    }

    private async Task LoadSmurfsAsync()
    {
        if (_isLoading)
        {
            return;
        }

        _isLoading = true;
        try
        {
            LoadSmurfsCommand.RaiseCanExecuteChanged();
            ErrorMessage = null;

            await using var scope = _scopeFactory.CreateAsyncScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var result = await sender.Send(new List.Query());
            if (!result.IsSuccess)
            {
                ErrorMessage = result.Error;
                return;
            }

            Smurfs.Clear();
            foreach (var smurf in result.Value)
            {
                Smurfs.Add(smurf);
            }
        }
        catch (Exception exception)
        {
            ErrorMessage = $"Could not load Smurfs. {exception.Message}";
        }
        finally
        {
            _isLoading = false;
            LoadSmurfsCommand.RaiseCanExecuteChanged();
        }
    }
}
