using System;

namespace PublicConsultation.Infrastructure.Services;

public static class EmailTemplateHelper
{
    public static string GetConsultationNotificationHtml(string title, string ministry, string description, string viewAllUrl, string viewDocUrl)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.6;
            color: #333;
            margin: 0;
            padding: 0;
        }}
        .container {{
            max-width: 600px;
            margin: 20px auto;
            border: 1px solid #e0e0e0;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 4px 6px rgba(0,0,0,0.1);
        }}
        .header {{
            background-color: #594AE2;
            color: white;
            padding: 30px;
            text-align: center;
        }}
        .header h1 {{
            margin: 0;
            font-size: 24px;
            font-weight: 600;
        }}
        .content {{
            padding: 30px;
            background-color: #ffffff;
        }}
        .ministry {{
            color: #594AE2;
            font-weight: bold;
            text-transform: uppercase;
            font-size: 12px;
            letter-spacing: 1px;
            margin-bottom: 10px;
            display: block;
        }}
        .title {{
            font-size: 20px;
            font-weight: 700;
            margin-bottom: 20px;
            color: #1a1a1a;
        }}
        .description {{
            font-size: 15px;
            color: #555;
            margin-bottom: 30px;
        }}
        .button-container {{
            text-align: center;
            margin-bottom: 30px;
        }}
        .button {{
            background-color: #594AE2;
            color: white !important;
            padding: 12px 25px;
            text-decoration: none;
            border-radius: 5px;
            font-weight: 600;
            display: inline-block;
        }}
        .footer {{
            background-color: #f9f9f9;
            padding: 20px;
            text-align: center;
            font-size: 12px;
            color: #888;
            border-top: 1px solid #eeeeee;
        }}
        .accent {{
            color: #594AE2;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Public Consultation Notice</h1>
        </div>
        <div class='content'>
            <span class='ministry'>{ministry}</span>
            <div class='title'>{title}</div>
            <p class='description'>
                A new legislative draft has been published for public review and feedback. Your participation is vital in shaping our national policies.
            </p>
            
            <div class='button-container'>
                <a href='{viewDocUrl}' class='button'>Review Document & Give Feedback</a>
            </div>

            <p style='font-size: 14px;'>
                Alternatively, you can browse all active consultations on our portal: <br/>
                <a href='{viewAllUrl}' class='accent'>{viewAllUrl}</a>
            </p>
        </div>
        <div class='footer'>
            <p>&copy; {DateTime.Now.Year} Digital Public Consultation System. All rights reserved.</p>
            <p>You are receiving this because you registered for government transparency alerts.</p>
        </div>
    </div>
</body>
</html>";
    }
}
