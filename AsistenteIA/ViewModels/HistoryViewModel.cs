using System.Collections.ObjectModel;
using AsistenteIA.Models;
using AsistenteIA.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AsistenteIA.ViewModels;

public partial class HistoryViewModel : ObservableObject
{
    private readonly DatabaseService _db;

    public ObservableCollection<ConversationRecord> Records { get; } = new();

    [ObservableProperty] private string searchText = string.Empty;

    public HistoryViewModel(DatabaseService db) => _db = db;

    // Carga todo el historial, o filtra si hay texto de búsqueda
    [RelayCommand]
    private async Task LoadAsync()
    {
        string text = SearchText.Trim();
        List<ConversationRecord> items = string.IsNullOrEmpty(text)
            ? await _db.GetAllAsync()
            : await _db.SearchAsync(text);

        Records.Clear();
        foreach (var item in items)
            Records.Add(item);
    }

    // Búsqueda en vivo mientras el usuario escribe
    partial void OnSearchTextChanged(string value) => _ = LoadAsync();

    [RelayCommand]
    private async Task DeleteAsync(ConversationRecord record)
    {
        bool confirm = await Shell.Current.DisplayAlert(
            "Eliminar consulta",
            "¿Seguro que desea eliminar esta consulta del historial?",
            "Eliminar", "Cancelar");

        if (!confirm) return;

        await _db.DeleteAsync(record);
        Records.Remove(record);
    }

    [RelayCommand]
    private async Task OpenDetailAsync(ConversationRecord record) =>
        await Shell.Current.GoToAsync($"detalle?id={record.Id}");
}