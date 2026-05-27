using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ChibitsLink.main.cs.viewmodel;

public abstract class BaseViewModel : INotifyPropertyChanged
{
    private bool _isBusy;
    private string _title = string.Empty;

    public bool IsBusy
    {
        get => _isBusy;
        set 
        {
            if (SetProperty(ref _isBusy, value))
                OnPropertyChanged(nameof(IsNotBusy));
        }
    }

    public bool IsNotBusy => !IsBusy;

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        bool changed = false;
        if (!EqualityComparer<T>.Default.Equals(storage, value))
        {
            storage = value;
            OnPropertyChanged(propertyName);
            changed = true;
        }
        return changed;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
