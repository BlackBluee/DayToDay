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
    public ICommand OpenGitHubCommand { get; private set; }

    public ProjectsPage(ProjectService projectService)
    {
        InitializeComponent();
        _projectService = projectService;

        AddCommand = new Command(OnAddClicked);
        EditCommand = new Command<ProjectDto>(OnEditProject);
        DeleteCommand = new Command<ProjectDto>(OnDeleteProject);
        OpenGitHubCommand = new Command<ProjectDto>(OnOpenGitHub);

        BindingContext = this;

        ProjectsRefreshView.Command = new Command(async () => {
            await LoadProjectsAsync();
            IsRefreshing = false;
        });

        LoadProjectsAsync();
    }

    private async Task LoadProjectsAsync()
    {
        try
        {
            IsRefreshing = true;
            var projects = await _projectService.GetProjectsAsync();
            _projects = new ObservableCollection<ProjectDto>(projects);
            ProjectsCollectionView.ItemsSource = _projects;
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

    private async void OnAddClicked()
    {
        try
        {
            string name = await DisplayPromptAsync("Nowy projekt", "Nazwa projektu:", maxLength: 50);
            if (string.IsNullOrWhiteSpace(name))
                return;

            string description = await DisplayPromptAsync("Opis", "Krótki opis projektu:", maxLength: 200);
            if (string.IsNullOrWhiteSpace(description))
                return;

            string gitHubUrl = await DisplayPromptAsync("GitHub URL", "Adres GitHub projektu (opcjonalnie):");

            var newProject = new ProjectDto
            {
                Name = name.Trim(),
                Description = description.Trim(),
                GitHubUrl = gitHubUrl?.Trim() ?? string.Empty,
                UserTasks = new List<UserTask>(),
                CalendarEvent = new List<CalendarEvent>()
            };

            IsBusy = true;
            await _projectService.CreateProjectAsync(newProject);
            await LoadProjectsAsync();
            await DisplayAlert("Sukces", "Projekt został utworzony pomyślnie", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Błąd", $"Nie udało się utworzyć projektu: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void OnEditProject(ProjectDto project)
    {
        if (project == null)
            return;
        try
        {
            string name = await DisplayPromptAsync("Edytuj projekt", "Nazwa projektu:",
                initialValue: project.Name, maxLength: 50);
            if (string.IsNullOrWhiteSpace(name))
                return;

            string description = await DisplayPromptAsync("Edytuj opis", "Krótki opis projektu:",
                initialValue: project.Description, maxLength: 200);
            if (string.IsNullOrWhiteSpace(description))
                return;

            string gitHubUrl = await DisplayPromptAsync("Edytuj GitHub URL", "Adres GitHub projektu (opcjonalnie):",
                initialValue: project.GitHubUrl);

            project.Name = name.Trim();
            project.Description = description.Trim();
            project.GitHubUrl = gitHubUrl?.Trim() ?? string.Empty;

            IsBusy = true;

            await _projectService.UpdateProjectAsync(project);

            await LoadProjectsAsync();

            await DisplayAlert("Sukces", "Projekt został zaktualizowany pomyślnie", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Błąd", $"Nie udało się zaktualizować projektu: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void OnDeleteProject(ProjectDto project)
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
            return;
        try
        {
            Uri uri = new Uri(project.GitHubUrl);
            await Browser.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
        }
        catch (Exception)
        {
            await DisplayAlert("Błąd", "Nie udało się otworzyć adresu URL. Sprawdź, czy adres jest prawidłowy.", "OK");
        }
    }
}
