using System.Collections.ObjectModel;
using System.Globalization;
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
    private CalendarEventDto _currentEvent;
    private Dictionary<DateTime, List<CalendarEventDto>> _eventsByDate;
    private List<Border> _dayBorders;

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
        _dayBorders = new List<Border>();
        _eventsByDate = new Dictionary<DateTime, List<CalendarEventDto>>();

        Events = new ObservableCollection<CalendarEventDto>();

        AddCommand = new Command(async () => await ShowEventForm(null));
        EditCommand = new Command<CalendarEventDto>(async (calEvent) => await ShowEventForm(calEvent));
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

            Events = new ObservableCollection<CalendarEventDto>(monthEvents);
            OnPropertyChanged(nameof(CurrentMonthYear));

            _eventsByDate = new Dictionary<DateTime, List<CalendarEventDto>>();
            foreach (var ev in monthEvents)
            {
                var eventDate = ev.StartDate.Date;
                if (!_eventsByDate.ContainsKey(eventDate))
                {
                    _eventsByDate[eventDate] = new List<CalendarEventDto>();
                }
                _eventsByDate[eventDate].Add(ev);
            }

            GenerateCalendarGrid();
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

    private void GenerateCalendarGrid()
    {
        CalendarGrid.Children.Clear();
        _dayBorders.Clear();

        CalendarGrid.RowDefinitions.Clear();
        CalendarGrid.ColumnDefinitions.Clear();

        for (int i = 0; i < 6; i++)
            CalendarGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) });

        for (int i = 0; i < 7; i++)
            CalendarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });


        var firstDayOfMonth = new DateTime(_currentDate.Year, _currentDate.Month, 1);

        int firstDayOfWeek = ((int)firstDayOfMonth.DayOfWeek + 6) % 7;

        int daysInMonth = DateTime.DaysInMonth(_currentDate.Year, _currentDate.Month);

        var today = DateTime.Today;

        int day = 1;
        for (int row = 0; row < 6; row++)
        {
            for (int col = 0; col < 7; col++)
            {
                if ((row == 0 && col < firstDayOfWeek) || day > daysInMonth)
                {
                    continue;
                }

                var currentDate = new DateTime(_currentDate.Year, _currentDate.Month, day);

                var dayBorder = new Border
                {
                    Style = currentDate.Date == today.Date
                        ? (Style)Resources["TodayStyle"]
                        : (Style)Resources["CalendarDayStyle"]
                };

                var dayLabel = new Label
                {
                    Text = day.ToString(),
                    Style = (Style)Resources["DayNumberStyle"],
                    TextColor = currentDate.Date == today.Date
                        ? Colors.White
                        : (col == 6 ? Colors.Red : (Color)Application.Current.Resources["PrimaryDarkText"])
                };

                dayBorder.Content = new VerticalStackLayout
                {
                    Children = { dayLabel }
                };

                if (_eventsByDate.ContainsKey(currentDate))
                {
                    var eventMarker = new BoxView
                    {
                        Style = (Style)Resources["EventMarkerStyle"]
                    };
                    ((VerticalStackLayout)dayBorder.Content).Children.Add(eventMarker);

                    var tapGesture = new TapGestureRecognizer();
                    tapGesture.Tapped += (s, e) => ShowEventsForDay(currentDate);
                    dayBorder.GestureRecognizers.Add(tapGesture);
                }

                Grid.SetRow(dayBorder, row);
                Grid.SetColumn(dayBorder, col);
                CalendarGrid.Children.Add(dayBorder);
                _dayBorders.Add(dayBorder);

                day++;
            }
        }
    }

    private void ShowEventsForDay(DateTime date)
    {
        if (_eventsByDate.TryGetValue(date, out var dayEvents))
        {
            Events = new ObservableCollection<CalendarEventDto>(dayEvents);
        }
    }

    public void OnEventSelected(object sender, SelectionChangedEventArgs e)
    {
        if (sender is CollectionView collectionView)
        {
            collectionView.SelectedItem = null;
        }
    }

    private async Task ShowEventForm(CalendarEventDto calendarEvent)
    {
        _currentEvent = calendarEvent ?? new CalendarEventDto
        {
            Name = string.Empty,
            Description = string.Empty,
            Location = string.Empty,
            IsAllDay = false,
            StartDate = _currentDate.Date.AddHours(9),
            EndDate = _currentDate.Date.AddHours(10)
        };

        bool isNew = calendarEvent == null;

        var nameEntry = new Entry
        {
            Text = _currentEvent.Name,
            Placeholder = "Nazwa wydarzenia",
            Style = (Style)Resources["FormEntry"]
        };

        var descriptionEditor = new Editor
        {
            Text = _currentEvent.Description,
            Placeholder = "Opis wydarzenia (opcjonalnie)",
            Style = (Style)Resources["FormEditor"]
        };

        var locationEntry = new Entry
        {
            Text = _currentEvent.Location,
            Placeholder = "Lokalizacja (opcjonalnie)",
            Style = (Style)Resources["FormEntry"]
        };

        var startDatePicker = new DatePicker
        {
            Date = _currentEvent.StartDate.Date,
            Margin = new Thickness(0, 0, 0, 10)
        };

        var startTimePicker = new TimePicker
        {
            Time = _currentEvent.StartDate.TimeOfDay,
            Margin = new Thickness(0, 0, 0, 15)
        };

        var endDatePicker = new DatePicker
        {
            Date = _currentEvent.EndDate.Date,
            Margin = new Thickness(0, 0, 0, 10)
        };

        var endTimePicker = new TimePicker
        {
            Time = _currentEvent.EndDate.TimeOfDay,
            Margin = new Thickness(0, 0, 0, 15)
        };

        var isAllDayCheckBox = new CheckBox
        {
            IsChecked = _currentEvent.IsAllDay,
            Color = (Color)Application.Current.Resources["Primary"]
        };

        var isAllDayLabel = new Label
        {
            Text = "Całodniowe wydarzenie",
            VerticalOptions = LayoutOptions.Center
        };

        var form = new StackLayout
        {
            Padding = new Thickness(20),
            Children =
            {
                new Label { Text = "Nazwa wydarzenia", Style = (Style)Resources["FormLabel"] },
                nameEntry,

                new Label { Text = "Opis", Style = (Style)Resources["FormLabel"] },
                descriptionEditor,

                new Label { Text = "Lokalizacja", Style = (Style)Resources["FormLabel"] },
                locationEntry,

                new Label { Text = "Data i czas rozpoczęcia", Style = (Style)Resources["FormLabel"] },
                startDatePicker,
                startTimePicker,

                new Label { Text = "Data i czas zakończenia", Style = (Style)Resources["FormLabel"] },
                endDatePicker,
                endTimePicker,

                new HorizontalStackLayout
                {
                    Children = { isAllDayCheckBox, isAllDayLabel },
                    Spacing = 10,
                    Margin = new Thickness(0, 10, 0, 15)
                }
            }
        };

        isAllDayCheckBox.CheckedChanged += (s, e) =>
        {
            bool isChecked = ((CheckBox)s).IsChecked;
            startTimePicker.IsVisible = !isChecked;
            endTimePicker.IsVisible = !isChecked;

            if (isChecked)
            {
                startTimePicker.Time = new TimeSpan(0, 0, 0);
                endTimePicker.Time = new TimeSpan(23, 59, 59);
            }
        };

        var scrollView = new ScrollView { Content = form };

        var page = new ContentPage
        {
            Title = isNew ? "Nowe wydarzenie" : "Edytuj wydarzenie",
            Content = scrollView
        };

        page.ToolbarItems.Add(new ToolbarItem
        {
            Text = "Anuluj",
            Command = new Command(async () => await Navigation.PopModalAsync())
        });

        page.ToolbarItems.Add(new ToolbarItem
        {
            Text = "Zapisz",
            Command = new Command(async () =>
            {
                if (string.IsNullOrWhiteSpace(nameEntry.Text))
                {
                    await DisplayAlert("Błąd", "Nazwa wydarzenia nie może być pusta", "OK");
                    return;
                }

                var startDateTime = startDatePicker.Date.Add(startTimePicker.Time);
                var endDateTime = endDatePicker.Date.Add(endTimePicker.Time);

                if (endDateTime < startDateTime)
                {
                    await DisplayAlert("Błąd", "Data zakończenia nie może być wcześniejsza niż data rozpoczęcia", "OK");
                    return;
                }

                _currentEvent.Name = nameEntry.Text.Trim();
                _currentEvent.Description = descriptionEditor.Text?.Trim() ?? string.Empty;
                _currentEvent.Location = locationEntry.Text?.Trim() ?? string.Empty;
                _currentEvent.StartDate = startDateTime;
                _currentEvent.EndDate = endDateTime;
                _currentEvent.IsAllDay = isAllDayCheckBox.IsChecked;

                try
                {
                    IsRefreshing = true;

                    if (isNew)
                    {
                        await _calendarEventService.CreateCalendarEventAsync(_currentEvent);
                    }
                    else
                    {
                        await _calendarEventService.UpdateCalendarEventAsync(_currentEvent);
                    }

                    await Navigation.PopModalAsync();
                    await LoadEventsForMonthAsync();
                    await DisplayAlert("Sukces", $"Wydarzenie zostało {(isNew ? "utworzone" : "zaktualizowane")} pomyślnie", "OK");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Błąd", $"Nie udało się {(isNew ? "utworzyć" : "zaktualizować")} wydarzenia: {ex.Message}", "OK");
                }
                finally
                {
                    IsRefreshing = false;
                }
            })
        });

        await Navigation.PushModalAsync(new NavigationPage(page));
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
            IsRefreshing = true;
            await _calendarEventService.DeleteCalendarEventAsync(calendarEvent.Id);
            Events.Remove(calendarEvent);
            await LoadEventsForMonthAsync();
            await DisplayAlert("Sukces", "Wydarzenie zostało usunięte", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Błąd", $"Nie udało się usunąć wydarzenia: {ex.Message}", "OK");
        }
        finally
        {
            IsRefreshing = false;
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
    public Color GetEventColor(object parameters)
    {
        if (parameters is not Binding[] bindings || bindings.Length < 2)
            return Colors.Gray;

        bool isAllDay = bindings[0].Source is bool b ? b : false;
        DateTime startDate = bindings[1].Source is DateTime dt ? dt : DateTime.Now;

        if (isAllDay)
        {
            return Color.Parse("#4CAF50"); 
        }

        var today = DateTime.Today;
        var daysDifference = (startDate.Date - today).Days;

        return daysDifference switch
        {
            < 0 => Color.Parse("#9E9E9E"),  
            0 => Color.Parse("#2196F3"),     
            < 7 => Color.Parse("#FF9800"),   
            _ => Color.Parse("#512BD4")      
        };
    }

    public string GetEventEmoji(object parameters)
    {
        if (parameters is not Binding[] bindings || bindings.Length < 2)
            return "📅";

        bool isAllDay = bindings[0].Source is bool b ? b : false;
        DateTime startDate = bindings[1].Source is DateTime dt ? dt : DateTime.Now;

        if (isAllDay)
        {
            return "📅";
        }

        var today = DateTime.Today;
        var daysDifference = (startDate.Date - today).Days;

        return daysDifference switch
        {
            < 0 => "⏮️",    
            0 => "📍",       
            < 7 => "🔜",     
            _ => "🗓️"        
        };
    }
}
