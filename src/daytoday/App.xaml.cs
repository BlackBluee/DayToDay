using daytoday.Views;

namespace daytoday
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            if (Preferences.ContainsKey("jwt_token") && !string.IsNullOrEmpty(Preferences.Get("jwt_token", "")))
            {
                MainPage = new AppShell();
            }
            else
            {
                MainPage = new NavigationPage(new LoginPage());
            }

        }

        
    }
}