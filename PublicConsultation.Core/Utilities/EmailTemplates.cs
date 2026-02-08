using System;

namespace PublicConsultation.Core.Utilities;

public static class EmailTemplates
{
    public static string GetOtpEmail(string otp, int expirationMinutes)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Email Verification</title>
</head>
<body style='margin: 0; padding: 0; background-color: #f4f4f4; font-family: Arial, sans-serif;'>
    <table role='presentation' border='0' cellpadding='0' cellspacing='0' width='100%'>
        <tr>
            <td style='padding: 20px 0 30px 0;'>
                <table align='center' border='0' cellpadding='0' cellspacing='0' width='600' style='border-collapse: collapse; border: 1px solid #cccccc; border-radius: 8px; overflow: hidden; background-color: #ffffff;'>
                    <!-- Header -->
                    <tr>
                        <td align='center' bgcolor='#594AE2' style='padding: 30px 0 30px 0;'>
                           <h1 style='color: #ffffff; font-size: 24px; margin: 0; font-family: Arial, sans-serif;'>Public Consultation System</h1>
                        </td>
                    </tr>
                    <!-- Body -->
                    <tr>
                        <td bgcolor='#ffffff' style='padding: 40px 30px 40px 30px;'>
                            <table border='0' cellpadding='0' cellspacing='0' width='100%'>
                                <tr>
                                    <td style='color: #153643; font-family: Arial, sans-serif; font-size: 24px;'>
                                        <b>Verify Your Email Address</b>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='padding: 20px 0 30px 0; color: #153643; font-family: Arial, sans-serif; font-size: 16px; line-height: 24px;'>
                                        Hello,
                                        <br><br>
                                        Thank you for registering with the Public Consultation System. To complete your verification, please use the following One-Time Password (OTP).
                                        <br><br>
                                        This code is valid for <b>{expirationMinutes} minutes</b>.
                                    </td>
                                </tr>
                                <tr>
                                    <td align='center'>
                                        <div style='background-color: #f8f9fa; border: 1px solid #dee2e6; border-radius: 4px; padding: 15px; display: inline-block;'>
                                            <span style='font-size: 32px; font-weight: bold; letter-spacing: 5px; color: #594AE2; font-family: monospace;'>{otp}</span>
                                        </div>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='padding: 30px 0 0 0; color: #153643; font-family: Arial, sans-serif; font-size: 16px; line-height: 24px;'>
                                        If you did not request this verification, please ignore this email.
                                        <br><br>
                                        Best regards,<br>
                                        Public Consultation Team
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td bgcolor='#ee4c50' style='padding: 30px 30px 30px 30px; background-color: #333333;'>
                            <table border='0' cellpadding='0' cellspacing='0' width='100%'>
                                <tr>
                                    <td style='color: #ffffff; font-family: Arial, sans-serif; font-size: 14px;' width='75%'>
                                        &copy; {DateTime.Now.Year} Public Consultation System<br/>
                                        Government of the People's Republic of Bangladesh
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }
}
