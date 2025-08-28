using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.File;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Utils;
using NodaTime;
using Plugin.Media.Abstractions;
using SkiaSharp;
using Topten.RichTextKit;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Classes
{
    public class WatermarkGenerator : IWatermarkGenerator
    {
        private readonly IFileService _fileService;

        public WatermarkGenerator()
        {
            _fileService = DependencyService.Get<IFileService>();
        }

        public async Task<string> GeneratePictureProofWatermark(string path, string itemName, LocalDateTime? localDateTime = null)
        {
            localDateTime ??= DateTimeHelper.Now;
            string pictureProofFilename = string.Format(Constants.PictureProofFilenameFormat, localDateTime.Value.ToFileTime());

            using var img = SKImage.FromEncodedData(path);
            using var bitmap = SKBitmap.FromImage(img);
            using var canvas = new SKCanvas(bitmap);


            var dateTime = localDateTime.Value.ToString("dd-MM-yyyy HH:mm", CultureInfo.CurrentUICulture);

            var translatedBy = TranslateExtension.GetValueFromDictionary(LanguageConstants.taskDetailMarkedBy);
            var dateAndUserString = $"{dateTime} {translatedBy} {UserSettings.Fullname}";

            var textColor = new SKColor(255, 255, 255, 155);

            var textSize = bitmap.Width / 40; //looks ok



            var rs = new RichString();

            rs.MaxWidth = bitmap.Width;
            rs.DefaultAlignment = Topten.RichTextKit.TextAlignment.Center;

            if (Settings.IsRightToLeftLanguage)
                rs.DefaultDirection = TextDirection.RTL;

            //task name
            rs.Add(itemName, "Arial", textSize, textColor: textColor)
                .MarginBottom(30);

            //date and user
            rs.Paragraph()
                .Add(dateAndUserString, fontSize: textSize + 10, textColor: textColor);

            var xPoint = 0;
            var yPoint = bitmap.Height * 0.75f;

            rs.Paint(canvas, new SKPoint(xPoint, yPoint));
            canvas.Flush();

            var image = SKImage.FromBitmap(bitmap);
            var pngImage = image.Encode();

            return await AsyncAwaiter.AwaitResultAsync(pictureProofFilename, async () =>
            {
                var pictureUrl = await _fileService.SaveFileToInternalStorageAsync(pngImage.ToArray(), pictureProofFilename, Constants.PictureProofsDirectory);
                return pictureUrl;
            });
        }
    }
}
