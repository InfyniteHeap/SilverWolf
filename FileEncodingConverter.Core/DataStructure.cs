namespace FileEncodingConverter.Core;

public record struct ConversionResult
{
    public required int FailedNumber;
    public required bool[] FileStatus;
    public required int SucceededNumber;
    public required string TargetEncodingName;
}

public record struct FileInfo
{
    // HACK: We were managed to use enumeration to represent each kind of encoding,
    // but we cannot get formal name of encodings to display on UI.
    // For example, if there is an enumeration variable called `Utf8`,
    // then we are hardly to let UI displays "UTF-8".
    public required string FileEncoding;
    public required string FileName;
    public required string FilePath;
    public required string FileStatus;
    public required string FileType;
}