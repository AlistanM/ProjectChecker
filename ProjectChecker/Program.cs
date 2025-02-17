
using Aspose.Words;
using System.IO;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

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

                    foreach (Page page in document.GetPages())
                    {
                        Console.WriteLine($"    Текст страницы {page.Number}:");
                        Console.WriteLine(page.Text);
                        Console.WriteLine(new string('-', 50));
                    }
                }
            }

        }




    }

}