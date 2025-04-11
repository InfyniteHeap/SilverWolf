using System.Collections.Generic;
using System.IO;

namespace FileEncodingConverter.Core;

public static class EncodingDetector
{
    public static string GetEncodingType(string filePath)
    {
        var encodingResultCandidates = new List<EncodingResult>(3);

        var bytes = File.ReadAllBytes(filePath);

        // If this file is empty, we assume that this file is encoded in UTF-8.
        if (bytes.Length == 0) return "UTF-8";

        var utf8Result = DetectUtf8Bytes(bytes);
        encodingResultCandidates.Add(utf8Result);

        var utf16Result = DetectUtf16Bytes(bytes);
        encodingResultCandidates.Add(utf16Result);

        var gbResult = DetectGbBytes(bytes);
        encodingResultCandidates.Add(gbResult);

        encodingResultCandidates.Sort((x, y) => -x.Confidence.CompareTo(y.Confidence));

        return encodingResultCandidates[0].Type;
    }

    private static EncodingResult DetectUtf8Bytes(byte[] bytes)
    {
        var hasBom = false;
        var len = bytes.Length;

        var validUtf8CharNum = 0;
        var approximateActualUtf8CharNum = 0;

        var idx = 0;

        if (len > 2 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
        {
            hasBom = true;
            idx += 3;
        }

        // HACK: Too much `if` there!
        // Are there some more effective methods to do so? QAQ
        while (idx < len)
        {
            if ((bytes[idx] & 0x80) == 0x00)
            {
                validUtf8CharNum += 1;
                approximateActualUtf8CharNum += 1;

                idx += 1;

                continue;
            }

            if (idx + 1 >= len)
            {
                approximateActualUtf8CharNum += 1;

                break;
            }

            if ((bytes[idx] & 0xE0) == 0xC0 && (bytes[idx + 1] & 0xC0) == 0x80)
            {
                validUtf8CharNum += 1;
                approximateActualUtf8CharNum += 1;

                idx += 2;

                continue;
            }

            if (idx + 2 >= len)
            {
                approximateActualUtf8CharNum += 1;

                break;
            }

            if ((bytes[idx] & 0xF0) == 0xE0 &&
                (bytes[idx + 1] & 0xC0) == 0x80 &&
                (bytes[idx + 2] & 0xC0) == 0x80)
            {
                validUtf8CharNum += 1;
                approximateActualUtf8CharNum += 1;

                idx += 3;

                continue;
            }

            if (idx + 3 >= len)
            {
                approximateActualUtf8CharNum += 1;

                break;
            }

            if ((bytes[idx] & 0xF8) == 0xF0 &&
                (bytes[idx + 1] & 0xC0) == 0x80 &&
                (bytes[idx + 2] & 0xC0) == 0x80 &&
                (bytes[idx + 3] & 0xC0) == 0x80)
            {
                validUtf8CharNum += 1;
                approximateActualUtf8CharNum += 1;

                idx += 4;

                continue;
            }

            // We also regard this character as an "actual" UTF-8 character,
            // even if this character is actually invalid.
            approximateActualUtf8CharNum += 1;

            idx += 1;
        }

        return new EncodingResult(hasBom ? "UTF-8 with BOM" : "UTF-8",
            (double)validUtf8CharNum / approximateActualUtf8CharNum + (hasBom ? 0.2 : 0));
    }

    // TODO: This function is incomplete, and it just works!
    private static EncodingResult DetectUtf16Bytes(byte[] bytes)
    {
        // An initial judgement of the UTF-16 ending
        // by reading first two bytes of the file.
        var ending = bytes[..2] switch
        {
            [0xFF, 0xFE] => Utf16Ending.Little,
            [0xFE, 0xFF] => Utf16Ending.Big,
            _ => Utf16Ending.Unknown
        };

        if (ending is not Utf16Ending.Unknown)
            return new EncodingResult(ending == Utf16Ending.Little ? "UTF-16 LE" : "UTF-16 BE", 0.8);

        return new EncodingResult("Unknown", 0.5);
    }

    private static EncodingResult DetectGbBytes(byte[] bytes)
    {
        var len = bytes.Length;
        if (len < 2) return new EncodingResult("Unknown", 0.6);

        var validOneByteUtf8CharNum = 0;
        var validGb2312CharNum = 0;
        var validGbkCharNum = 0;
        var approximateActualCharNum = 0;

        var idx = 0;

        while (idx < len)
        {
            // ASCII
            if ((bytes[idx] & 0x80) == 0x00)
            {
                validOneByteUtf8CharNum += 1;

                idx += 1;

                continue;
            }

            if (idx + 1 >= len)
            {
                approximateActualCharNum += 1;

                break;
            }

            // GB2312
            if (bytes[idx] > 0xB0 && bytes[idx] < 0xF7 && bytes[idx + 1] > 0xA1 && bytes[idx + 1] < 0xFE)
            {
                validGb2312CharNum += 1;
                approximateActualCharNum += 1;

                idx += 2;
            }
            // GBK
            else if (bytes[idx] > 0x81 && bytes[idx] < 0xFE &&
                     ((bytes[idx + 1] > 0x40 && bytes[idx + 1] < 0x7E) ||
                      (bytes[idx + 1] > 0x80 && bytes[idx + 1] < 0xFE)))
            {
                validGbkCharNum += 1;
                approximateActualCharNum += 1;

                idx += 2;
            }
            else
            {
                approximateActualCharNum += 1;

                idx += 1;
            }
        }

        if (validOneByteUtf8CharNum > validGb2312CharNum && validOneByteUtf8CharNum > validGbkCharNum)
            return new EncodingResult("UTF-8", (double)validOneByteUtf8CharNum / approximateActualCharNum);
        if (validGb2312CharNum > validOneByteUtf8CharNum && validGb2312CharNum > validGbkCharNum)
            return new EncodingResult("GB2312", (double)validGb2312CharNum / approximateActualCharNum);
        if (validGbkCharNum > validOneByteUtf8CharNum && validGbkCharNum > validGb2312CharNum)
            return new EncodingResult("GBK", (double)validGbkCharNum / approximateActualCharNum);

        return new EncodingResult("Unknown", 0.8);
    }

    private record struct EncodingResult(string Type, double Confidence);

    private enum Utf16Ending
    {
        Unknown,
        Little,
        Big
    }
}