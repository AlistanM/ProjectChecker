using Aspose.Words;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectChecker.Services
{
    public class ToPDFService
    {
        public void DockToPDF(string filePath)
        {
            Console.WriteLine($"  Найден проект: {filePath}");
            //бежим по документам с расширение docx
            Console.WriteLine($"   Обработка docx файлов: ");
            string[] docxfiles = Directory.GetFiles(filePath, "*.docx");
            foreach (string file in docxfiles)
            {
                Console.WriteLine($"    Перевод документа {file} в pdf");

                // Генерируем имя файла
                string fileName = Path.GetFileNameWithoutExtension(file) + ".pdf";
                string newFilePath = Path.Combine(filePath, fileName);

                Document doc = new Document(file);
                doc.Save(newFilePath, SaveFormat.Pdf);
            }
        }
    }
}
