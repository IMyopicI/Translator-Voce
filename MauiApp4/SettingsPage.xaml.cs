using System.Diagnostics;
using RestSharp;
using System.Text.Json; // Asigură-te că ai importat acest namespace
using System.IO;

namespace MauiApp4
{
    public partial class SettingsPage : ContentPage
    {
        private string selectedFilePath;

        public SettingsPage()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            VoiceEntry.Text = Preferences.Get("voice", "s3://voice-cloning-zero-shot/8e666786-3622-4d61-9502-aa4e187ccafe/original/manifest.json");
            AuthorizationEntry.Text = Preferences.Get("authorization", "142399e241354c86b3b68b8a02077969");
            UserIdEntry.Text = Preferences.Get("userId", "tyyIa69CxpXomt7o2T8VFKgE8ul2");
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            Preferences.Set("voice", VoiceEntry.Text);
            Preferences.Set("authorization", AuthorizationEntry.Text);
            Preferences.Set("userId", UserIdEntry.Text);
            DisplayAlert("Setări", "Setări salvate cu succes.", "OK");
        }

        private void OpenAppDirectoryButton_Click(object sender, EventArgs e)
        {
            string appDirectory = FileSystem.AppDataDirectory;
            Process.Start(new ProcessStartInfo
            {
                FileName = appDirectory,
                UseShellExecute = true,
                Verb = "open"
            });
        }

        private async void GetClonedVoicesButton_Click(object sender, EventArgs e)
        {
            var options = new RestClientOptions("https://api.play.ht/api/v2/cloned-voices");
            var client = new RestClient(options);
            var request = new RestRequest();
            request.AddHeader("AUTHORIZATION", AuthorizationEntry.Text);
            request.AddHeader("X-USER-ID", UserIdEntry.Text);
            var response = await client.GetAsync(request);

            if (response.IsSuccessful)
            {
                var voices = JsonSerializer.Deserialize<List<Voice>>(response.Content);
                VoicesCollectionView.ItemsSource = voices;
            }
            else
            {
                await DisplayAlert("Eroare", "Nu s-au putut recupera vocile.", "OK");
            }
        }

        private async void CopyVoiceIdButton_Click(object sender, EventArgs e)
        {
            var button = sender as Button;
            var voice = button.BindingContext as Voice;
            await Clipboard.SetTextAsync(voice.id);
            await DisplayAlert("Copiere", "ID-ul vocal a fost copiat în clipboard.", "OK");
        }

        private async void ChooseFileButton_Click(object sender, EventArgs e)
        {
            try
            {
                var result = await FilePicker.PickAsync();
                if (result != null)
                {
                    selectedFilePath = result.FullPath;
                    await DisplayAlert("Fișier selectat", $"Fișier selectat: {result.FileName}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Eroare", $"Fișier selectat: {ex.Message}", "OK");
            }
        }

        private async void CreateVoiceButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFilePath))
            {
                await DisplayAlert("Eroare", "Vă rugăm să alegeți mai întâi un fișier.", "OK");
                return;
            }

            string voiceName = await DisplayPromptAsync("Denumirea profilului vocii", "Introduceți numele noii voci:");

            if (string.IsNullOrEmpty(voiceName))
            {
                await DisplayAlert("Eroare", "Numele vocii nu poate fi necompletat.", "OK");
                return;
            }

            var options = new RestClientOptions("https://api.play.ht/api/v2/cloned-voices/instant");
            var client = new RestClient(options);
            var request = new RestRequest();
            request.AlwaysMultipartFormData = true;
            request.AddHeader("accept", "application/json");
            request.FormBoundary = "---011000010111000001101001";
            request.AddHeader("AUTHORIZATION", AuthorizationEntry.Text);
            request.AddHeader("X-USER-ID", UserIdEntry.Text);
            request.AddParameter("voice_name", voiceName);
            request.AddFile("file", selectedFilePath);

            var response = await client.PostAsync(request);

            if (response.IsSuccessful)
            {
                await DisplayAlert("Success", "Vocea creată cu succes.", "OK");
            }
            else
            {
                await DisplayAlert("Eroare", "Nu s-a putut crea vocea.", "OK");
            }
        }

        public class Voice
        {
            public string id { get; set; }
            public string name { get; set; }
        }
    }
}
