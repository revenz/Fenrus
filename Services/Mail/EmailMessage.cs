using MailKit;
using MimeKit;
using SixLabors.ImageSharp.Formats.Webp;

namespace Fenrus.Services;

/// <summary>
/// Represents an email message with a subset of its properties for display in an inbox list.
/// </summary>
public class EmailMessage
{
    /// <summary>
    /// Gets or sets the unique identifier of the email message.
    /// </summary>
    public uint Uid { get; set; }

    /// <summary>
    /// Gets or sets the message identifier of the email message.
    /// </summary>
    public string MessageId { get; set; }

    /// <summary>
    /// Gets or sets the sender of the email message.
    /// </summary>
    public string From { get; set; }

    /// <summary>
    /// Gets or sets the recipient(s) of the email message.
    /// </summary>
    public string[] To { get; set; }

    /// <summary>
    /// Gets or sets the subject of the email message.
    /// </summary>
    public string Subject { get; set; }

    /// <summary>
    /// Gets or sets the date and time that the email message was sent, in UTC.
    /// </summary>
    public DateTime DateUtc { get; set; }

    /// <summary>
    /// Gets or sets the body of the email message.
    /// </summary>
    public string Body { get; set; }

    /// <summary>
    /// Gets or sest if the body is HTML
    /// </summary>
    public bool IsHtml { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the email message has attachments.
    /// </summary>
    public bool HasAttachments { get; set; }
    
    /// <summary>
    /// Gets or sets the message flags
    /// </summary>
    public MessageFlags? Flags { get; set; } 

    /// <summary>
    /// Creates a new <see cref="EmailMessage"/> object from an <see cref="IMessageSummary"/> object.
    /// </summary>
    /// <param name="summary">The <see cref="IMessageSummary"/> object to create the email message from.</param>
    /// <returns>A new <see cref="EmailMessage"/> object with the properties extracted from the <paramref name="summary"/> object.</returns>
    public static EmailMessage FromMessageSummary(IMessageSummary summary)
    {
        return new EmailMessage
        {
            Uid = summary.UniqueId.Id,
            MessageId = summary.Envelope.MessageId,
            From = summary.Envelope.From?.ToString() ?? string.Empty,
            To = summary.Envelope.To?.Select(a => a.ToString()).ToArray() ?? new string[] { },
            Subject = summary.Envelope.Subject ?? string.Empty,
            DateUtc = summary.Date.UtcDateTime,
            Flags = summary.Flags
        };
    }

    /// <summary>
    /// Converts a <see cref="MimeMessage"/> to an <see cref="EmailMessage"/>.
    /// </summary>
    /// <param name="message">The <see cref="MimeMessage"/> to convert.</param>
    /// <returns>The <see cref="EmailMessage"/> representation of the <see cref="MimeMessage"/>.</returns>
    public static async Task<EmailMessage> FromMimeMessage(MimeMessage message)
    {
        var emailMessage = new EmailMessage();

        if (message.Headers.Contains("X-Message-ID"))
            emailMessage.MessageId = message.Headers["X-Message-ID"];

        emailMessage.MessageId = emailMessage.MessageId?.EmptyAsNull() ?? message.MessageId;
        emailMessage.DateUtc = message.Date.UtcDateTime;
        emailMessage.From = message.From.ToString();
        emailMessage.To = message.To.Select(x => x.ToString()).ToArray();
        emailMessage.Subject = message.Subject;
        if(string.IsNullOrEmpty(message.HtmlBody))
            emailMessage.Body = message.TextBody ?? string.Empty;
        else
        {
            emailMessage.Body = CleanHtml(message);
            emailMessage.IsHtml = true;
        }

        emailMessage.HasAttachments = message.Attachments.Any();


        return emailMessage;
    }

    public static byte[] GetImageBytes(Image image, string contentType)
    {
        using (var stream = new MemoryStream())
        {
            image.Save(stream, new WebpEncoder());
            return stream.ToArray();
        }
    }

    private static string ImageToBase64Converted(Stream stream, string mimeType)
    {
        
        using (var image = Image.Load(stream))
        {
            // Resize the image if it's too large
            if (image.Width > 800 || image.Height > 800)
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(800, 800),
                    Mode = ResizeMode.Max
                }));
            }

            // Convert the image to a Base64-encoded stringar contentType = imageAttachment.ContentType;
            try
            {
                var bytes = GetImageBytes(image, mimeType);
                string base64String = Convert.ToBase64String(bytes);

                return base64String;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }

    public static string CleanHtml(MimeMessage message)
    {

        var html = message.HtmlBody;
        if (message.Body is Multipart multipart == false)
            return html;

        
        foreach (var part in message.BodyParts.OfType<MimePart>())
        {
            if (string.IsNullOrEmpty(part.ContentId))
                continue;
            if (part.ContentDisposition?.Disposition != ContentDisposition.Inline)
                continue;
            
            using var memoryStream = new MemoryStream();
            part.Content.DecodeTo(memoryStream);
            memoryStream.Position = 0;
            string base64String = ImageToBase64Converted(memoryStream, part.ContentType.MediaSubtype);
            var cid = part.ContentId.Trim('<', '>');
            html = html.Replace($"cid:{cid}", $"data:webp;base64,{base64String}");
        }

        return html;
    }
}
