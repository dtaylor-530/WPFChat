using ChatterClient;
using ChatterServer.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ChatterServer
{

    public class MainWindowViewModel : BaseViewModel
    {
        public string ExternalAddress
        {
            get => Model.Instance.String<ExternalAddressChange>();
            set => Model.Instance.AddChange(new ExternalAddressChange(value));
        }

        public string Port
        {
            get => Model.Instance.String<PortChange>();
            set => Model.Instance.AddChange(new ExternalAddressChange(value));
        }

        public string Status
        {
            get => Model.Instance.String<StatusChange>();
            set => Model.Instance.AddChange(new StatusChange(value));
        }

        public int ClientsConnected
        {
            get => Model.Instance.Int<ClientsConnectedChange>();
            set => Model.Instance.AddChange(new ClientsConnectedChange(value));
        }

        public ObservableCollection<string> Outputs { get; set; }

        public Dictionary<string, string> Usernames = new();

        public ICommand RunCommand { get; set; }
        public ICommand StopCommand { get; set; }


        public MainWindowViewModel()
        {
            Outputs = new ObservableCollection<string>();
            RunCommand = new AsyncCommand(() => Model.Instance.AddChange(new RunChange()));
            StopCommand = new AsyncCommand(() => Model.Instance.AddChange(new StopChange()));

            Model.Instance.Changes.CollectionChanged += (s, e) =>
            {
                foreach (var item in e.NewItems)
                {
                    switch (item)
                    {
                        case OutputChange outputChange:
                            Outputs.Add(outputChange.Value);
                            break;
                        case ExternalAddressChange:
                            OnPropertyChanged(nameof(ExternalAddress));
                            break;
                        case PortChange:
                            OnPropertyChanged(nameof(Port));
                            break;
                        case StatusChange:
                            OnPropertyChanged(nameof(Status));
                            break;
                        case ClientsConnectedChange:
                            OnPropertyChanged(nameof(ClientsConnected));
                            break;
                    }
                }
            };
        }


    }
}
