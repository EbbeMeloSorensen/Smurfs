using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Smurfs.Application.Smurfs;

namespace Smurfs.ViewModel;

/// <summary>
/// Loads and navigates pages of Smurfs and reports load failures to the view.
/// </summary>
public class MainWindowViewModel : ViewModelBase
{
    private readonly IServiceScopeFactory _scopeFactory;

    private bool _isLoading;

    private int _currentPage;

    private int _totalPages;

    private string _pageStatus = "No page loaded";

    private string? _errorMessage;

    /// <summary>
    /// Loads the first page when no other load is running.
    /// </summary>
    public RelayCommand LoadSmurfsCommand { get; }

    /// <summary>
    /// Loads the preceding page when one is available.
    /// </summary>
    public RelayCommand PreviousPageCommand { get; }

    /// <summary>
    /// Loads the following page when one is available.
    /// </summary>
    public RelayCommand NextPageCommand { get; }

    /// <summary>
    /// Describes the displayed page and the total number of pages.
    /// </summary>
    public string PageStatus
    {
        get => _pageStatus;
        private set => Set(ref _pageStatus, value);
    }

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
        LoadSmurfsCommand = new RelayCommand(async () => await LoadSmurfsAsync(1), () => !_isLoading);
        PreviousPageCommand = new RelayCommand(
            async () => await LoadSmurfsAsync(_currentPage - 1),
            () => !_isLoading && _currentPage > 1);
        NextPageCommand = new RelayCommand(
            async () => await LoadSmurfsAsync(_currentPage + 1),
            () => !_isLoading && _currentPage < _totalPages);
    }

    private async Task LoadSmurfsAsync(int pageNumber)
    {
        if (_isLoading)
        {
            return;
        }

        _isLoading = true;
        try
        {
            RefreshCommands();
            ErrorMessage = null;

            await using var scope = _scopeFactory.CreateAsyncScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var result = await sender.Send(new List.Query
            {
                Params = new SmurfParams { PageNumber = pageNumber }
            });
            if (!result.IsSuccess)
            {
                ErrorMessage = result.Error;
                return;
            }

            var page = result.Value;
            if (page.TotalPages > 0 && page.CurrentPage > page.TotalPages)
            {
                ErrorMessage = "That page is no longer available. Select Load smurfs to start again.";
                return;
            }

            _totalPages = page.TotalPages;
            _currentPage = _totalPages == 0 ? 0 : page.CurrentPage;
            PageStatus = _totalPages == 0 ? "No pages" : $"Page {_currentPage} of {_totalPages}";

            Smurfs.Clear();
            foreach (var smurf in page)
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
            RefreshCommands();
        }
    }

    private void RefreshCommands()
    {
        LoadSmurfsCommand.RaiseCanExecuteChanged();
        PreviousPageCommand.RaiseCanExecuteChanged();
        NextPageCommand.RaiseCanExecuteChanged();
    }
}
