using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace FileEncodingConverter.Core;

public static class EncodingConverter
{
    public static event Action<ConversionResult> OnConversionComplete;

    public static void ConvertFiles(ObservableCollection<FileInfo> files, string targetEncoding)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var succeededNumber = 0;
        var failedNumber = 0;
        var fileStatus = new bool[files.Count];

        foreach (var file in files.Select((value, index) => new { value, index }))
        {
            var encoding = file.value.FileEncoding switch
            {
                "UTF-8" => new UTF8Encoding(false),
                "UTF-8 with BOM" => new UTF8Encoding(true),
                "UTF-16 LE" => Encoding.Unicode,
                "UTF-16 BE" => Encoding.BigEndianUnicode,
                "GB2312" => Encoding.GetEncoding("gb2312"),
                "GBK" => Encoding.GetEncoding("gbk"),
                _ => null
            };

            // BUG: This implementation seems always produce "succeeded" results.
            // That is, we need to check each file after its encoding is being converted.
            if (encoding is not null)
            {
                var content = File.ReadAllText(file.value.FilePath, encoding);
                // TODO: Extract the mapping logic into an independent function.
                File.WriteAllText(file.value.FilePath, content, targetEncoding switch
                {
                    "UTF-8" => new UTF8Encoding(false),
                    "UTF-8 with BOM" => new UTF8Encoding(true),
                    "UTF-16 LE" => Encoding.Unicode,
                    "UTF-16 BE" => Encoding.BigEndianUnicode,
                    "GB2312" => Encoding.GetEncoding("gb2312"),
                    "GBK" => Encoding.GetEncoding("gbk"),
                    _ => new UTF8Encoding(false)
                });

                succeededNumber += 1;
                fileStatus[file.index] = true;
            }
            else
            {
                failedNumber += 1;
                fileStatus[file.index] = false;
            }
        }

        var result = new ConversionResult
        {
            SucceededNumber = succeededNumber,
            FailedNumber = failedNumber,
            FileStatus = fileStatus,
            TargetEncodingName = targetEncoding
        };

        OnConversionComplete?.Invoke(result);
    }
}