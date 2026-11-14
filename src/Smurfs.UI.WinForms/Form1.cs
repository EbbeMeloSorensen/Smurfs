using System.Windows.Input;
using Smurfs.ViewModel;

namespace Smurfs.UI.WinForms;

/// <summary>
/// Displays and navigates the shared Smurf catalog pages.
/// </summary>
public partial class Form1 : Form
{
    /// <summary>
    /// Creates the form for the designer.
    /// </summary>
    public Form1()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Connects the form to the shared paging state and commands.
    /// </summary>
    public Form1(MainWindowViewModel viewModel) : this()
    {
        BindCommand(loadButton, viewModel.LoadSmurfsCommand);
        BindCommand(previousButton, viewModel.PreviousPageCommand);
        BindCommand(nextButton, viewModel.NextPageCommand);
        pageStatus.DataBindings.Add(nameof(Label.Text), viewModel, nameof(viewModel.PageStatus));
        errorStatus.DataBindings.Add(nameof(Label.Text), viewModel, nameof(viewModel.ErrorMessage), true, DataSourceUpdateMode.Never, string.Empty);

        void UpdateRows(object? sender, EventArgs args)
        {
            smurfs.BeginUpdate();
            try
            {
                smurfs.Items.Clear();
                smurfs.Items.AddRange(viewModel.Smurfs.Select(smurf => smurf.Name).ToArray());
            }
            finally
            {
                smurfs.EndUpdate();
            }
        }

        viewModel.Smurfs.CollectionChanged += UpdateRows;
        Disposed += (_, _) => viewModel.Smurfs.CollectionChanged -= UpdateRows;
        UpdateRows(this, EventArgs.Empty);
    }

    private void BindCommand(Button button, ICommand command)
    {
        void UpdateEnabled(object? sender, EventArgs args) => button.Enabled = command.CanExecute(null);

        button.Click += (_, _) =>
        {
            if (command.CanExecute(null))
            {
                command.Execute(null);
            }
        };
        command.CanExecuteChanged += UpdateEnabled;
        Disposed += (_, _) => command.CanExecuteChanged -= UpdateEnabled;
        UpdateEnabled(this, EventArgs.Empty);
    }
}
