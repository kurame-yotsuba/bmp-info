// BMPのヘッダについては以下を参考にした
// http://www.umekkii.jp/data/computer/file_format/bitmap.cgi

// 引数個数チェック
if (args.Length != 1)
{
    Console.WriteLine("BMPファイルを１つ指定してください。");
    return (int)BmpInfoResult.ArgumentError;
}

string filePath = args[0];

try
{
    ReadOnlySpan<byte> BM = stackalloc byte[] { (byte)'B', (byte)'M' };
    using var fs = new FileStream(filePath, FileMode.Open);

    // BMPファイルのヘッダは2バイトと4バイトしか使われていない
    Span<byte> buff2 = stackalloc byte[2];
    Span<byte> buff4 = stackalloc byte[4];

    /////////////////////////////////////////////
    // ファイルヘッダ
    /////////////////////////////////////////////

    // ファイルタイプ 2 byte
    fs.Read(buff2);

    if (buff2.SequenceEqual(BM) is false)
    {
        Console.WriteLine($"{filePath} はBMPファイルではありません。");
        return (int)BmpInfoResult.NotBmpError;
    }

    // ファイルサイズ 4 byte
    fs.Read(buff4);
    int fileSize = BitConverter.ToInt32(buff4);

    // 予約領域 2 byte x 2
    fs.Read(buff4);

    // ファイル先頭から画像データまでのオフセット
    fs.Read(buff4);
    int dataOffset = BitConverter.ToInt32(buff4);

    /////////////////////////////////////////////
    // 情報ヘッダ
    /////////////////////////////////////////////

    // 情報ヘッダサイズ
    fs.Read(buff4);
    int infoSize = BitConverter.ToInt32(buff4);

    // 画像の幅
    fs.Read(buff4);
    int width = BitConverter.ToInt32(buff4);

    // 画像の高さ
    fs.Read(buff4);
    int height = BitConverter.ToInt32(buff4);

    // プレーン数（常に1）
    fs.Read(buff2);
    short planeCount = BitConverter.ToInt16(buff2);

    // 色ビット数
    fs.Read(buff2);
    short bitCount = BitConverter.ToInt16(buff2);

    // 圧縮形式
    fs.Read(buff4);
    int compression = BitConverter.ToInt32(buff4);

    // 画像データサイズ
    fs.Read(buff4);
    int imageSize = BitConverter.ToInt32(buff4);

    // 水平解像度
    fs.Read(buff4);
    double horizontalPixelPerMeter = BitConverter.ToInt32(buff4) / 1000.0 * 25.4;

    // 垂直解像度
    fs.Read(buff4);
    double verticalPixelPerMeter = BitConverter.ToInt32(buff4) / 1000.0 * 25.4;

    // 格納パレット数（0の場合もある）
    fs.Read(buff4);
    int usedColorCount = BitConverter.ToInt32(buff4);

    // 重要色数
    fs.Read(buff4);
    int importantColorCount = BitConverter.ToInt32(buff4);

    Console.WriteLine($"ファイルサイズ：{fileSize:N0}");
    Console.WriteLine($"データオフセット：{dataOffset}");
    Console.WriteLine($"情報ヘッダサイズ：{infoSize}");
    Console.WriteLine($"画像サイズ：{width} x {height}");
    Console.WriteLine($"プレーン数：{planeCount}");
    Console.WriteLine($"色ビット数：{bitCount}");
    Console.WriteLine($"圧縮形式：{compression}");
    Console.WriteLine($"画像データサイズ：{imageSize:N0}");
    Console.WriteLine($"水平解像度：{horizontalPixelPerMeter} ppi");
    Console.WriteLine($"垂直解像度：{verticalPixelPerMeter} ppi");
    Console.WriteLine($"格納パレット数：{usedColorCount}");
    Console.WriteLine($"重要色数：{importantColorCount}");
}
catch (FileNotFoundException e)
{
    Console.WriteLine($"{e.FileName} が見つかりません。");
    return (int)BmpInfoResult.FileNotFoundError;
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
    return (int)BmpInfoResult.OtherError;
}

return (int)BmpInfoResult.Success;

internal enum BmpInfoResult
{
    Success = 0,
    NotBmpError = -1,
    ArgumentError = -2,
    FileNotFoundError = -3,
    OtherError = -99,
}