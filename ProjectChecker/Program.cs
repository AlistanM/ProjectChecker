using ProjectChecker.Data;
using ProjectChecker.Services;
using System.Reflection;

class Program
{
    static void Main(string[] args)
    {

        var path = "C:\\Users\\Консультант1\\source\\repos\\ProjectChecker\\ProjectChecker\\Projects";
        var toPDFservice = new ToPDFService();
        var readService = new ReadingPDFService();

        string[] projects = Directory.GetDirectories(path);
        List<Project> projectApps = new List<Project>();



        foreach (string project in projects)
        {

            toPDFservice.DocxToPDF(project);


            //бежим по документам с расширением pdf
            Console.WriteLine($"   Чтение pdf файлов: ");
            string[] pdfFiles = Directory.GetFiles(project, "*.pdf");

            foreach (string file in pdfFiles)
            {
                readService.readPDF(file, projectApps);
            }
        }

        var i = 1;
        foreach (var application in projectApps)
        {
            if (application == null) continue;

            Type type = application.GetType();

            foreach (PropertyInfo property in type.GetProperties())
            {
                // Получаем имя свойства
                string name = property.Name;

                // Получаем значение свойства
                object value = property.GetValue(application);

                // Выводим имя и значение свойства
                Console.WriteLine($"{name}: {value}");
            }
        }


    }
}