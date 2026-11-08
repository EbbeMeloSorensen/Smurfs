using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Smurfs.ViewModel;

namespace Smurfs.UI.WPF;

/// <summary>
/// Displays the Smurf list and owns its modal creation dialog.
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;

    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>
    /// Binds the list view model and supplies scopes for creation dialogs.
    /// </summary>
    public MainWindow(MainWindowViewModel viewModel, IServiceScopeFactory scopeFactory)
    {
        InitializeComponent();

        _viewModel = viewModel;
        _scopeFactory = scopeFactory;
        DataContext = viewModel;
    }

    private async void CreateSmurf(object sender, RoutedEventArgs e)
    {
        bool? created;
        await using (var scope = _scopeFactory.CreateAsyncScope())
        {
            var dialog = scope.ServiceProvider.GetRequiredService<CreateSmurfDialog>();
            dialog.Owner = this;
            created = dialog.ShowDialog();
        }

        if (created == true)
        {
            await _viewModel.RefreshAsync();
        }
    }
}
