using Google.Cloud.Translation.V2;
using System.Diagnostics;

namespace MauiApp4
{
    public partial class TextPage : ContentPage
    {
        private TranslationResult translationResult; // Rezultatul traducerii

        public TextPage()
        {
            InitializeComponent();
        }

        private void TranslateButton_Click(object sender, EventArgs e)
        {
            var textToTranslate = TextInput.Text; // Textul de tradus
            var sourceLanguage = SourceLanguagePicker.SelectedItem?.ToString(); // Limba sursă
            var targetLanguage = TargetLanguagePicker.SelectedItem?.ToString(); // Limba țintă

            if (!string.IsNullOrEmpty(textToTranslate) &&
                !string.IsNullOrEmpty(sourceLanguage) &&
                !string.IsNullOrEmpty(targetLanguage))
            {
                TranslateText(textToTranslate, sourceLanguage, targetLanguage); // Traduce textul
            }
        }

        private async void TranslateText(string text, string sourceLanguage, string targetLanguage)
        {
            var translationClient = TranslationClient.Create();
            translationResult = translationClient.TranslateText(
                text: text,
                targetLanguage: targetLanguage,
                sourceLanguage: sourceLanguage);

            ResultLabeltranslate.Text = translationResult.TranslatedText;
            UpdateTranslationHistory(text, translationResult.TranslatedText);
        }

        public void UpdateTranslationHistory(string originalText, string translatedText)
        {
            var frame = new Frame
            {
                BackgroundColor = Color.FromRgb(170, 222, 255),
                BorderColor = Colors.Black,
                CornerRadius = 5,
                Padding = 10,
                Margin = new Thickness(0, 5)
            };

            var stackLayout = new StackLayout(); // Crează un layout de tip stack

            var originalLabel = new Label
            {
                Text = originalText,
                FontAttributes = FontAttributes.Bold
            };

            var originalCopyButton = new Button
            {
                Text = "📋",
                FontSize = 10,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Center 
            };
            originalCopyButton.Clicked += (s, e) => 
            {
                Clipboard.SetTextAsync(originalText);
            };

            var translatedLabel = new Label
            {
                Text = translatedText,
                FontAttributes = FontAttributes.Bold
            };

            var translatedCopyButton = new Button
            {
                Text = "📋",
                FontSize = 10,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Center 
            };
            translatedCopyButton.Clicked += (s, e) => 
            {
                Clipboard.SetTextAsync(translatedText);
            };

            var deleteButton = new Button
            {
                Text = "Delete",
                HorizontalOptions = LayoutOptions.End
            };
            deleteButton.Clicked += (s, e) =>
            {
                TranslationHistory.Children.Remove(frame);
            };

            stackLayout.Children.Add(originalLabel);
            stackLayout.Children.Add(originalCopyButton);
            stackLayout.Children.Add(translatedLabel);
            stackLayout.Children.Add(translatedCopyButton);
            stackLayout.Children.Add(deleteButton);

            frame.Content = stackLayout;
            TranslationHistory.Children.Insert(0, frame);
        }
    }
}
