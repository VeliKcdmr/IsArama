using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IsArama.Mobile.Services;
using System.Net.Http.Json;

namespace IsArama.Mobile.ViewModels;

public partial class FeedbackViewModel : ObservableObject
{
    private readonly HttpClient _http;

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _selectedSubject = string.Empty;
    [ObservableProperty] private string _message = string.Empty;
    [ObservableProperty] private string _sendButtonText = "Gönder";
    [ObservableProperty] private bool _isSending;
    [ObservableProperty] private bool _isSuccess;
    [ObservableProperty] private string _errorText = string.Empty;
    [ObservableProperty] private bool _hasError;

    public List<string> Subjects { get; } =
    [
        "💡 Öneri",
        "🐛 Hata Bildirimi",
        "📋 Eksik / Hatalı İlan",
        "🎨 Tasarım",
        "💬 Diğer"
    ];

    public FeedbackViewModel(HttpClient http) => _http = http;

    [RelayCommand]
    public async Task SendAsync()
    {
        HasError = false;

        if (string.IsNullOrWhiteSpace(SelectedSubject))
        {
            ErrorText = "Lütfen bir konu seçin.";
            HasError = true;
            return;
        }
        if (string.IsNullOrWhiteSpace(Message))
        {
            ErrorText = "Lütfen mesajınızı yazın.";
            HasError = true;
            return;
        }

        IsSending = true;
        SendButtonText = "Gönderiliyor...";
        try
        {
            var res = await _http.PostAsJsonAsync("http://192.168.1.15:8080/api/Feedback", new
            {
                name     = string.IsNullOrWhiteSpace(Name)  ? null : Name.Trim(),
                email    = string.IsNullOrWhiteSpace(Email) ? null : Email.Trim(),
                subject  = SelectedSubject.Trim(),
                message  = Message.Trim(),
                platform = "Mobile"
            });

            if (res.IsSuccessStatusCode)
                IsSuccess = true;
            else
            {
                ErrorText = "Bir hata oluştu, tekrar deneyin.";
                HasError = true;
            }
        }
        catch
        {
            ErrorText = "Bağlantı hatası, tekrar deneyin.";
            HasError = true;
        }
        finally
        {
            IsSending = false;
            SendButtonText = "Gönder";
        }
    }

    [RelayCommand]
    public async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    public void Reset()
    {
        Name = string.Empty;
        Email = string.Empty;
        SelectedSubject = string.Empty;
        Message = string.Empty;
        IsSuccess = false;
        HasError = false;
        ErrorText = string.Empty;
    }
}
