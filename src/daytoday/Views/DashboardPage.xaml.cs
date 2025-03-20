namespace daytoday.Views;

public partial class DashboardPage : ContentPage
{
    public DashboardPage()
    {
        InitializeComponent();
    }

    private async void GoToTasks(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Tasks");
    }

    private async void GoToCalendar(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Calendar");
    }
}