using Craft.Domain;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MediatR;
using Smurfs.Application.Smurfs;
using Smurfs.Domain;

namespace Smurfs.ViewModel;

/// <summary>
/// Validates a proposed name and submits the create command from the dialog.
/// </summary>
public sealed class CreateSmurfDialogViewModel : ViewModelBase
{
    private readonly IBusinessRuleCatalog _catalog;

    private readonly ISender _sender;

    private string _name = string.Empty;

    private string? _nameError;

    private string? _saveError;

    private bool _isSaving;

    /// <summary>
    /// Raised after the create command succeeds.
    /// </summary>
    public event EventHandler? Saved;

    /// <summary>
    /// Gets or sets the draft name, including invalid or incomplete input.
    /// </summary>
    public string Name
    {
        get => _name;
        set
        {
            if (Set(ref _name, value))
            {
                SaveError = null;
                NameError = null;
            }
        }
    }

    /// <summary>
    /// Gets the first name violation from the latest save attempt.
    /// </summary>
    public string? NameError
    {
        get => _nameError;
        private set => Set(ref _nameError, value);
    }

    /// <summary>
    /// Gets the latest save failure, if any.
    /// </summary>
    public string? SaveError
    {
        get => _saveError;
        private set => Set(ref _saveError, value);
    }

    /// <summary>
    /// Gets whether the draft can be edited or the dialog dismissed.
    /// </summary>
    public bool CanEdit => !_isSaving;

    /// <summary>
    /// Validates and submits the draft while preventing overlapping saves.
    /// </summary>
    public RelayCommand SaveCommand { get; }

    /// <summary>
    /// Uses the scoped catalog and sender when the operator saves the draft.
    /// </summary>
    public CreateSmurfDialogViewModel(IBusinessRuleCatalog catalog, ISender sender)
    {
        _catalog = catalog;
        _sender = sender;
        SaveCommand = new RelayCommand(async () => await SaveAsync(), () => CanEdit);
    }

    private async Task SaveAsync()
    {
        if (!SaveCommand.CanExecute(null))
        {
            return;
        }

        _isSaving = true;
        RaisePropertyChanged(nameof(CanEdit));
        SaveCommand.RaiseCanExecuteChanged();
        SaveError = null;

        try
        {
            NameError = _catalog.ValidateAtomic(new SmurfNameDraft(Name)).FirstOrDefault()?.Message;
            if (NameError is not null)
            {
                return;
            }

            var result = await _sender.Send(new Create.Command { Name = Name });
            if (!result.IsSuccess)
            {
                SaveError = result.Error;
                return;
            }
        }
        catch (Exception exception)
        {
            SaveError = $"Could not create Smurf. {exception.Message}";
            return;
        }
        finally
        {
            _isSaving = false;
            RaisePropertyChanged(nameof(CanEdit));
            SaveCommand.RaiseCanExecuteChanged();
        }

        Saved?.Invoke(this, EventArgs.Empty);
    }
}
