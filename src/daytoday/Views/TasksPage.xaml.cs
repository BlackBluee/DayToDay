using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using daytoday.Core.DTOs;
using daytoday.Services; 
using Microsoft.Maui.Controls;

namespace daytoday.Views;

public partial class TasksPage : ContentPage
{
    private readonly TaskService _taskService;
    private ObservableCollection<TaskDto> _tasks;
    private bool _isRefreshing;
    private bool _isBusy;
    private TaskDto _currentTask;

    private readonly string[] StatusOptions = new[] { "New", "In Progress", "On Hold", "Completed", "Cancelled" };
    private readonly string[] PriorityOptions = new[] { "Low", "Medium", "High", "Critical" };
    private readonly string[] CategoryOptions = new[] { "General", "Work", "Personal", "Study", "Finance", "Health", "Other" };

    public int AllTasksCount => Tasks?.Count ?? 0;
    public int NewTasksCount => Tasks?.Count(t => t.Status?.Equals("New", StringComparison.OrdinalIgnoreCase) ?? false) ?? 0;
    public int InProgressTasksCount => Tasks?.Count(t => t.Status?.Equals("In Progress", StringComparison.OrdinalIgnoreCase) ?? false) ?? 0;
    public int CompletedTasksCount => Tasks?.Count(t => t.Status?.Equals("Completed", StringComparison.OrdinalIgnoreCase) ?? false) ?? 0;
    public int OnHoldTasksCount => Tasks?.Count(t => t.Status?.Equals("On Hold", StringComparison.OrdinalIgnoreCase) ?? false) ?? 0;
    public int CancelledTasksCount => Tasks?.Count(t => t.Status?.Equals("Cancelled", StringComparison.OrdinalIgnoreCase) ?? false) ?? 0;

    public ObservableCollection<TaskDto> Tasks
    {
        get => _tasks;
        set
        {
            _tasks = value;
            OnPropertyChanged();
            UpdateTaskCounters();
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

    public ICommand RefreshCommand { get; private set; }
    public ICommand AddCommand { get; private set; }
    public ICommand EditCommand { get; private set; }
    public ICommand DeleteCommand { get; private set; }

    public TasksPage(TaskService taskService)
    {
        InitializeComponent();
        _taskService = taskService;

        _tasks = new ObservableCollection<TaskDto>();

        RefreshCommand = new Command(async () => await LoadTasksAsync());
        AddCommand = new Command(async () => await ShowTaskForm(null));
        EditCommand = new Command<TaskDto>(async (task) => await ShowTaskForm(task));
        DeleteCommand = new Command<TaskDto>(async (task) => await OnDeleteTask(task));

        BindingContext = this;
        LoadTasksAsync();
    }
    private void UpdateTaskCounters()
    {
        OnPropertyChanged(nameof(AllTasksCount));
        OnPropertyChanged(nameof(NewTasksCount));
        OnPropertyChanged(nameof(InProgressTasksCount));
        OnPropertyChanged(nameof(CompletedTasksCount));
        OnPropertyChanged(nameof(OnHoldTasksCount));
        OnPropertyChanged(nameof(CancelledTasksCount));
    }

    public string GetStatusEmoji(string status)
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

    public string GetPriorityEmoji(string priority)
    {
        return priority?.ToLower() switch
        {
            "critical" => "🔴",
            "high" => "🟠",
            "medium" => "🟡",
            "low" => "🟢",
            _ => "⚪"
        };
    }

    public string GetCategoryEmoji(string category)
    {
        return category?.ToLower() switch
        {
            "work" => "💼",
            "personal" => "🏠",
            "study" => "📚",
            "finance" => "💰",
            "health" => "🏥",
            _ => "📋" 
        };
    }

    private async Task LoadTasksAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            IsRefreshing = true;

            Guid projectId = Guid.Empty; 
            var tasks = await _taskService.GetTasksAsync(projectId);
            Tasks = new ObservableCollection<TaskDto>(tasks);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Nie udało się załadować zadań: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    private async Task ShowTaskForm(TaskDto task)
    {
        _currentTask = task ?? new TaskDto
        {
            Status = "New",
            Priority = "Medium",
            Category = "General"
        };

        bool isNew = task == null;

        var titleEntry = new Entry { Text = _currentTask.Title, Placeholder = "Tytuł zadania", Style = (Style)Resources["FormEntry"] };
        var descriptionEditor = new Editor { Text = _currentTask.Description, Placeholder = "Opis zadania", Style = (Style)Resources["FormEditor"] };

        var categoryPicker = new Picker { Title = "Kategoria", Style = (Style)Resources["FormPicker"] };
        foreach (var category in CategoryOptions) categoryPicker.Items.Add(category);
        categoryPicker.SelectedItem = _currentTask.Category;

        var statusPicker = new Picker { Title = "Status", Style = (Style)Resources["FormPicker"] };
        foreach (var status in StatusOptions) statusPicker.Items.Add(status);
        statusPicker.SelectedItem = _currentTask.Status;

        var priorityPicker = new Picker { Title = "Priorytet", Style = (Style)Resources["FormPicker"] };
        foreach (var priority in PriorityOptions) priorityPicker.Items.Add(priority);
        priorityPicker.SelectedItem = _currentTask.Priority;

        var headerLabel = new Label
        {
            Text = isNew ? "Nowe zadanie" : "Edycja zadania",
            FontSize = 22,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#512BD4"), 
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 0, 20)
        };

        var form = new StackLayout
        {
            Padding = new Thickness(20),
            Children =
            {
                headerLabel,
                new Label { Text = "Tytuł", Style = (Style)Resources["FormLabel"] },
                titleEntry,
                new Label { Text = "Opis", Style = (Style)Resources["FormLabel"] },
                descriptionEditor,
                new Label { Text = "Kategoria", Style = (Style)Resources["FormLabel"] },
                categoryPicker,
                new Label { Text = "Status", Style = (Style)Resources["FormLabel"] },
                statusPicker,
                new Label { Text = "Priorytet", Style = (Style)Resources["FormLabel"] },
                priorityPicker
            }
        };

        var scrollView = new ScrollView { Content = form };

        var page = new ContentPage
        {
            Title = isNew ? "Nowe zadanie" : "Edytuj zadanie",
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
                if (string.IsNullOrWhiteSpace(titleEntry.Text))
                {
                    await DisplayAlert("Błąd", "Tytuł nie może być pusty", "OK");
                    return;
                }

                _currentTask.Title = titleEntry.Text;
                _currentTask.Description = descriptionEditor.Text ?? "";
                _currentTask.Category = categoryPicker.SelectedItem?.ToString() ?? "General";
                _currentTask.Status = statusPicker.SelectedItem?.ToString() ?? "New";
                _currentTask.Priority = priorityPicker.SelectedItem?.ToString() ?? "Medium";

                try
                {
                    IsBusy = true;

                    if (isNew)
                    {
                        var createdTask = await _taskService.CreateTaskAsync(_currentTask);
                        Tasks.Add(createdTask);
                        UpdateTaskCounters();
                    }
                    else
                    {
                        var updatedTask = await _taskService.UpdateTaskAsync(_currentTask);
                        int index = Tasks.IndexOf(Tasks.FirstOrDefault(t => t.Id == updatedTask.Id));
                        if (index >= 0)
                        {
                            Tasks[index] = updatedTask;
                            UpdateTaskCounters();
                        }
                    }

                    await Navigation.PopModalAsync();
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Błąd", $"Nie udało się {(isNew ? "utworzyć" : "zaktualizować")} zadania: {ex.Message}", "OK");
                }
                finally
                {
                    IsBusy = false;
                }
            })
        });

        await Navigation.PushModalAsync(new NavigationPage(page));
    }

    private async Task OnDeleteTask(TaskDto task)
    {
        if (task == null)
            return;

        bool confirm = await DisplayAlert("Potwierdź usunięcie",
            $"Czy na pewno chcesz usunąć zadanie \"{task.Title}\"?", "Usuń", "Anuluj");

        if (!confirm)
            return;

        try
        {
            IsBusy = true;
            await _taskService.DeleteTaskAsync(task.Id);
            Tasks.Remove(task);
            UpdateTaskCounters();
            await DisplayAlert("Sukces", "Zadanie zostało usunięte", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Błąd", $"Nie udało się usunąć zadania: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is CollectionView collectionView)
        {
            collectionView.SelectedItem = null;
        }
    }

    public string GetStatusColor(string status)
    {
        return status?.ToLower() switch
        {
            "completed" => "#4CAF50", 
            "in progress" => "#2196F3", 
            "on hold" => "#FF9800", 
            "cancelled" => "#F44336", 
            _ => "#9E9E9E" 
        };
    }
    public Color GetStatusColorForBinding(string status)
    {
        return Color.Parse(GetStatusColor(status));
    }
    public Color GetPriorityColor(string priority)
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
}
