using System.Collections.ObjectModel;
using MediatR;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Smurfs.Application.Smurfs;

namespace Smurfs.ViewModel;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IMediator _mediator;

    public RelayCommand LoadSmurfsCommand { get; }

    public ObservableCollection<SmurfDto> Smurfs { get; } = new();

    public MainWindowViewModel(IMediator mediator)
    {
        _mediator = mediator;

        LoadSmurfsCommand = new RelayCommand(async void () => await LoadSmurfsAsync());
    }

    private async Task LoadSmurfsAsync()
    {
        var smurfs = await _mediator.Send(new List.Query { Params = new SmurfParams() });

        Smurfs.Clear();
        foreach (var smurfDto in smurfs.Value)
        {
            Smurfs.Add(smurfDto);
        }
    }
}
