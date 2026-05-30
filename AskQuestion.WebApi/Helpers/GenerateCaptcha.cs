using SkiaSharp;
using System.Text;

namespace AskQuestion.WebApi.Helpers
{
    public static class GenerateCaptcha
    {
        public static string GetCaptchaBase64(string captchaText)
        {
            int height = 48;
            int width = 160;

            using var bitmap = new SKBitmap(width, height);
            using var font = new SKFont(SKTypeface.Default, 32);
            using var paint = new SKPaint { MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Inner, 4) };

            float textWidth = font.MeasureText(captchaText);
            float x = (width - textWidth) / 2F;
            float y = 35;

            using (var bitmapCanvas = new SKCanvas(bitmap))
            {
                bitmapCanvas.Clear();
                bitmapCanvas.DrawText(captchaText, x, y, font, paint);
            }

            using var resultImage = SKImage.FromBitmap(bitmap);
            using var data = resultImage.Encode(SKEncodedImageFormat.Png, 100);

            return "data:image;base64," + Convert.ToBase64String(data.ToArray());
        }


        public static string GetCaptchaText()
        {
            string combinations = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

            Random random = new();
            StringBuilder captcha = new();

            for (int i = 0; i < 6; i++)
            {
                captcha.Append(combinations[random.Next(combinations.Length)]);
            }

            return captcha.ToString();
        }
    }
}
