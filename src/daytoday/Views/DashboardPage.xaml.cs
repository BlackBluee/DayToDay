using System.Collections.ObjectModel;
using System.Globalization;
using daytoday.Core.DTOs;
using daytoday.Services;

namespace daytoday.Views;

public partial class DashboardPage : ContentPage
{
    private readonly TaskService _taskService;
    private readonly ProjectService _projectService;
    private readonly CalendarEventService _calendarEventService;

    private bool _isRefreshing;
    private bool _isBusy;

    private ObservableCollection<TaskDto> _recentTasks;
    private ObservableCollection<CalendarEventDto> _upcomingEvents;

    public int CompletedTasksCount { get; private set; }
    public int InProgressTasksCount { get; private set; } 
    public int ActiveTasksCount { get; private set; }
    public int ProjectsCount { get; private set; }

    public static readonly Func<object, object> StatusEmojiConverter =
        (obj) => obj is TaskDto task ? GetStatusEmoji(task.Status) : "🆕";

    public static readonly Func<object, object> PriorityColorConverter =
        (obj) => obj is TaskDto task ? GetPriorityColor(task.Priority) : Colors.Gray;


    public static readonly Func<object, object> EventColorConverter =
        (obj) => obj is CalendarEventDto ev ? GetEventColor(ev.IsAllDay, ev.StartDate) : Colors.Gray;

    public static readonly Func<object, bool> StringToBoolConverter =
        (obj) => obj is string str && !string.IsNullOrWhiteSpace(str);

    public ObservableCollection<TaskDto> RecentTasks
    {
        get => _recentTasks;
        set
        {
            _recentTasks = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<CalendarEventDto> UpcomingEvents
    {
        get => _upcomingEvents;
        set
        {
            _upcomingEvents = value;
            OnPropertyChanged();
        }
    }

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set
        {
            _isRefreshing = value;
            OnPropertyChanged();
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            _isBusy = value;
            OnPropertyChanged();
        }
    }

    public DashboardPage(TaskService taskService, ProjectService projectService, CalendarEventService calendarEventService)
    {
        InitializeComponent();
        _taskService = taskService;
        _projectService = projectService;
        _calendarEventService = calendarEventService;

        _recentTasks = new ObservableCollection<TaskDto>();
        _upcomingEvents = new ObservableCollection<CalendarEventDto>();

        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDashboardDataAsync();
    }

    private async Task LoadDashboardDataAsync()
    {
        try
        {
            IsBusy = true;

            var projects = await _projectService.GetProjectsAsync();
            ProjectsCount = projects.Count;
            OnPropertyChanged(nameof(ProjectsCount));

            var tasks = await _taskService.GetTasksAsync(Guid.Empty);

            CompletedTasksCount = tasks.Count(t => t.Status?.Equals("Completed", StringComparison.OrdinalIgnoreCase) ?? false);
            OnPropertyChanged(nameof(CompletedTasksCount));

            InProgressTasksCount = tasks.Count(t => t.Status?.Equals("In Progress", StringComparison.OrdinalIgnoreCase) ?? false);
            OnPropertyChanged(nameof(InProgressTasksCount));

            ActiveTasksCount = tasks.Count(t => !t.Status?.Equals("Completed", StringComparison.OrdinalIgnoreCase) ?? true);
            OnPropertyChanged(nameof(ActiveTasksCount));

            var activeTasks = tasks
                .Where(t => !t.Status?.Equals("Completed", StringComparison.OrdinalIgnoreCase) ?? true)
                .OrderByDescending(t => t.Status?.Equals("In Progress", StringComparison.OrdinalIgnoreCase)) 
                .Take(5)
                .ToList();

            RecentTasks = new ObservableCollection<TaskDto>(activeTasks);

            var today = DateTime.Today;
            var nextWeek = today.AddDays(7);

            var events = await _calendarEventService.GetCalendarEventsAsync();

            var upcomingEvents = events
                .Where(e => e.StartDate >= today && e.StartDate <= nextWeek)
                .OrderBy(e => e.StartDate)
                .Take(3)
                .ToList();

            UpcomingEvents = new ObservableCollection<CalendarEventDto>(upcomingEvents);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Błąd", $"Nie udało się załadować danych: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void GoToTasks(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Tasks");
    }

    private async void GoToCalendar(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Calendar");
    }
    private static string GetStatusEmoji(string status)
    {
        return status?.ToLower() switch
        {
            "completed" => "✅",
            "in progress" => "🔄",
            "on hold" => "⏸️",
            "cancelled" => "❌",
            _ => "🆕" 
        };
    }

    private static Color GetPriorityColor(string priority)
    {
        return priority?.ToLower() switch
        {
            "critical" => Color.Parse("#F44336"), 
            "high" => Color.Parse("#FF9800"),     
            "medium" => Color.Parse("#FFC107"),   
            "low" => Color.Parse("#4CAF50"),      
            _ => Color.Parse("#9E9E9E")           
        };
    }

    private static Color GetEventColor(bool isAllDay, DateTime startDate)
    {
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
}

