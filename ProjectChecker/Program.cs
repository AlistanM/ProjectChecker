using ProjectChecker.Data;
using ProjectChecker.Services;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

class Program
{
    static void Main(string[] args)
    {

        var path = "C:\\Users\\Консультант1\\source\\repos\\ProjectChecker\\ProjectChecker\\Projects";
        var toPDFservice = new ToPDFService();
        var readService = new ReadingPDFService();

        string[] projects = Directory.GetDirectories(path);
        List<Application> projectApps = new List<Application>();

        foreach (string project in projects)
        {

            toPDFservice.DockToPDF(project);
            

            //бежим по документам с расширением pdf
            Console.WriteLine($"   Чтение pdf файлов: ");
            string[] pdfFiles = Directory.GetFiles(project, "*.pdf");

            foreach (string file in pdfFiles)
            {
                readService.readPDF(file, projectApps);
            }
        }

        var i = 1;
        foreach(var application in  projectApps)
        {
            Console.WriteLine($"{i}: {application.Name11}");
            i++;
        }
    }
}