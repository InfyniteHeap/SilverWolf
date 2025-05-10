using FileEncodingConverter.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using Windows.Storage.Pickers;
using WinRT;
using WinRT.Interop;

namespace FileEncodingConverter;

public sealed partial class MainPage
{
    private readonly ObservableCollection<FileInfo> _filesToBeProcessed = [];

    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnAddFileButtonClick(object sender, RoutedEventArgs e)
    {
        AddFileButton.IsEnabled = false;

        var picker = new FileOpenPicker
        {
            ViewMode = PickerViewMode.List,
            FileTypeFilter = { "*" }
        };

        var hWnd = WindowNative.GetWindowHandle(App.Window);
        InitializeWithWindow.Initialize(picker, hWnd);

        var files = await picker.PickMultipleFilesAsync();
        var fileNums = files.Count;

        var fileNoun = fileNums == 1 ? "file" : "files";
        var beVerb = fileNums == 1 ? "is" : "are";

        if (fileNums > 0)
        {
            foreach (var file in files)
            {
                var info = new FileInfo
                {
                    FileName = file.Name,
                    FilePath = file.Path,
                    FileType = file.FileType,
                    FileEncoding = EncodingDetector.GetEncodingType(file.Path),
                    FileStatus = null
                };

                // BUG: This judgement statement doesn't work as expected!
                if (_filesToBeProcessed.Contains(info))
                {
                    fileNums -= 1;
                    if (fileNums != 0) continue;

                    Result.Text = $"Selected {fileNoun} {beVerb} repeated.";

                    return;
                }

                _filesToBeProcessed.Add(info);
            }

            fileNoun = fileNums == 1 ? "file" : "files";
            Result.Text = $"Added {fileNums} {fileNoun}.";
        }
        else
        {
            Result.Text = "No files selected.";
        }

        AddFileButton.IsEnabled = true;
    }

    private async void OnSelectFolderButtonClick(object sender, RoutedEventArgs e)
    {
        SelectFolderButton.IsEnabled = false;

        var picker = new FolderPicker
        {
            ViewMode = PickerViewMode.List
        };

        var hWnd = WindowNative.GetWindowHandle(App.Window);
        InitializeWithWindow.Initialize(picker, hWnd);

        var folder = await picker.PickSingleFolderAsync();
        if (folder is not null)
        {
            var files = await folder.GetFilesAsync();
            var fileNums = files.Count;

            var fileNoun = fileNums == 1 ? "file" : "files";
            var beVerb = fileNums == 1 ? "is" : "are";

            if (fileNums > 0)
            {
                foreach (var file in files)
                {
                    var info = new FileInfo
                    {
                        FileName = file.Name,
                        FilePath = file.Path,
                        FileType = file.FileType,
                        FileEncoding = EncodingDetector.GetEncodingType(file.Path),
                        FileStatus = null
                    };

                    // BUG: This judgement statement doesn't work as expected!
                    if (_filesToBeProcessed.Contains(info))
                    {
                        fileNums -= 1;
                        if (fileNums != 0) continue;

                        Result.Text = $"Selected {fileNoun} {beVerb} repeated.";

                        return;
                    }

                    _filesToBeProcessed.Add(info);
                }

                fileNoun = fileNums == 1 ? "file" : "files";
                Result.Text = $"Added {fileNums} {fileNoun}.";
            }
            else
            {
                Result.Text = "This folder contains none of files!";
            }
        }
        else
        {
            Result.Text = "No folder selected.";
        }

        SelectFolderButton.IsEnabled = true;
    }

    private async void OnConvertButtonClick(object sender, RoutedEventArgs e)
    {
        if (_filesToBeProcessed.Count == 0)
        {
            var dialog = new ContentDialog
            {
                XamlRoot = XamlRoot,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "Empty Files",
                Content = "Add files before converting!",
                PrimaryButtonText = "Confirm",
                DefaultButton = ContentDialogButton.Primary
            };

            await dialog.ShowAsync();

            return;
        }

        EncodingConverter.OnConversionComplete += OnConversionCompleteHandler;

        ConvertButton.IsEnabled = false;
        AddFileButton.IsEnabled = false;
        SelectFolderButton.IsEnabled = false;
        TargetEncodingSelector.IsEnabled = false;

        FileProcessingProgressBar.Visibility = Visibility.Visible;

        var encoding = TargetEncodingSelector.SelectedItem.As<string>();

        EncodingConverter.ConvertFiles(_filesToBeProcessed, encoding);
    }

    private void OnRemoveButtonClick(object sender, RoutedEventArgs e)
    {
        var senderButton = sender.As<Button>();
        var fileInfo = senderButton.CommandParameter.As<FileInfo>();
        _filesToBeProcessed.Remove(fileInfo);
    }

    private void OnConversionCompleteHandler(ConversionResult result)
    {
        var succeededFileNum = result.SucceededFileNumber;
        var failedFileNum = result.FailedFileNumber;

        var succeededFileNumNoun = succeededFileNum == 1 ? "file" : "files";
        var failedFileNumNoun = failedFileNum == 1 ? "file" : "files";

        Result.Text =
            $"Succeeded {succeededFileNum} {succeededFileNumNoun}, failed {result.FailedFileNumber} {failedFileNumNoun}.";

        for (var idx = 0; idx < _filesToBeProcessed.Count; idx += 1)
            _filesToBeProcessed[idx] = new FileInfo
            {
                FileName = _filesToBeProcessed[idx].FileName,
                FilePath = _filesToBeProcessed[idx].FilePath,
                FileType = _filesToBeProcessed[idx].FileType,
                FileEncoding = result.TargetEncodingName,
                FileStatus = result.FileStatus[idx] ? "Succeeded" : "Failed"
            };

        ConvertButton.IsEnabled = true;
        AddFileButton.IsEnabled = true;
        SelectFolderButton.IsEnabled = true;
        TargetEncodingSelector.IsEnabled = true;

        FileProcessingProgressBar.Visibility = Visibility.Collapsed;
    }
}