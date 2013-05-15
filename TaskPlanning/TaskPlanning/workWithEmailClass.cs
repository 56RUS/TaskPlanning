using System;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.Windows.Forms;
using System.Collections.Generic;

namespace TaskPlanning
{
    // Состояние блокировки отправки сообщений
    enum enumStatesLockedSendMessage
    {
        unlocked,
        locked
    }

    // Структура каждого отправляемого письма
    public struct structEmailMessageProperties
    {
        public List<string> receiversAddres;                                        // Список получателей
        public string subject;                                                      // Тема письма
        public string message;                                                      // Содержание письма
    }


    public class workWithEmailClass
    {
        private string[] emailAddresesToSendsAllTasks = { };                        // Список адресов, на которые отправляются ВСЕ письма (на адреса, указанные в данном массиве, всегда будут отправляться все письма)
        private string emailLoginName = "OctavianTaskPlanningNotifier@gmail.com";   // Логин почтового акаунта
        private string emailPassword = "Octavian_Pass_56_w0rd_Notifier";            // Пароль почтового акаунта
        private List<structEmailMessageProperties> listEmailMessages;               // Список отправляемых писем (письма выстраиваются в очередь)
        private MailMessage myMailMessage;                                          // Через эту переменную отправляется каждое письмо
        private SmtpClient mySmtpClient;                                            // SMTP, используемый при отправлении письма
        private int isLockedSendMessage;                                            // Состояние блокировки отправления писем
        

        public workWithEmailClass()
        {
            listEmailMessages = new List<structEmailMessageProperties>();

            // Настройка свойств, используемых при отправлении письма
            myMailMessage = new MailMessage();
            myMailMessage.From = new MailAddress (emailLoginName, "Octavian Notifier", Encoding.UTF8);
            myMailMessage.SubjectEncoding = Encoding.UTF8;
            myMailMessage.BodyEncoding = Encoding.UTF8;

            // Настройка SMTP для отправления писем
            mySmtpClient = new SmtpClient();
            mySmtpClient.Host = "smtp.gmail.com";                                   // Адрес SMTP сервера
            mySmtpClient.Port = 587;                                                // Порт SMTP сервера
            mySmtpClient.EnableSsl = true;                                          // Использовать SSL
            mySmtpClient.Credentials = new NetworkCredential(emailLoginName, emailPassword);
            //mySmtpClient.Host = "mail.octavianonline.com";
            //mySmtpClient.Port = 587;

            isLockedSendMessage = (int)enumStatesLockedSendMessage.unlocked;
        }

        // Добавление письма в очередь отправляемых писем
        public void addMessageToSendQueue(List<string> receiversEmailAddres, string subject, string message)
        {
            structEmailMessageProperties prepareEmailMessage = new structEmailMessageProperties();

            // Если у письма пустая тема, пустое тело, список получателей = null или список текущих получателей и список постоянных получателей пусты, ТО выходим из функции
            if ((subject == "") || (message == "") || (receiversEmailAddres == null) || ((receiversEmailAddres.Count == 0) && (emailAddresesToSendsAllTasks.Length == 0)))
                return;

            prepareEmailMessage.subject = subject;
            prepareEmailMessage.message = message;
            prepareEmailMessage.receiversAddres = receiversEmailAddres;

            listEmailMessages.Add(prepareEmailMessage);                                 // Добавляю письмо в список
        }

        // Отправление сообщений
        public void sendMessage()
        {
            int i = 0;

            // Если отправление писем заблокировано, то выходим из функции
            if (isLockedSendMessage == (int)enumStatesLockedSendMessage.locked)
                return;

            // Блокирую отправление писем (для того, чтобы не возникло ситуации, что несколько потоков отправляют одни и те же письма)
            isLockedSendMessage = (int)enumStatesLockedSendMessage.locked;

            // Пока список отправляемых писем не пуст (т.е. для каждого письма из списка отправляемых писем):
            while (listEmailMessages.Count > 0)
            {
                // Добавляю в адресаты те адреса, на которые должны приходить ВСЕ письма
                for (i = 0; i < emailAddresesToSendsAllTasks.Length; i++)
                {
                    if (emailAddresesToSendsAllTasks[i] != "")
                        myMailMessage.To.Add(emailAddresesToSendsAllTasks[i]);
                }

                // Добавляю в адресаты адреса для текущего письма
                for (int listReceiversCount = 0; listReceiversCount < listEmailMessages[0].receiversAddres.Count; listReceiversCount++)
                {
                    // Если список адресатов, которым отправляются ВСЕ письма не пуст, то
                    // для того, чтобы не было ситуации, что один и тот же адрес попадет в список адресатов дважды
                    if (emailAddresesToSendsAllTasks.Length > 0)
                    {
                        // Сравниваю текущий адрес почты с каждым адресом из списка, на который отправляются ВСЕ письма
                        for (i = 0; i < emailAddresesToSendsAllTasks.Length; i++)
                        {
                            // Если адресат для текущего письма совпал с адресатом, которому отправляются ВСЕ письма, то выхожу из цикла
                            if ((listEmailMessages[0].receiversAddres[listReceiversCount] == emailAddresesToSendsAllTasks[i]))
                                break;
                        }

                        // Если счетчик равен длине списка адресатов, которым отправляются ВСЕ письма,
                        // т.е. адресат для текущего письма не присутствует в списке адресатов, которым отправляются все письма,
                        // добавляю адрес в список адресатов
                        if (i == emailAddresesToSendsAllTasks.Length)
                            myMailMessage.To.Add(listEmailMessages[0].receiversAddres[listReceiversCount]);
                    }
                    else
                        myMailMessage.To.Add(listEmailMessages[0].receiversAddres[listReceiversCount]);     // Добавляю адрес в список адресатов
                }

                // Если для письма число адресатов больше нуля
                if (myMailMessage.To.Count > 0)
                {
                    // Подготавливаю отправляемое письмо
                    myMailMessage.Subject = listEmailMessages[0].subject;
                    myMailMessage.Body = listEmailMessages[0].message;

                    try
                    {
                        mySmtpClient.Send(myMailMessage);                               // Инициирую отправление письма через SNTP
                        listEmailMessages.RemoveAt(0);                                  // Если письмо удачно отправлено (не было исключений) удаляю письмо из списка отправляемых писем
                        //MessageBox.Show("Message sended!", "Success send message!");
                    }
                    catch (Exception ex)                        // Если при отправлении письма произошло исключение, то перехватываю его
                    {
                        MessageBox.Show("Error: " + ex.Message, "Error in send message!");
                    }

                    myMailMessage.To.Clear();                                           // Удаляю адресатов
                    myMailMessage.Subject = "";                                         // Удаляю тему письма
                    myMailMessage.Body = "";                                            // Удаляю тело письма
                }
            }

            // Снимаю блокировку отправления писем
            isLockedSendMessage = (int)enumStatesLockedSendMessage.unlocked;
        }
    }
}
