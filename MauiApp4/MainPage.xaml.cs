namespace MauiApp4
{
    
    public partial class MainPage : FlyoutPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        // Eveniment pentru apasare pe butonul TranslatorPage
        private void OnTranslatorPageClicked(object sender, EventArgs e)
        {
            // Setează pagina de detaliu ca fiind TranslatorPage într-un NavigationPage
            Detail = new NavigationPage(new TranslatorPage());
            IsPresented = true; 
        }

        // Eveniment pentru apasare pe butonul TextPage
        private void OnTextPageClicked(object sender, EventArgs e)
        {
            // Setează pagina de detaliu ca fiind TextPage într-un NavigationPage
            Detail = new NavigationPage(new TextPage());
            IsPresented = true; 
        }

        // Eveniment pentru apasare pe butonul Settings
        private void SettingsButton_Click(object sender, EventArgs e)
        {
            // Setează pagina de detaliu ca fiind SettingsPage într-un NavigationPage
            Detail = new NavigationPage(new SettingsPage());
            IsPresented = true; 
        }
    }
}