using SkiaSharp;
using System.Text;

namespace AskQuestion.WebApi.Helpers
{
    public static class GenerateCaptcha
    {
        private const int CaptchaHeight = 38;
        private const int CaptchaWidth = 127;
        private const int FontSize = 24;
        private const float YBaseOffset = 8f;
        private const float YOffsetRange = 4f;
        private const float YOffsetMidpoint = 2f;
        private const float RotationRange = 20f;
        private const float RotationMidpoint = 10f;
        private const int CurveCountMin = 3;
        private const int CurveCountMax = 6;
        private const int DotCount = 40;
        private const int CurveColorMin = 80;
        private const int CurveColorMax = 160;
        private const int CurveAlphaMin = 40;
        private const int CurveAlphaMax = 80;
        private const int DotColorMin = 60;
        private const int DotColorMax = 180;
        private const int DotAlphaMin = 80;
        private const int DotAlphaMax = 160;
        private const int StrokeWidthMin = 1;
        private const int StrokeWidthMax = 3;
        private const int DotRadiusMin = 1;
        private const int DotRadiusMax = 3;
        public static string GetCaptchaBase64(string captchaText)
        {
            using var bitmap = new SKBitmap(CaptchaWidth, CaptchaHeight);
            using var font = new SKFont(SKTypeface.Default, FontSize);
            using var textPaint = new SKPaint
            {
                Color = new SKColor(100, 110, 120),
                IsAntialias = true,
            };

            using (var canvas = new SKCanvas(bitmap))
            {
                canvas.Clear(SKColors.White);

                float x = 8f;
                foreach (char c in captchaText)
                {
                    float charWidth = font.MeasureText(c.ToString());
                    float y = CaptchaHeight / 2f + YBaseOffset + (float)(Random.Shared.NextDouble() * YOffsetRange - YOffsetMidpoint);
                    float angle = (float)(Random.Shared.NextDouble() * RotationRange - RotationMidpoint);

                    canvas.Save();
                    canvas.Translate(x + charWidth / 2f, y);
                    canvas.RotateDegrees(angle);
                    canvas.DrawText(c.ToString(), -charWidth / 2f, 0, font, textPaint);
                    canvas.Restore();

                    x += charWidth + 2f;
                }

                for (int i = 0; i < Random.Shared.Next(CurveCountMin, CurveCountMax); i++)
                {
                    using var curvePaint = new SKPaint
                    {
                        Color = new SKColor(
                            (byte)Random.Shared.Next(CurveColorMin, CurveColorMax),
                            (byte)Random.Shared.Next(CurveColorMin, CurveColorMax),
                            (byte)Random.Shared.Next(CurveColorMin, CurveColorMax),
                            (byte)Random.Shared.Next(CurveAlphaMin, CurveAlphaMax)),
                        StrokeWidth = Random.Shared.Next(StrokeWidthMin, StrokeWidthMax),
                        Style = SKPaintStyle.Stroke,
                        IsAntialias = true,
                    };

                    using var path = new SKPath();
                    path.MoveTo(Random.Shared.Next(CaptchaWidth), Random.Shared.Next(CaptchaHeight));
                    path.CubicTo(
                        Random.Shared.Next(CaptchaWidth), Random.Shared.Next(CaptchaHeight),
                        Random.Shared.Next(CaptchaWidth), Random.Shared.Next(CaptchaHeight),
                        Random.Shared.Next(CaptchaWidth), Random.Shared.Next(CaptchaHeight));
                    canvas.DrawPath(path, curvePaint);
                }

                using var dotPaint = new SKPaint { IsAntialias = true };
                for (int i = 0; i < DotCount; i++)
                {
                    dotPaint.Color = new SKColor(
                        (byte)Random.Shared.Next(DotColorMin, DotColorMax),
                        (byte)Random.Shared.Next(DotColorMin, DotColorMax),
                        (byte)Random.Shared.Next(DotColorMin, DotColorMax),
                        (byte)Random.Shared.Next(DotAlphaMin, DotAlphaMax));
                    canvas.DrawCircle(
                        Random.Shared.Next(CaptchaWidth),
                        Random.Shared.Next(CaptchaHeight),
                        Random.Shared.Next(DotRadiusMin, DotRadiusMax),
                        dotPaint);
                }
            }

            using var resultImage = SKImage.FromBitmap(bitmap);
            using var data = resultImage.Encode(SKEncodedImageFormat.Png, 100);

            return "data:image;base64," + Convert.ToBase64String(data.ToArray());
        }

        public static string GetCaptchaText()
        {
            string combinations = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            StringBuilder captcha = new();

            for (int i = 0; i < 6; i++)
            {
                captcha.Append(combinations[Random.Shared.Next(combinations.Length)]);
            }

            return captcha.ToString();
        }
    }
}
