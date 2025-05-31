// <copyright file="WhisperModelService.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Drastic.Services;
using transcribe.io.Models;
using Whisper.net.Ggml;

namespace transcribe.io.Services;

public class WhisperModelService : INotifyPropertyChanged
{
    private IAppDispatcher dispatcher;
    private WhisperModel? selectedModel;

    public WhisperModelService(IServiceProvider provider)
    {
        this.dispatcher = provider.GetRequiredService<IAppDispatcher>();
        foreach (var item in Enum.GetValues(typeof(GgmlType)))
        {
            foreach (var qantizationType in Enum.GetValues(typeof(QuantizationType)))
            {
                this.AllModels.Add(new WhisperModel((GgmlType)item, (QuantizationType)qantizationType));
            }
        }

        this.UpdateAvailableModels();
    }

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler? OnUpdatedSelectedModel;

    public event EventHandler? OnAvailableModelsUpdate;

    public ObservableCollection<WhisperModel> AllModels { get; } = new ObservableCollection<WhisperModel>();

    public ObservableCollection<WhisperModel> AvailableModels { get; } = new ObservableCollection<WhisperModel>();

    public WhisperModel? SelectedModel
    {
        get
        {
            return this.selectedModel;
        }

        set
        {
            this.SetProperty(ref this.selectedModel, value);
            this.OnUpdatedSelectedModel?.Invoke(this, EventArgs.Empty);
        }
    }

    public void UpdateAvailableModels()
    {
        lock (this)
        {
            this.dispatcher.Dispatch(() =>
            {
                this.AvailableModels.Clear();
                var models = this.AllModels.Where(n => n.Exists);
                foreach (var model in models)
                {
                    this.AvailableModels.Add(model);
                }

                if (this.SelectedModel is not null && !this.AvailableModels.Contains(this.SelectedModel))
                {
                    this.SelectedModel = null;
                }

                this.SelectedModel ??= this.AvailableModels.FirstOrDefault();
            });
        }

        this.OnAvailableModelsUpdate?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// On Property Changed.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        this.dispatcher?.Dispatch(() =>
        {
            var changed = this.PropertyChanged;
            if (changed == null)
            {
                return;
            }

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        });
    }

#pragma warning disable SA1600 // Elements should be documented
    protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "", Action? onChanged = null)
#pragma warning restore SA1600 // Elements should be documented
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
        {
            return false;
        }

        backingStore = value;
        onChanged?.Invoke();
        this.OnPropertyChanged(propertyName);
        return true;
    }
}