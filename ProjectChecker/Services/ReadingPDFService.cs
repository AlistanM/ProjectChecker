using ImageMagick;
using iText.Kernel.Pdf;
using System.Text.RegularExpressions;
using Tesseract;
using Application = ProjectChecker.Data.Application;

namespace ProjectChecker.Services
{
    public class ReadingPDFService
    {
        public void readPDF(string file, List<Application> projectApps)
        {
            var application = new Application();
            var hasImage = PdfHasImage(file);

            if (!hasImage)
            {
                var app = readTextPdf(file, application);
                projectApps.Add(app);
            }
            else
            {
                var app = readTextFromImagePdf(file, application);
                Console.WriteLine(app);
            }

        }


        private static bool PdfHasImage(string filePath)
        {
            using (PdfReader reader = new PdfReader(filePath))
            using (PdfDocument pdfDoc = new PdfDocument(reader))
            {
                for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
                {
                    var page = pdfDoc.GetPage(i);
                    var resources = page.GetResources();
                    var xObjects = resources.GetResource(PdfName.XObject);

                    if (xObjects != null)
                    {
                        foreach (var xObjectEntry in xObjects.EntrySet())
                        {
                            var obj = xObjectEntry.Value;
                            if (obj.IsStream())
                            {
                                var xObjStream = (PdfStream)obj;
                                var subtype = xObjStream.GetAsName(PdfName.Subtype);

                                if (PdfName.Image.Equals(subtype)) // Проверяем, является ли объект изображением
                                {
                                    //Console.WriteLine($"Изображение найдено на странице {i}");
                                    return true; // Возвращаем true, если изображение найдено
                                }
                            }
                        }
                    }
                }
            }
            return false; // Если ни на одной странице изображения не найдено
        }

        private Application readTextPdf(string file, Application application)
        {
            Console.WriteLine($"    Чтение {file} ");
            using (UglyToad.PdfPig.PdfDocument document = UglyToad.PdfPig.PdfDocument.Open(file))
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

                foreach (UglyToad.PdfPig.Content.Page page in document.GetPages())
                {
                    var text = page.Text;

                    //Console.WriteLine($"    Текст страницы {page.Number}:");
                    //Console.WriteLine(page.Text);
                    //Console.WriteLine(new string('-', 50));

                    if (type == 1)
                    {

                        var projectName11Pattern = @"1\.1\.\s+(?<ProjectName11>.+?)\s+\(";
                        Match projectName11Match = Regex.Match(text, projectName11Pattern);
                        if (projectName11Match.Success != false)
                        {
                            application.Name11 = projectName11Match.Groups["ProjectName11"].Value;
                        }

                        var projectName12Pattern = @"1\.1\.\s+(?<ProjectName12>.+?)\s+\(";
                        Match projectName12Match = Regex.Match(text, projectName12Pattern);
                        if (projectName12Match.Success != false)
                        {
                            application.Name12 = projectName12Match.Groups["ProjectName12"].Value;
                        }

                        var projectAddress22Pattern = @"2\.2\.\s+Населенный пункт \(адрес\):\s+(?<ProjectAddress22>.+?)(?=3\.\s+Описание проекта)";
                        Match projectAddress22Match = Regex.Match(text, projectAddress22Pattern);
                        if (projectAddress22Match.Success != false)
                        {
                            application.Address22 = projectAddress22Match.Groups["ProjectAddress22"].Value;
                        }

                        var projectTotalCost33Pattern = @"Итого(?<TotalCost>.+?)(?=3\.4\.)";
                        Match projectTotalCost33Match = Regex.Match(text, projectTotalCost33Pattern);
                        if (projectTotalCost33Match.Success != false)
                        {
                            application.TotalCost33 = projectTotalCost33Match.Groups["TotalCost"].Value;
                        }

                        var projectPartCost33Pattern = @"(?<=\s|\b)(?<Index>\d)(?!\d|\.)(?<WorkType>.+?)(?=\d|--)(?<Cost>\d{1,3}(?:\s\d{3})*(?:,\d{2})|-)";
                        var projectPartCost33Match = Regex.Matches(text, projectPartCost33Pattern);

                        int costIndex = 1; // Для присвоения значений в Cost1, Cost2 и т.д.
                        int descriptionIndex = 1; // Для присвоения значений в Description1, Description2 и т.д.
                        if (projectPartCost33Match.Count == 6)
                        {
                            foreach (Match match in projectPartCost33Match)
                            {
                                // Извлекаем группы из совпадения
                                var cost = match.Groups["Cost"].Value.Trim();
                                var workType = match.Groups["WorkType"].Value.Trim();

                                // Присваиваем стоимость в соответствующее свойство (Cost1, Cost2 и т.д.)
                                var costProperty = application.GetType().GetProperty($"Cost{costIndex}");
                                if (costProperty != null)
                                {
                                    costProperty.SetValue(application, cost);
                                }

                                // Присваиваем описание в соответствующее свойство (Description1, Description2 и т.д.)
                                var descriptionProperty = application.GetType().GetProperty($"Description{descriptionIndex}");
                                if (descriptionProperty != null)
                                {
                                    descriptionProperty.SetValue(application, workType);
                                }

                                // Увеличиваем индекс для следующего совпадения
                                costIndex++;
                                descriptionIndex++;
                            }
                        }

                        var projectPleaseMoneyPattern = @"(?<PleaseMoney>\d{1,3}(?:\s\d{3})*(?:,\d{1}).+?)(?=2Иной)";
                        Match projectPleaseMoneyMatch = Regex.Match(text, projectPleaseMoneyPattern);
                        if (projectPleaseMoneyMatch.Success != false)
                        {
                            application.MunicipalMoney4 = projectPleaseMoneyMatch.Groups["PleaseMoney"].Value;
                        }

                        var projectUdmurtMoneyPattern = @"(?<=не более 1 000 тыс\. рублей\))\s*\d{1,3}(?:\s\d{3})*,\d{2}";
                        Match projectUdmurtPleaseMoneyMatch = Regex.Match(text, projectUdmurtMoneyPattern);
                        if (projectUdmurtPleaseMoneyMatch.Success != false)
                        {
                            application.UdmurtMoney4 = projectUdmurtPleaseMoneyMatch.Value;
                        }

                        var projectTotalPleaseMoneyPattern = @"(?<=\D)(?<TotalMoney4>\d{1,3}(?:\s\d{3})*,\d{2})(?=5\.)";
                        Match projectTotalPleaseMoneyMatch = Regex.Match(text, projectTotalPleaseMoneyPattern);
                        if (projectTotalPleaseMoneyMatch.Success != false)
                        {
                            application.TotalCost4 = projectTotalPleaseMoneyMatch.Groups["TotalMoney4"].Value;
                        }
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

                return application;
            }
        }




        private string readTextFromImagePdf(string file, Application application)
        {
            var imagepath = ConvertPdfToImages(file);
            var text = PerformOCR(imagepath);

            return $"В файле {Path.GetFileName(file)} присутсвуют сканы";
        }

        private string ConvertPdfToImages(string file)
        {
            var directory = Path.GetDirectoryName(file);
            string outputDir = directory + "\\ImageToPdf";

            Directory.CreateDirectory(outputDir);
            using (MagickImageCollection images = new MagickImageCollection())
            {
                images.Read(file);

                int page = 1;
                foreach (MagickImage image in images)
                {
                    //проверить, переделать, понять, простить
                    image.Format = MagickFormat.Png;
                    image.Contrast();
                    image.Resize(1000, 1000);
                    image.Despeckle();

                    string outputFilePath = Path.Combine(outputDir, $"page-{page}.png");
                    image.Write(outputFilePath);
                    page++;
                }
            }

            return outputDir;
        }
        private static string PerformOCR(string imagePath)
        {
            string fullText = "";

            var images = Directory.GetFiles(imagePath);
            using (var engine = new TesseractEngine(@"C:\Users\Консультант1\source\repos\ProjectChecker\ProjectChecker\tesdata\", "rus", EngineMode.Default))
            {
                foreach (var image in images)
                {
                    using (var img = Pix.LoadFromFile(image))
                    {
                        using (var page = engine.Process(img))
                        {
                            fullText += page.GetText();
                        }
                    }
                }
            }
            return fullText;
        }
    }
}
