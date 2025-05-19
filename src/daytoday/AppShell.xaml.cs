using daytoday.Services;
using daytoday.Views;
using Microsoft.Maui.Controls;

namespace daytoday
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            //Preferences.Remove("jwt_token");
            
            var token = Preferences.Get("jwt_token", "");
            if (string.IsNullOrWhiteSpace(token))
            {


                Items.Add(new ShellContent
                {
                    Title = "Logowanie",
                    Icon = "settings.png",
                    ContentTemplate = new DataTemplate(typeof(LoginPage)),
                    Route = "Login"
                });
                LoginInfoLabel.Text = "Nie zalogowano";
            }
            else
            {
                Items.Add(new ShellContent
                {
                    Title = "Pulpit",
                    Icon = "dashboard.png",
                    ContentTemplate = new DataTemplate(typeof(DashboardPage)),
                    Route = "Dashboard"
                });
                Items.Add(new ShellContent
                {
                    Title = "Projekty",
                    Icon = "dashboard.png",
                    ContentTemplate = new DataTemplate(typeof(ProjectsPage)),
                    Route = "Projects"
                });
                Items.Add(new ShellContent
                {
                    Title = "Zadania",
                    Icon = "tasks.png",
                    ContentTemplate = new DataTemplate(typeof(TasksPage)),
                    Route = "Tasks"
                });
                Items.Add(new ShellContent
                {
                    Title = "Kalendarz",
                    Icon = "calendar.png",
                    ContentTemplate = new DataTemplate(typeof(CalendarPage)),
                    Route = "Calendar"
                });
                Items.Add(new ShellContent
                {
                    Title = "Ustawienia",
                    Icon = "settings.png",
                    ContentTemplate = new DataTemplate(typeof(SettingsPage)),
                    Route = "Settings"
                });
                LoginInfoLabel.Text = "Zalogowano";
            }

            this.Navigating += AppShell_Navigating;
        }

        private void AppShell_Navigating(object sender, ShellNavigatingEventArgs e)
        {
            if (e.Target.Location.OriginalString.Contains("Login"))
                return;

            var token = Preferences.Get("jwt_token", null);
            if (string.IsNullOrEmpty(token))
            {
                e.Cancel();
                Shell.Current.GoToAsync("//Login");
            }
        }
    }
}
