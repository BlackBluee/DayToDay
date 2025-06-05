using System.Collections.ObjectModel;
using System.Windows.Input;
using daytoday.Core.DTOs;
using daytoday.Services;
using Microsoft.Maui.Controls;

namespace daytoday.Views;

public partial class CalendarPage : ContentPage
{
    private readonly CalendarEventService _calendarEventService;
    private ObservableCollection<CalendarEventDto> _events;
    private DateTime _currentDate;
    private bool _isRefreshing;

    public ObservableCollection<CalendarEventDto> Events
    {
        get => _events;
        set { _events = value; OnPropertyChanged(); }
    }

    public string CurrentMonthYear => _currentDate.ToString("MMMM yyyy");

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set
        {
            _isRefreshing = value;
            OnPropertyChanged();
        }
    }

    public ICommand AddCommand { get; private set; }
    public ICommand EditCommand { get; private set; }
    public ICommand DeleteCommand { get; private set; }
    public ICommand RefreshCommand { get; private set; }
    public ICommand NextMonthCommand { get; private set; }
    public ICommand PreviousMonthCommand { get; private set; }

    public CalendarPage(CalendarEventService calendarEventService)
    {
        InitializeComponent();
        _calendarEventService = calendarEventService;
        _currentDate = DateTime.Today;

        Events = new ObservableCollection<CalendarEventDto>();

        AddCommand = new Command(OnAddEvent);
        EditCommand = new Command<CalendarEventDto>(OnEditEvent);
        DeleteCommand = new Command<CalendarEventDto>(OnDeleteEvent);
        RefreshCommand = new Command(async () => await LoadEventsForMonthAsync());
        NextMonthCommand = new Command(OnNextMonth);
        PreviousMonthCommand = new Command(OnPreviousMonth);

        BindingContext = this;

        LoadEventsForMonthAsync();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadEventsForMonthAsync();
    }

    private async Task LoadEventsForMonthAsync()
    {
        try
        {
            IsRefreshing = true;

            var allEvents = await _calendarEventService.GetCalendarEventsAsync();
            var firstDayOfMonth = new DateTime(_currentDate.Year, _currentDate.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            var monthEvents = allEvents
                .Where(e => (e.StartDate.Date >= firstDayOfMonth && e.StartDate.Date <= lastDayOfMonth) ||
                            (e.EndDate.Date >= firstDayOfMonth && e.EndDate.Date <= lastDayOfMonth))
                .OrderBy(e => e.StartDate)
                .ToList();

            Events.Clear();
            foreach (var ev in monthEvents)
            {
                Events.Add(ev);
            }

            OnPropertyChanged(nameof(CurrentMonthYear));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Błąd", $"Nie udało się załadować wydarzeń: {ex.Message}", "OK");
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private void OnEventSelected(object sender, SelectionChangedEventArgs e)
    {
        if (sender is CollectionView collectionView)
        {
            collectionView.SelectedItem = null;
        }
    }

    private async void OnAddEvent()
    {
        try
        {
            string name = await DisplayPromptAsync("Nowe wydarzenie", "Nazwa wydarzenia:", maxLength: 50);
            if (string.IsNullOrWhiteSpace(name))
                return;

            var startDate = _currentDate.Date.AddHours(9); 
            var endDate = startDate.AddHours(1); 

            var newEvent = new CalendarEventDto
            {
                Name = name.Trim(),
                StartDate = startDate,
                EndDate = endDate,
                Description = string.Empty,
                Location = string.Empty,
                IsAllDay = false
            };

            await _calendarEventService.CreateCalendarEventAsync(newEvent);
            await LoadEventsForMonthAsync();
            await DisplayAlert("Sukces", "Wydarzenie zostało utworzone pomyślnie", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Błąd", $"Nie udało się utworzyć wydarzenia: {ex.Message}", "OK");
        }
    }

    private async void OnEditEvent(CalendarEventDto calendarEvent)
    {
        if (calendarEvent == null)
            return;

        try
        {
            string name = await DisplayPromptAsync("Edytuj wydarzenie", "Nazwa wydarzenia:",
                initialValue: calendarEvent.Name, maxLength: 50);
            if (string.IsNullOrWhiteSpace(name))
                return;

            string description = await DisplayPromptAsync("Opis", "Opis wydarzenia (opcjonalnie):",
                initialValue: calendarEvent.Description ?? string.Empty, maxLength: 200);

            calendarEvent.Name = name.Trim();
            calendarEvent.Description = description?.Trim() ?? string.Empty;

            await _calendarEventService.UpdateCalendarEventAsync(calendarEvent);
            await LoadEventsForMonthAsync();
            await DisplayAlert("Sukces", "Wydarzenie zostało zaktualizowane pomyślnie", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Błąd", $"Nie udało się zaktualizować wydarzenia: {ex.Message}", "OK");
        }
    }

    private async void OnDeleteEvent(CalendarEventDto calendarEvent)
    {
        if (calendarEvent == null)
            return;

        bool confirm = await DisplayAlert("Potwierdź usunięcie",
            $"Czy na pewno chcesz usunąć wydarzenie \"{calendarEvent.Name}\"?", "Usuń", "Anuluj");

        if (!confirm)
            return;

        try
        {
            await _calendarEventService.DeleteCalendarEventAsync(calendarEvent.Id);
            Events.Remove(calendarEvent);
            await DisplayAlert("Sukces", "Wydarzenie zostało usunięte", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Błąd", $"Nie udało się usunąć wydarzenia: {ex.Message}", "OK");
        }
    }

    private async void OnNextMonth()
    {
        _currentDate = _currentDate.AddMonths(1);
        OnPropertyChanged(nameof(CurrentMonthYear));
        await LoadEventsForMonthAsync();
    }

    private async void OnPreviousMonth()
    {
        _currentDate = _currentDate.AddMonths(-1);
        OnPropertyChanged(nameof(CurrentMonthYear));
        await LoadEventsForMonthAsync();
    }
}
