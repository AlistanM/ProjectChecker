
using Aspose.Words;
using ProjectChecker.Data;
using System.IO;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using static System.Net.Mime.MediaTypeNames;

class Program
{
    static void Main(string[] args)
    {

        var path = "C:\\Users\\Консультант1\\source\\repos\\ProjectChecker\\ProjectChecker\\Projects";

        string[] projects = Directory.GetDirectories(path);
        foreach (string project in projects)
        {
            Console.WriteLine($"  Найден проект: {project}");
            //бежим по документам с расширение docx
            Console.WriteLine($"   Обработка docx файлов: ");
            string[] docxfiles = Directory.GetFiles(project, "*.docx");
            foreach (string file in docxfiles)
            {
                Console.WriteLine($"    Перевод документа {file} в pdf");

                // Генерируем имя файла
                string fileName = Path.GetFileNameWithoutExtension(file) + ".pdf";
                string filePath = Path.Combine(project, fileName);

                Document doc = new Document(file);
                doc.Save(filePath, SaveFormat.Pdf);
            }

            //бежим по документам с расширением pdf
            Console.WriteLine($"   Чтение pdf файлов: ");
            string[] pdfFiles = Directory.GetFiles(project, "*.pdf");
            foreach(string file in pdfFiles)
            {
                Console.WriteLine($"    Чтение {file} ");
                using (PdfDocument document = PdfDocument.Open(file))
                {
                    Console.WriteLine($"    Документ содержит {document.NumberOfPages} страниц.");

                    var firstPage = document.GetPage(1);
                    var type = 0;

                    if (firstPage.Text.Contains("ЗАЯВКА"))
                    {
                        type = 1;
                    }
                    else if (firstPage.Text.Contains("Единый государственный реестр недвижимости.") || firstPage.Text.Contains("Кадастровый номер"))
                    {
                        type = 2;
                    }
                    else if (firstPage.Text.Contains("сводной бюджетной росписи"))
                    {
                        type = 3;
                    }

                    foreach (Page page in document.GetPages())
                    {
                        var text = page.Text;

                        //Console.WriteLine($"    Текст страницы {page.Number}:");
                        //Console.WriteLine(page.Text);
                        //Console.WriteLine(new string('-', 50));

                        if (type == 1)
                        {
                            var application = new ProjectChecker.Data.Application();

                            var projectName11Pattern = @"1\.1\.\s+(?<ProjectName11>.+?)\s+\(";
                            Match projectName11Match = Regex.Match(text, projectName11Pattern);
                            application.Name11 = projectName11Match.Groups["ProjectName11"].Value;

                            var projectName12Pattern = @"1\.1\.\s+(?<ProjectName12>.+?)\s+\(";
                            Match projectName12Match = Regex.Match(text, projectName12Pattern);
                            application.Name12 = projectName12Match.Groups["ProjectName12"].Value;

                            var projectAddress22Pattern = @"2\.2\.\s+Населенный пункт \(адрес\):\s+(?<ProjectAddress22>.+?)(?=3\.\s+Описание проекта)";
                            Match projectAddress22Match = Regex.Match(text, projectAddress22Pattern);
                            application.Address22 = projectAddress22Match.Groups["ProjectAddress22"].Value;

                            var projectTotalCost33Pattern = @"Итого(?<TotalCost>.+?)(?=3\.4\.)";
                            Match projectTotalCost33Match = Regex.Match(text, projectTotalCost33Pattern);
                            application.TotalCost33 = projectTotalCost33Match.Groups["TotalCost"].Value;

                            Console.WriteLine(application.TotalCost33);
                        }
                        else if (type == 2)
                        {
                            Console.WriteLine("Выписка из ЕГРН.");
                        }
                        else if (type == 3)
                        {
                            Console.WriteLine("Выписка из сводной бюджетной росписи.");
                        }
                    }
                }
            }

        }




    }

}