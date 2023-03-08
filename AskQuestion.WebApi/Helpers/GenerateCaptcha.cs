using SkiaSharp;
using System.Text;

namespace AskQuestion.WebApi.Helpers
{
    public static class GenerateCaptcha
    {
        public static string GetCaptchaBase64(string capcthaText)
        {
            int height = 48;
            int width = 160;

            SKBitmap bitmap = new(width, height);

            using (SKPaint textPaint = new() { TextSize = 32 })
            {
                textPaint.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Inner, 4);

                using (SKCanvas bitmapCanvas = new(bitmap))
                {
                    bitmapCanvas.Clear();
                    bitmapCanvas.DrawText(capcthaText, 10, 35, textPaint);
                }
            }

            var resultImage = SKImage.FromBitmap(bitmap);
            var data = resultImage.Encode(SKEncodedImageFormat.Png, 100);

            return "data:image;base64," + Convert.ToBase64String(data.ToArray());
        }


        public static string GetCapcthaText()
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
