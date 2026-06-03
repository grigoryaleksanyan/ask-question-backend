namespace AskQuestion.BLL.Email
{
    public static class EmailTemplateBuilder
    {
        public static EmailMessage BuildNewQuestionNotification(
            string toEmail,
            string toName,
            string questionText,
            string questionUrl)
        {
            string truncatedText = questionText.Length > 200
                ? questionText[..200] + "…"
                : questionText;

            string html = $"""
                <!DOCTYPE html>
                <html>
                <head><meta charset="utf-8"></head>
                <body style="margin:0;padding:0;background:#f5f5f5;font-family:Arial,sans-serif;">
                  <table width="100%" cellpadding="0" cellspacing="0" style="background:#f5f5f5;padding:32px 0;">
                    <tr><td align="center">
                      <table width="560" cellpadding="0" cellspacing="0" style="background:#ffffff;border-radius:8px;padding:32px;">
                        <tr><td style="font-size:20px;font-weight:600;color:#1a1a1a;">
                          Новый вопрос для вас
                        </td></tr>
                        <tr><td style="padding-top:16px;font-size:14px;color:#666;">
                          Здравствуйте, {toName}!
                        </td></tr>
                        <tr><td style="padding-top:16px;font-size:14px;color:#333;">
                          {truncatedText}
                        </td></tr>
                        <tr><td style="padding-top:24px;" align="center">
                          <a href="{questionUrl}" style="display:inline-block;padding:12px 24px;background:#4F6AF6;color:#ffffff;text-decoration:none;border-radius:6px;font-size:14px;font-weight:500;">
                            Перейти к вопросу
                          </a>
                        </td></tr>
                      </table>
                    </td></tr>
                  </table>
                </body>
                </html>
                """;

            return new EmailMessage
            {
                ToEmail = toEmail,
                ToName = toName,
                Subject = "Новый вопрос для вас",
                HtmlBody = html,
            };
        }

        public static EmailMessage BuildSpeakerCredentials(
            string toEmail,
            string toName,
            string password,
            string loginUrl)
        {
            string html = $"""
                <!DOCTYPE html>
                <html>
                <head><meta charset="utf-8"></head>
                <body style="margin:0;padding:0;background:#f5f5f5;font-family:Arial,sans-serif;">
                  <table width="100%" cellpadding="0" cellspacing="0" style="background:#f5f5f5;padding:32px 0;">
                    <tr><td align="center">
                      <table width="560" cellpadding="0" cellspacing="0" style="background:#ffffff;border-radius:8px;padding:32px;">
                        <tr><td style="font-size:20px;font-weight:600;color:#1a1a1a;">
                          Ваша учётная запись
                        </td></tr>
                        <tr><td style="padding-top:16px;font-size:14px;color:#666;">
                          Здравствуйте, {toName}!
                        </td></tr>
                        <tr><td style="padding-top:16px;font-size:14px;color:#333;">
                          Для вас создана учётная запись на платформе Ask Question.
                        </td></tr>
                        <tr><td style="padding-top:16px;">
                          <table cellpadding="0" cellspacing="0" style="background:#f0f0f0;border-radius:4px;padding:12px 16px;font-size:14px;">
                            <tr><td style="color:#666;padding-right:12px;">Email:</td><td style="color:#333;font-weight:500;">{toEmail}</td></tr>
                            <tr><td style="color:#666;padding-right:12px;padding-top:8px;">Пароль:</td><td style="color:#333;font-weight:500;">{password}</td></tr>
                          </table>
                        </td></tr>
                        <tr><td style="padding-top:24px;" align="center">
                          <a href="{loginUrl}" style="display:inline-block;padding:12px 24px;background:#4F6AF6;color:#ffffff;text-decoration:none;border-radius:6px;font-size:14px;font-weight:500;">
                            Войти
                          </a>
                        </td></tr>
                      </table>
                    </td></tr>
                  </table>
                </body>
                </html>
                """;

            return new EmailMessage
            {
                ToEmail = toEmail,
                ToName = toName,
                Subject = "Ваша учётная запись на Ask Question",
                HtmlBody = html,
            };
        }
    }
}
