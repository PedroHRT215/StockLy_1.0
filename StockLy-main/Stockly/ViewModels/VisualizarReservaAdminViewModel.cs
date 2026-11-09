using Microsoft.Maui.Controls;
using Stockly.Models;
using Stockly.Services;
using Stockly.ViewModels;
using System.Collections.ObjectModel;

namespace Stockly.ViewModels
{
    public class VisualizarReservaAdminViewModel : BaseNotifyViewModel
    {
        private DatabaseService _dbService;

        private Usuario? _usuarioSelecionado;
        public Usuario? UsuarioSelecionado
        {
            get { return _usuarioSelecionado; }
            set
            {
                _usuarioSelecionado = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Historico> _listaReservas = new ObservableCollection<Historico>();
        public ObservableCollection<Historico> ListaReservas
        {
            get { return _listaReservas; }
            set
            {
                _listaReservas = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalMovimentacoes));
            }
        }

        public int TotalMovimentacoes
        {
            get { return ListaReservas == null ? 0 : ListaReservas.Count; }
        }

        public Command VoltarCommand { get; }

        public VisualizarReservaAdminViewModel(Usuario usuario)
        {
            _dbService = new DatabaseService();
            UsuarioSelecionado = usuario;

            VoltarCommand = new Command(async () =>
            {
                await Application.Current.MainPage.Navigation.PopAsync();
            });

            CarregarReservas();
        }

        private void CarregarReservas()
        {
            if (UsuarioSelecionado != null)
            {
                var historico = _dbService.GetHistoricoPorUsuarioId(UsuarioSelecionado.Id);
                ListaReservas = new ObservableCollection<Historico>(historico);
            }
            else
            {
                ListaReservas = new ObservableCollection<Historico>();
            }
        }
    }
}
