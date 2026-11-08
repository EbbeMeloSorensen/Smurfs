using System.ComponentModel;
using System.Windows;
using Smurfs.ViewModel;

namespace Smurfs.UI.WPF;

/// <summary>
/// Hosts the name form and closes with a positive result after a successful save.
/// </summary>
public partial class CreateSmurfDialog : Window
{
    private readonly CreateSmurfDialogViewModel _viewModel;

    /// <summary>
    /// Binds the form to its scoped view model.
    /// </summary>
    public CreateSmurfDialog(CreateSmurfDialogViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        _viewModel.Saved += OnSaved;
    }

    /// <inheritdoc />
    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
        if (!_viewModel.CanEdit)
        {
            e.Cancel = true;
        }
    }

    private void OnSaved(object? sender, EventArgs e) => DialogResult = true;
}
