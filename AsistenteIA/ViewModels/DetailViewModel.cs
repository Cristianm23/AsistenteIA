using System.Globalization;
using AsistenteIA.Models;
using AsistenteIA.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace AsistenteIA.ViewModels;

public partial class DetailViewModel : ObservableObject, IQueryAttributable
{
	private readonly DatabaseService _db;
	private ConversationRecord? _record;

	private string _prompt = string.Empty;
	private string _answer = string.Empty;
	private string _dateText = string.Empty;
	private string _category = string.Empty;
	private bool _isVerified;
	private bool _isBusy;
	private string _statusMessage = string.Empty;

	public DetailViewModel(DatabaseService db)
	{
		_db = db;
		SaveChangesCommand = new AsyncRelayCommand(SaveChangesAsync);
		DeleteCommand = new AsyncRelayCommand(DeleteAsync);
		GoBackCommand = new AsyncRelayCommand(GoBackAsync);
	}

	public string Prompt
	{
		get => _prompt;
		private set => SetProperty(ref _prompt, value);
	}

	public string Answer
	{
		get => _answer;
		private set => SetProperty(ref _answer, value);
	}

	public string DateText
	{
		get => _dateText;
		private set => SetProperty(ref _dateText, value);
	}

	public string Category
	{
		get => _category;
		private set => SetProperty(ref _category, value);
	}

	public bool IsVerified
	{
		get => _isVerified;
		set => SetProperty(ref _isVerified, value);
	}

	public bool IsBusy
	{
		get => _isBusy;
		private set => SetProperty(ref _isBusy, value);
	}

	public string StatusMessage
	{
		get => _statusMessage;
		private set => SetProperty(ref _statusMessage, value);
	}

	public IAsyncRelayCommand SaveChangesCommand { get; }
	public IAsyncRelayCommand DeleteCommand { get; }
	public IAsyncRelayCommand GoBackCommand { get; }

	public void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		if (!query.TryGetValue("id", out object? value) ||
			!int.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), out int id))
		{
			StatusMessage = "No se encontró el identificador de la consulta.";
			return;
		}

		_ = LoadAsync(id);
	}

	private async Task LoadAsync(int id)
	{
		if (IsBusy) return;

		IsBusy = true;
		StatusMessage = "Cargando consulta...";

		try
		{
			ConversationRecord? record = await _db.GetByIdAsync(id);
			if (record is null)
			{
				StatusMessage = "La consulta ya no existe en el historial.";
				return;
			}

			_record = record;
			Prompt = record.Prompt;
			Answer = record.Answer;
			DateText = record.CreatedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
			Category = record.Category;
			IsVerified = record.IsVerified;
			StatusMessage = string.Empty;
		}
		catch (Exception)
		{
			StatusMessage = "No se pudo cargar la consulta.";
		}
		finally
		{
			IsBusy = false;
		}
	}

	private async Task SaveChangesAsync()
	{
		if (_record is null) return;

		_record.IsVerified = IsVerified;
		await _db.UpdateAsync(_record);
		StatusMessage = "Cambios guardados.";
	}

	private async Task DeleteAsync()
	{
		if (_record is null) return;

		bool confirm = await Shell.Current.DisplayAlert(
			"Eliminar consulta",
			"¿Seguro que desea eliminar esta consulta del historial?",
			"Eliminar", "Cancelar");

		if (!confirm) return;

		await _db.DeleteAsync(_record);
		await GoBackAsync();
	}

	private static async Task GoBackAsync() => await Shell.Current.GoToAsync("..");
}
