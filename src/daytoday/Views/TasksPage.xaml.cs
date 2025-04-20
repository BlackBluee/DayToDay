using daytoday.Models;

namespace daytoday.Views;

public partial class TasksPage : ContentPage
{
    public TasksPage()
    {
        InitializeComponent();
        LoadTasks();
    }

    private void LoadTasks()
    {
        var tasks = new List<UserTask>
            {
                new UserTask { Title = "Zrobić nawigację w MAUI", Category = "Development", Time = "2h" },
                new UserTask { Title = "Napisać testy jednostkowe", Category = "Testing", Time = "1h" },
                new UserTask { Title = "Spotkanie z klientem (pilne!)", Category = "Meeting", Time = "30m" }
            };

        TasksCollectionView.ItemsSource = tasks;
    }
}
