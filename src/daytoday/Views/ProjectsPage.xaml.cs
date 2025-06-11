using System.Collections.ObjectModel;
using System.Windows.Input;
using daytoday.Core.DTOs;
using daytoday.Core.Models;
using daytoday.Services;
using Microsoft.Maui.Controls;

namespace daytoday.Views;

public partial class ProjectsPage : ContentPage
{
    private readonly ProjectService _projectService;
    private ObservableCollection<ProjectDto> _projects;
    private bool _isRefreshing;
    private bool _isBusy;
    private ProjectDto _currentProject;

    public int ProjectsCount => _projects?.Count ?? 0;
    public int TasksCount => _projects?.Sum(p => p.UserTasks?.Count ?? 0) ?? 0;

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

    public ICommand AddCommand { get; private set; }
    public ICommand EditCommand { get; private set; }
    public ICommand DeleteCommand { get; private set; }
    public ICommand OpenGitHubCommand { get; private set; }

    public ProjectsPage(ProjectService projectService)
    {
        InitializeComponent();
        _projectService = projectService;
        _projects = new ObservableCollection<ProjectDto>();

        AddCommand = new Command(async () => await ShowProjectForm(null));
        EditCommand = new Command<ProjectDto>(async (project) => await ShowProjectForm(project));
        DeleteCommand = new Command<ProjectDto>(async (project) => await OnDeleteProject(project));
        OpenGitHubCommand = new Command<ProjectDto>(OnOpenGitHub);

        BindingContext = this;

        if (ProjectsRefreshView != null)
        {
            ProjectsRefreshView.Command = new Command(async () =>
            {
                await LoadProjectsAsync();
                IsRefreshing = false;
            });
        }

        LoadProjectsAsync();
    }

    private async Task LoadProjectsAsync()
    {
        try
        {
            IsRefreshing = true;
            var projects = await _projectService.GetProjectsAsync();
            _projects = new ObservableCollection<ProjectDto>(projects);

            if (ProjectsCollectionView != null)
            {
                ProjectsCollectionView.ItemsSource = _projects;
            }

            OnPropertyChanged(nameof(ProjectsCount));
            OnPropertyChanged(nameof(TasksCount));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Błąd", $"Nie udało się załadować projektów: {ex.Message}", "OK");
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is CollectionView collectionView)
        {
            collectionView.SelectedItem = null;
        }
    }

    private async Task ShowProjectForm(ProjectDto project)
    {
        _currentProject = project ?? new ProjectDto
        {
            Name = string.Empty,
            Description = string.Empty,
            GitHubUrl = string.Empty,
            UserTasks = new List<UserTask>(),
            CalendarEvent = new List<CalendarEvent>()
        };

        bool isNew = project == null;

        try
        {
            var nameEntry = new Entry
            {
                Text = _currentProject.Name,
                Placeholder = "Nazwa projektu",
                Style = Resources.ContainsKey("FormEntry") ? (Style)Resources["FormEntry"] : null
            };

            var descriptionEditor = new Editor
            {
                Text = _currentProject.Description,
                Placeholder = "Opis projektu",
                Style = Resources.ContainsKey("FormEditor") ? (Style)Resources["FormEditor"] : null
            };

            var gitHubUrlEntry = new Entry
            {
                Text = _currentProject.GitHubUrl,
                Placeholder = "URL repozytorium GitHub (opcjonalnie)",
                Style = Resources.ContainsKey("FormEntry") ? (Style)Resources["FormEntry"] : null,
                Keyboard = Keyboard.Url
            };

            var form = new StackLayout
            {
                Padding = new Thickness(20),
                Children =
                {
                    new Label { Text = "Nazwa projektu", Style = Resources.ContainsKey("FormLabel") ? (Style)Resources["FormLabel"] : null },
                    nameEntry,
                    new Label { Text = "Opis", Style = Resources.ContainsKey("FormLabel") ? (Style)Resources["FormLabel"] : null },
                    descriptionEditor,
                    new Label { Text = "URL repozytorium GitHub", Style = Resources.ContainsKey("FormLabel") ? (Style)Resources["FormLabel"] : null },
                    gitHubUrlEntry
                }
            };

            var scrollView = new ScrollView { Content = form };

            var page = new ContentPage
            {
                Title = isNew ? "Nowy projekt" : "Edytuj projekt",
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
                        await DisplayAlert("Błąd", "Nazwa projektu nie może być pusta", "OK");
                        return;
                    }

                    _currentProject.Name = nameEntry.Text.Trim();
                    _currentProject.Description = descriptionEditor.Text?.Trim() ?? string.Empty;
                    _currentProject.GitHubUrl = gitHubUrlEntry.Text?.Trim() ?? string.Empty;

                    try
                    {
                        IsBusy = true;

                        if (isNew)
                        {
                            await _projectService.CreateProjectAsync(_currentProject);
                        }
                        else
                        {
                            await _projectService.UpdateProjectAsync(_currentProject);
                        }

                        await LoadProjectsAsync();
                        await Navigation.PopModalAsync();
                        await DisplayAlert("Sukces", $"Projekt został {(isNew ? "utworzony" : "zaktualizowany")} pomyślnie", "OK");
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Błąd", $"Nie udało się {(isNew ? "utworzyć" : "zaktualizować")} projektu: {ex.Message}", "OK");
                    }
                    finally
                    {
                        IsBusy = false;
                    }
                })
            });

            await Navigation.PushModalAsync(new NavigationPage(page));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Błąd", $"Wystąpił problem przy tworzeniu formularza: {ex.Message}", "OK");
        }
    }

    private async Task OnDeleteProject(ProjectDto project)
    {
        if (project == null)
            return;

        bool confirm = await DisplayAlert("Potwierdź usunięcie",
            $"Czy na pewno chcesz usunąć projekt \"{project.Name}\"?", "Usuń", "Anuluj");

        if (!confirm)
            return;
        try
        {
            IsBusy = true;
            await _projectService.DeleteProjectAsync(project.Id);
            _projects.Remove(project);

            OnPropertyChanged(nameof(ProjectsCount));
            OnPropertyChanged(nameof(TasksCount));

            await DisplayAlert("Sukces", "Projekt został usunięty", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Błąd", $"Nie udało się usunąć projektu: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void OnOpenGitHub(ProjectDto project)
    {
        if (project == null || string.IsNullOrWhiteSpace(project.GitHubUrl))
        {
            await DisplayAlert("Informacja", "Ten projekt nie ma przypisanego adresu GitHub.", "OK");
            return;
        }

        try
        {
            string url = project.GitHubUrl;

            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                url = "https://" + url;
            }

            Uri uri = new Uri(url);
            await Browser.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
        }
        catch (Exception)
        {
            await DisplayAlert("Błąd", "Nie udało się otworzyć adresu URL. Sprawdź, czy adres jest prawidłowy.", "OK");
        }
    }
}
