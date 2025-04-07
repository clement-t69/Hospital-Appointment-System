using System.Net;
using System.Net.Mail;

namespace HealthApp.MVC.Models
{
    public class SendEmailModel
    {
        public SendEmailModel() { }

        public void SendCreateConfirmation(string firstname, string lastname, string email, string source)
        {
            if (source == "user")
            {
                try
                {
                    string passwordResetLink = "https://localhost:44368/account/forgot_password";

                    MailMessage message = new MailMessage();
                    message.From = new MailAddress("hospital.dorset@gmail.com");
                    message.To.Add(email);
                    message.Subject = "Account Created - Hospital Appointment System";
                    message.Body =
                        $@"
                        <!DOCTYPE html>
                        <html>
                        <head>
                            <h3>Account Created - Hospital Appointment System</h3>
                        </head>
                        <body>
                            <p>Dear {firstname} {lastname},</p>
                            <p>Your account has been created on the Hospital Appointment System. You can now log in to the system using your email ({email}) and your password, and start booking appointments.</p>                         
                            <p>To change your password, go on the 'My Account' page and click on 'Change your Password' or click <a href='{passwordResetLink}' target=''_blank"" rel='noopener noreferrer'>here</a></p>
                            <p>Hope you will enjoy using our website.</p>
                            <p>Regards,</p>
                            <p>The Hospital Appointment System team.</p>
                        </body>
                        </html>
                    ";
                    message.IsBodyHtml = true;

                    SmtpClient smtp = new SmtpClient("smtp.gmail.com")
                    {
                        Port = 587,
                        Credentials = new NetworkCredential("hospital.dorset@gmail.com", "mksw hfnp ykal htcr"),
                        EnableSsl = true
                    };

                    smtp.Send(message);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else if (source == "admin")
            {
                try
                {
                    string passwordResetLink = "https://localhost:44368/account/forgot_password";

                    MailMessage message = new MailMessage();
                    message.From = new MailAddress("hospital.dorset@gmail.com");
                    message.To.Add(email);
                    message.Subject = "Account Created - Hospital Appointment System";
                    message.Body =
                        $@"
                        <!DOCTYPE html>
                        <html>
                        <head>
                            <h3>Account Created - Hospital Appointment System</h3>
                        </head>
                        <body>
                            <p>Dear {firstname} {lastname},</p>
                            <p>Your account has been created on the Hospital Appointment System by an administrator. You can now log in to the system using your email ({email}) and your password, and start booking appointments.</p>                         
                            <p>To change your password, go on the 'My Account' page and click on 'Change your Password' or click <a href='{passwordResetLink}' target=''_blank"" rel='noopener noreferrer'>here</a></p>
                            <p>Hope you will enjoy using our website.</p>
                            <p>Regards,</p>
                            <p>The Hospital Appointment System team.</p>
                        </body>
                        </html>
                    ";
                    message.IsBodyHtml = true;

                    SmtpClient smtp = new SmtpClient("smtp.gmail.com")
                    {
                        Port = 587,
                        Credentials = new NetworkCredential("hospital.dorset@gmail.com", "mksw hfnp ykal htcr"),
                        EnableSsl = true
                    };

                    smtp.Send(message);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        public void SendEditConfirmation(string firstname, string lastname, string email, string source, string info)
        {
            string passwordResetLink = "https://localhost:44368/account/forgot_password";
            string contactUsLink = "https://localhost:44368/home/contact";

            if (source == "user")
            {
                if (info == "information")
                {
                    try
                    {
                        MailMessage message = new MailMessage();
                        message.From = new MailAddress("hospital.dorset@gmail.com");
                        message.To.Add(email);
                        message.Subject = "Edited Information - Hospital Appointment System";
                        message.Body =
                            $@"
                        <!DOCTYPE html>
                        <html>
                        <head>
                            <h3>Edited Information - Hospital Appointment System</h3>
                        </head>
                        <body>
                            <p>Dear {firstname} {lastname},</p>
                            <p>Your information has been updated.</p>
                            <p>If you did not make these changes, please reset your password by clicking <a href='{passwordResetLink}' target=''_blank"" rel='noopener noreferrer'>here</a> or <a href='{contactUsLink}' target=''_blank"" rel='noopener noreferrer'>contact us</a> immediately.</p>
                            <p>Regards,</p>
                            <p>The Hospital Appointment System team.</p>
                        </body>
                        </html>
                        ";
                        message.IsBodyHtml = true;

                        SmtpClient smtp = new SmtpClient("smtp.gmail.com")
                        {
                            Port = 587,
                            Credentials = new NetworkCredential("hospital.dorset@gmail.com", "mksw hfnp ykal htcr"),
                            EnableSsl = true
                        };

                        smtp.Send(message);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                else if (info == "email")
                {
                    try
                    {
                        MailMessage message = new MailMessage();
                        message.From = new MailAddress("hospital.dorset@gmail.com");
                        message.To.Add(email);
                        message.Subject = "Edited Email - Hospital Appointment System";
                        message.Body =
                            $@"
                        <!DOCTYPE html>
                        <html>
                        <head>
                            <h3>Edited Email - Hospital Appointment System</h3>
                        </head>
                        <body>
                            <p>Dear {firstname} {lastname},</p>
                            <p>Your Email has been updated.</p>
                            <p>If you did not make these changes, please <a href='{contactUsLink}' target=''_blank"" rel='noopener noreferrer'>contact us</a> immediately.</p>
                            <p>Regards,</p>
                            <p>The Hospital Appointment System team.</p>
                        </body>
                        </html>
                    ";
                        message.IsBodyHtml = true;

                        SmtpClient smtp = new SmtpClient("smtp.gmail.com")
                        {
                            Port = 587,
                            Credentials = new NetworkCredential("hospital.dorset@gmail.com", "mksw hfnp ykal htcr"),
                            EnableSsl = true
                        };

                        smtp.Send(message);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                else if (info == "password")
                {
                    try
                    {
                        MailMessage message = new MailMessage();
                        message.From = new MailAddress("hospital.dorset@gmail.com");
                        message.To.Add(email);
                        message.Subject = "Edited Password - Hospital Appointment System";
                        message.Body =
                            $@"
                        <!DOCTYPE html>
                        <html>
                        <head>
                            <h3>Edited Password - Hospital Appointment System</h3>
                        </head>
                        <body>
                            <p>Dear {firstname} {lastname},</p>
                            <p>Your Password has been updated.</p>
                            <p>If you did not make these changes, please reset your password by clicking <a href='{passwordResetLink}' target=''_blank"" rel='noopener noreferrer'>here</a> or <a href='{contactUsLink}' target=''_blank"" rel='noopener noreferrer'>contact us</a> immediately.</p>
                            <p>Regards,</p>
                            <p>The Hospital Appointment System team.</p>
                        </body>
                        </html>
                    ";
                        message.IsBodyHtml = true;

                        SmtpClient smtp = new SmtpClient("smtp.gmail.com")
                        {
                            Port = 587,
                            Credentials = new NetworkCredential("hospital.dorset@gmail.com", "mksw hfnp ykal htcr"),
                            EnableSsl = true
                        };

                        smtp.Send(message);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
            else if (source == "admin")
            {
                if (info == "information")
                {
                    try
                    {
                        MailMessage message = new MailMessage();
                        message.From = new MailAddress("hospital.dorset@gmail.com");
                        message.To.Add(email);
                        message.Subject = "Edited Information - Hospital Appointment System";
                        message.Body =
                            $@"
                        <!DOCTYPE html>
                        <html>
                        <head>
                            <h3>Edited Information - Hospital Appointment System</h3>
                        </head>
                        <body>
                            <p>Dear {firstname} {lastname},</p>
                            <p>Your information has been updated by an administrator.</p>
                            <p>If you need to revert these changes, please <a href='{contactUsLink}' target=''_blank"" rel='noopener noreferrer'>contact us</a>.</p>
                            <p>Regards,</p>
                            <p>The Hospital Appointment System team.</p>
                        </body>
                        </html>
                    ";
                        message.IsBodyHtml = true;

                        SmtpClient smtp = new SmtpClient("smtp.gmail.com")
                        {
                            Port = 587,
                            Credentials = new NetworkCredential("hospital.dorset@gmail.com", "mksw hfnp ykal htcr"),
                            EnableSsl = true
                        };

                        smtp.Send(message);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }            
            }
        }

        public void SendDeleteConfirmation(string firstname, string lastname, string email, string source)
        {
            string contactUsLink = "https://localhost:44368/home/contact";

            if (source == "user")
            {
                try
                {
                    MailMessage message = new MailMessage();
                    message.From = new MailAddress("hospital.dorset@gmail.com");
                    message.To.Add(email);
                    message.Subject = "Deleted Account - Hospital Appointment System";
                    message.Body =
                        $@"
                        <!DOCTYPE html>
                        <html>
                        <head>
                            <h3>Deleted Account - Hospital Appointment System</h3>
                        </head>
                        <body>
                            <p>Dear {firstname} {lastname},</p>
                            <p>Your account has been deleted.</p>
                            <p>If you are not at the origin of this action, <a href='{contactUsLink}' target=''_blank"" rel='noopener noreferrer'>contact us</a> immediately.</p>
                            <p>Regards,</p>
                            <p>The Hospital Appointment System team.</p>
                        </body>
                        </html>
                        ";
                    message.IsBodyHtml = true;

                    SmtpClient smtp = new SmtpClient("smtp.gmail.com")
                    {
                        Port = 587,
                        Credentials = new NetworkCredential("hospital.dorset@gmail.com", "mksw hfnp ykal htcr"),
                        EnableSsl = true
                    };

                    smtp.Send(message);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else if (source == "admin")
            {
                try
                {
                    MailMessage message = new MailMessage();
                    message.From = new MailAddress("hospital.dorset@gmail.com");
                    message.To.Add(email);
                    message.Subject = "Deleted Account - Hospital Appointment System";
                    message.Body =
                        $@"
                        <!DOCTYPE html>
                        <html>
                        <head>
                            <h3>Deleted Account - Hospital Appointment System</h3>
                        </head>
                        <body>
                            <p>Dear {firstname} {lastname},</p>
                            <p>Your account has been deleted by an administrator.</p>
                            <p>If you are not at the origin of this action, <a href='{contactUsLink}' target=''_blank"" rel='noopener noreferrer'>contact us</a> immediately.</p>
                            <p>Regards,</p>
                            <p>The Hospital Appointment System team.</p>
                        </body>
                        </html>
                        ";
                    message.IsBodyHtml = true;

                    SmtpClient smtp = new SmtpClient("smtp.gmail.com")
                    {
                        Port = 587,
                        Credentials = new NetworkCredential("hospital.dorset@gmail.com", "mksw hfnp ykal htcr"),
                        EnableSsl = true
                    };

                    smtp.Send(message);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

            }
        }

        public void SendForgotPassword(string firstname, string lastname, string email, string token, string callbackUrl)
        {
            try
            {
                MailMessage message = new MailMessage();
                message.From = new MailAddress("hospital.dorset@gmail.com");
                message.To.Add(email);
                message.Subject = "Reset your Password - Hospital Appointment System";
                message.Body =
                    $@"
                        <!DOCTYPE html>
                        <html>
                        <head>
                            <h3>Reset your Password - Hospital Appointment System</h3>
                        </head>
                        <body>
                            <p>Dear {firstname} {lastname},</p>
                            <p>You made a request to reset your password.</p>
                            <p>If you are not at the origin of this action, please ignore this email.</p>
                            <p>Else, you can reset your password by clicking <a href='{callbackUrl}' target=''_blank"" rel='noopener noreferrer'>here</a>.</p>
                            <p>Regards,</p>
                            <p>The Hospital Appointment System team.</p>
                        </body>
                        </html>
                        ";
                message.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("hospital.dorset@gmail.com", "mksw hfnp ykal htcr"),
                    EnableSsl = true
                };

                smtp.Send(message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void SendMessageEmail(string firstname, string lastname, string email) 
        {
            try
            {
                MailMessage message = new MailMessage();
                message.From = new MailAddress("hospital.dorset@gmail.com");
                message.To.Add(email);
                message.Subject = "New Message - Hospital Appointment System";
                message.Body =
                    $@"
                        <!DOCTYPE html>
                        <html>
                        <head>
                            <h3>New Message - Hospital Appointment System</h3>
                        </head>
                        <body>
                            <p>Dear {firstname} {lastname},</p>
                            <p>You just received a new message on the Hospital Appointment System. To read it, log in to your account by clicking <a href='https://localhost:44368/account/login' target=''_blank"" rel='noopener noreferrer'>here</a>.</p>                       
                            <p>Regards,</p>
                            <p>The Hospital Appointment System team.</p>
                        </body>
                        </html>
                    ";
                message.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("hospital.dorset@gmail.com", "mksw hfnp ykal htcr"),
                    EnableSsl = true
                };

                smtp.Send(message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    
        public void SendDisableAccount(string firstname, string lastname, string email)
        {
            try
            {
                MailMessage message = new MailMessage();
                message.From = new MailAddress("hospital.dorset@gmail.com");
                message.To.Add(email);
                message.Subject = "Account Disabled - Hospital Appointment System";
                message.Body =
                    $@"
                        <!DOCTYPE html>
                        <html>
                        <head>
                            <h3>Account Disabled - Hospital Appointment System</h3>
                        </head>
                        <body>
                            <p>Dear {firstname} {lastname},</p>
                            <p>Your account has been disabled. To reactivate it, please <a href='https://localhost:44368/home/contact' target=''_blank"" rel='noopener noreferrer'>contact us</a>.</p>
                            <p>Regards,</p>
                            <p>The Hospital Appointment System team.</p>
                        </body>
                        </html>
                    ";
                message.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("hospital.dorset@gmail.com", "mksw hfnp ykal htcr"),
                    EnableSsl = true
                };

                smtp.Send(message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void SendEnableAccount(string firstname, string lastname, string email)
        {
            try
            {
                MailMessage message = new MailMessage();
                message.From = new MailAddress("hospital.dorset@gmail.com");
                message.To.Add(email);
                message.Subject = "Account Disabled - Hospital Appointment System";
                message.Body =
                    $@"
                        <!DOCTYPE html>
                        <html>
                        <head>
                            <h3>Account Disabled - Hospital Appointment System</h3>
                        </head>
                        <body>
                            <p>Dear {firstname} {lastname},</p>
                            <p>Your account has been re-enabled. To reactivate it, please <a href='https://localhost:44368/home/contact' target=''_blank"" rel='noopener noreferrer'>contact us</a>.</p>
                            <p>Regards,</p>
                            <p>The Hospital Appointment System team.</p>
                        </body>
                        </html>
                    ";
                message.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("hospital.dorset@gmail.com", "mksw hfnp ykal htcr"),
                    EnableSsl = true
                };

                smtp.Send(message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
