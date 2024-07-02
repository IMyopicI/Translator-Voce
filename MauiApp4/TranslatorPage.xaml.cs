using NAudio.Wave;
using Google.Cloud.Speech.V1;
using Google.Cloud.Translation.V2;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace MauiApp4
{
    public partial class TranslatorPage : ContentPage
    {
        private WaveInEvent waveIn;
        private WaveFileWriter waveFileWriter;
        private bool isRecording = false;
        private int clickCount = 0;
        private TranslationResult translationResult;
        private WaveOutEvent waveOutEvent;
        private string sourceLanguageCode = "ro-RO";
        private string targetLanguage = "en";
        private bool isReverseTranslation = false;
        private bool shouldUseTextToSpeech = true;

        public TranslatorPage()
        {
            InitializeComponent();
            InitializeAudio();
        }

        private void InitializeAudio()
        {
            waveIn = new WaveInEvent();
            waveIn.DataAvailable +=
                WaveIn_DataAvailable; // Atașează evenimentul pentru când datele audio sunt disponibile
            waveIn.RecordingStopped +=
                WaveIn_RecordingStopped; // Atașează evenimentul pentru când înregistrarea se oprește
            waveIn.WaveFormat = new WaveFormat(44100, 1); // Setează formatul audio
        }

        private void RecordButton_Click(object sender, EventArgs e)
        {
            clickCount++;
            if (clickCount == 1)
            {
                // Începe înregistrarea
                string fileName = $"recorded_audio.wav";
                string filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
                waveFileWriter = new WaveFileWriter(filePath, waveIn.WaveFormat);
                waveIn.StartRecording();
                isRecording = true;
                ResultLabeltranslate.Text = ""; //clear
                ResultLabel.Text = ""; //clear
                sourceLanguageCode = "ro-RO"; // Setează limba sursă
                targetLanguage = "en"; // Setează limba țintă
            }
            else if (clickCount == 2)
            {
                // Oprește înregistrarea
                if (isRecording)
                {
                    waveIn.StopRecording();
                    isRecording = false;
                    clickCount = 0;
                }
            }
        }

        private async void ReverseTranslateButton_Click(object sender, EventArgs e)
        {
            clickCount++;

            if (clickCount == 1)
            {
                // Începe înregistrarea pentru traducere inversă
                string fileName = $"recorded_audio.wav";
                string filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
                waveFileWriter = new WaveFileWriter(filePath, waveIn.WaveFormat);
                waveIn.StartRecording();
                isRecording = true;
                ResultLabeltranslate.Text = "";
                ResultLabel.Text = "";
                sourceLanguageCode = "en-US"; // Schimbă limba sursă la engleză
                targetLanguage = "ro"; // Schimbă limba țintă la română
            }
            else if (clickCount == 2)
            {
                // Oprește înregistrarea
                if (isRecording)
                {
                    waveIn.StopRecording();
                    isRecording = false;
                    isReverseTranslation = true; // Setează că este traducere inversă
                    clickCount = 0;
                }
            }
        }

        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            // Scrie datele audio disponibile în fișier
            if (waveFileWriter != null && isRecording)
            {
                waveFileWriter.Write(e.Buffer, 0, e.BytesRecorded);
            }
        }

        private void WaveIn_RecordingStopped(object sender, StoppedEventArgs e)
        {
            // Oprește și eliberează resursele de înregistrare
            if (waveFileWriter != null)
            {
                waveFileWriter.Dispose();
                waveFileWriter = null;
                ProcessAudioFile(); // Procesează fișierul audio
            }
        }

        private async void ProcessAudioFile()
        {
            string fileName = "recorded_audio.wav";
            string filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            // Recunoaștere vocală utilizând Google Cloud Speech-to-Text
            var speech = SpeechClient.Create();
            var response = await speech.RecognizeAsync(new RecognitionConfig
            {
                Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                SampleRateHertz = 44100,
                LanguageCode = sourceLanguageCode, // Limba pentru recunoaștere
            }, RecognitionAudio.FromFile(filePath));

            string recognizedText = "";
            foreach (var result in response.Results)
            {
                foreach (var alternative in result.Alternatives)
                {
                    recognizedText += alternative.Transcript + "\n"; // Obține textul recunoscut
                }
            }

            if (!string.IsNullOrEmpty(recognizedText))
            {
                ResultLabel.Text = recognizedText;

                // Traduce textul utilizând Google Cloud Translation API
                var translationClient = TranslationClient.Create();
                translationResult = translationClient.TranslateText(
                    text: recognizedText,
                    targetLanguage: targetLanguage); // Limba țintă pentru traducere

                ResultLabeltranslate.Text = translationResult.TranslatedText;
                //setăm ca la traducerea inversă să nu facă conversia textului în mesaj vocal
                if (!isReverseTranslation)
                {
                    TextToSpeech(); // Convertește textul tradus în vorbire
                }

                isReverseTranslation = false;
            }
            else
            {
                ResultLabel.Text = "Recunoașterea a eșuat sau nu a fost detectat niciun text.";
                clickCount = 0;
            }

            // Șterge fișierul audio înregistrat
            File.Delete(filePath);
        }

        private async void UpdateTranslationHistory(string originalText, string translatedText, string url)
        {
            // Actualizează istoricul traducerilor
            var frame = new Frame
            {
                BackgroundColor = Color.FromRgb(170, 222, 255),
                BorderColor = Colors.Black,
                CornerRadius = 5,
                Padding = 10,
                Margin = new Thickness(0, 5)
            };

            var stackLayout = new StackLayout();

            var originalLabel = new Label
            {
                Text = originalText,
                FontAttributes = FontAttributes.Bold
            };

            var copyOriginalButton = new Button
            {
                Text = "📋",
                FontSize = 10,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Center
            };
            copyOriginalButton.Clicked += (s, e) => { CopyTextToClipboard(originalText); };

            var translatedLabel = new Label
            {
                Text = translatedText,
                FontAttributes = FontAttributes.Bold
            };

            var copyTranslatedButton = new Button
            {
                Text = "📋",
                FontSize = 10,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Center
            };
            copyTranslatedButton.Clicked += (s, e) => { CopyTextToClipboard(translatedText); };

            var playButton = new Button
            {
                Text = "Play",
                HorizontalOptions = LayoutOptions.Start
            };

            var downloadButton = new Button
            {
                Text = "➥",
                HorizontalOptions = LayoutOptions.Start
            };

            var deleteButton = new Button
            {
                Text = "Șterge",
                HorizontalOptions = LayoutOptions.End
            };

            var urlsalvat = new Label
            {
                Text = url,
                HorizontalOptions = LayoutOptions.End,
                TextColor = Colors.Transparent,
                FontSize = 1
            };

            // Adaugă evenimentul pentru ștergerea traducerii din istoric
            deleteButton.Clicked += (s, e) => { TranslationHistory.Children.Remove(frame); };

            // Adaugă evenimentul pentru redarea audio
            playButton.Clicked += (s, e) => { PlayAudioFromUrlAsync(url); };

            // Adaugă evenimentul pentru descărcarea audio
            downloadButton.Clicked += async (s, e) => { await DownloadAudioFromUrlAsync(url); };

            var buttonLayout = new HorizontalStackLayout
            {
                Children = { downloadButton, deleteButton }
            };

            stackLayout.Children.Add(originalLabel);
            stackLayout.Children.Add(copyOriginalButton);
            stackLayout.Children.Add(translatedLabel);
            stackLayout.Children.Add(copyTranslatedButton);
            stackLayout.Children.Add(urlsalvat);
            stackLayout.Children.Add(playButton);
            stackLayout.Children.Add(buttonLayout);
            frame.Content = stackLayout;
            TranslationHistory.Children.Insert(0, frame);
        }

        private async void CopyTextToClipboard(string text)
        {
            await Clipboard.SetTextAsync(text);
        }


        private string GetUrlFromBody(string body)
        {
            // Extrage URL-ul din răspunsul HTTP utilizând expresii regulate
            var regex = new Regex(@"https://[^\s]+");
            var match = regex.Match(body);

            if (match.Success)
            {
                var result = match.Value.Split('"');
                return result[0];
            }

            return null;
        }

        private async void TextToSpeech()
        {
            // Convertește textul tradus în vorbire utilizând API-ul PlayHT
            var text = translationResult.TranslatedText.Replace("\n", "");
            var voice = Preferences.Get("voice",
                "s3://voice-cloning-zero-shot/8e666786-3622-4d61-9502-aa4e187ccafe/original/manifest.json");
            var authorization = Preferences.Get("authorization", "142399e241354c86b3b68b8a02077969");
            var userId = Preferences.Get("userId", "tyyIa69CxpXomt7o2T8VFKgE8ul2");
            var outputFormat = "wav";
            var voiceEngine = "PlayHT2.0";
            var emotion = "male_surprised";

            var jsonString = "{" +
                             "\"text\":\"" + text + "\"," +
                             "\"voice\":\"" + voice + "\"," +
                             "\"output_format\":\"" + outputFormat + "\"," +
                             "\"voice_engine\":\"" + voiceEngine + "\"," +
                             "\"emotion\":\"" + emotion + "\"" +
                             "}";

            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://api.play.ht/api/v2/tts"),
                Headers =
                {
                    { "accept", "text/event-stream" },
                    { "AUTHORIZATION", authorization },
                    { "X-USER-ID", userId },
                },
                Content = new StringContent(jsonString)
                {
                    Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
                }
            };
            var result = await getResponse(client, request); // Obține URL-ul fișierului audio
            PlayAudioFromUrlAsync(result); // Redă fișierul audio
            UpdateTranslationHistory(ResultLabel.Text, translationResult.TranslatedText,
                result); // Actualizarea istoricului traducerilor
        }

        private async Task<string> getResponse(HttpClient client, HttpRequestMessage request)
        {
            // Trimite cererea și obține răspunsul de la API-ul PlayHT
            var response = await client.SendAsync(request);
            var url = await response.Content.ReadAsStringAsync();
            var result = GetUrlFromBody(url);
            return result;
        }

        private async void PlayAudioFromUrlAsync(string url)
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(url); // Obținem fișierul audio de la URL
                if (response.IsSuccessStatusCode)
                {
                    var stream = await response.Content.ReadAsStreamAsync();
                    if (waveOutEvent != null)
                    {
                        waveOutEvent.Stop();
                        waveOutEvent.Dispose();
                    }

                    var waveStream = new RawSourceWaveStream(stream, new WaveFormat(11025, 2)); // Creează fluxul audio
                    var waveChannel32 = new WaveChannel32(waveStream);
                    if (waveOutEvent == null)
                    {
                        waveOutEvent = new WaveOutEvent();
                    }

                    waveOutEvent.Init(waveChannel32); // Inițializează redarea audio
                    waveOutEvent.Play(); // Redă fișierul audio
                }
            }
        }

        private async Task DownloadAudioFromUrlAsync(string url)
        {
            // Descărcă fișierul audio de la URL
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsByteArrayAsync();

                    // Generare nume fișier cu data și ora curentă
                    var fileName = $"translated_audio_{DateTime.Now:yyyyMMdd_HHmmss}.wav";

                    var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);

                    File.WriteAllBytes(filePath, content);
                    await DisplayAlert("Descarcă", "Fișierul audio a fost descărcat cu succes.", "OK");
                }
                else
                {
                    await DisplayAlert("Descarcă", "Nu s-a putut descărca fișierul audio.", "OK");
                }
            }
        }
    }
}