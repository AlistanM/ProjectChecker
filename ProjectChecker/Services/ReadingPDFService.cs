using ImageMagick;
using iText.Kernel.Pdf;
using ProjectChecker.Data;
using System.Linq;
using System.Text.RegularExpressions;
using Tesseract;
using Application = ProjectChecker.Data.Application;

namespace ProjectChecker.Services
{
    public class ReadingPDFService
    {
        public void readPDF(string file, Project project)
        {
            var application = new Application();
            var egrn = new EGRN();
            var budget = new BudgetStatement();

            var app = readTextPdf(file, project, application, egrn, budget);
            
        }

        private void readTextPdf(string file, Project project, Application application, EGRN egrn, BudgetStatement budget)
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
                else if (firstPage.Text.Contains("Выписка") || firstPage.Text.Contains("сводной бюджетной росписи"))
                {
                    type = 3;
                }

                foreach (UglyToad.PdfPig.Content.Page page in document.GetPages())
                {
                    var text = page.Text;

                    Console.WriteLine($"    Текст страницы {page.Number}:");
                    Console.WriteLine(page.Text);
                    Console.WriteLine(new string('-', 50));

                    if (type == 1)
                    {

                        var projectName11Pattern = @"1\.1(\.| )\s+(?<ProjectName11>.+?)\s+\(";
                        Match projectName11Match = Regex.Match(text, projectName11Pattern);
                        if (projectName11Match.Success != false)
                        {
                            project.application.Name11 = projectName11Match.Groups["ProjectName11"].Value;
                        }

                        var projectName12Pattern = @"1\.2\.\s+(?<ProjectName12>.+?)\s+\(";
                        Match projectName12Match = Regex.Match(text, projectName12Pattern);
                        if (projectName12Match.Success != false)
                        {
                            project.application.Name12 = projectName12Match.Groups["ProjectName12"].Value;
                        }

                        var projectAddress22Pattern = @"2\.2\.\s+Населенный пункт \(адрес\):\s+(?<ProjectAddress22>.+?)(?=3\.\s+Описание проекта)";
                        Match projectAddress22Match = Regex.Match(text, projectAddress22Pattern);
                        if (projectAddress22Match.Success != false)
                        {
                            project.application.Address22 = projectAddress22Match.Groups["ProjectAddress22"].Value;
                        }

                        var projectTotalCost33Pattern = @"Итого(?<TotalCost>.+?)(?=3\.4\.)";
                        Match projectTotalCost33Match = Regex.Match(text, projectTotalCost33Pattern);
                        if (projectTotalCost33Match.Success != false)
                        {
                            project.application.TotalCost33 = projectTotalCost33Match.Groups["TotalCost"].Value;
                        }

                        var projectPartCost33Pattern = @"(?<=\s|\b)(?<Index>\d)(?!\d|\.)(?<WorkType>.+?)(?=\d|--)(?<Cost>\d{1,3}(?:\s\d{3})*(?:,\d{2})|-)";
                        var projectPartCost33Match = Regex.Matches(text, projectPartCost33Pattern);

                        int costIndex = 1; // Для присвоения значений в Cost1, Cost2 и т.д.
                        int descriptionIndex = 1; // Для присвоения значений в Description1, Description2 и т.д.
                        if (projectPartCost33Match.Count >= 6)
                        {
                            foreach (Match match in projectPartCost33Match)
                            {
                                // Извлекаем группы из совпадения
                                var cost = match.Groups["Cost"].Value.Trim();
                                var workType = match.Groups["WorkType"].Value.Trim();

                                // Присваиваем стоимость в соответствующее свойство (Cost1, Cost2 и т.д.)
                                var costProperty = project.application.GetType().GetProperty($"Cost{costIndex}");
                                if (costProperty != null && costIndex <= 6)
                                {
                                    costProperty.SetValue(application, cost);
                                }

                                // Присваиваем описание в соответствующее свойство (Description1, Description2 и т.д.)
                                var descriptionProperty = project.application.GetType().GetProperty($"Description{descriptionIndex}");
                                if (descriptionProperty != null && descriptionIndex <= 6)
                                {
                                    descriptionProperty.SetValue(application, workType);
                                }

                                // Увеличиваем индекс для следующего совпадения
                                costIndex++;
                                descriptionIndex++;
                            }
                        }

                        var projectPleaseMoneyPattern = @"проекта\)(?<PleaseMoney>\d{1,3}(?:\s\d{3})*(?:,\d{1}).+?)(?=2Иной)";
                        Match projectPleaseMoneyMatch = Regex.Match(text, projectPleaseMoneyPattern);
                        if (projectPleaseMoneyMatch.Success != false)
                        {
                            project.application.MunicipalMoney4 = projectPleaseMoneyMatch.Groups["PleaseMoney"].Value;
                        }

                        var projectUdmurtMoneyPattern = @"(?<=не более 1 000 тыс\. рублей\))\s*\d{1,3}(?:\s\d{3})*,\d{2}";
                        Match projectUdmurtPleaseMoneyMatch = Regex.Match(text, projectUdmurtMoneyPattern);
                        if (projectUdmurtPleaseMoneyMatch.Success != false)
                        {
                            project.application.UdmurtMoney4 = projectUdmurtPleaseMoneyMatch.Value;
                        }

                        var projectTotalPleaseMoneyPattern = @"(?<=\D)(?<TotalMoney4>\d{1,3}(?:\s\d{3})*,\d{2})(?=5\.)";
                        Match projectTotalPleaseMoneyMatch = Regex.Match(text, projectTotalPleaseMoneyPattern);
                        if (projectTotalPleaseMoneyMatch.Success != false)
                        {
                            project.application.TotalCost4 = projectTotalPleaseMoneyMatch.Groups["TotalMoney4"].Value;
                        }
                    }
                    else if (type == 2)
                    {
                        var projectEgrnAddresPattern = @"Местоположение:(?<Addres>.+?)(?=Площадь)";
                        Match projectEgrnAddresMatch = Regex.Match(text, projectEgrnAddresPattern);
                        if (projectEgrnAddresMatch.Success != false)
                        {
                            project.egrn.Address = projectEgrnAddresMatch.Groups["Addres"].Value;
                        }

                        var projectEgrnRightHolderPattern = @"\(правообладатели\):(?<RightHolder>.+?)(?=Сведения)";
                        var projectEgrnRightHolderMatch = Regex.Matches(text, projectEgrnRightHolderPattern);
                        foreach (Match match in projectEgrnRightHolderMatch)
                        {
                            // Извлекаем группы из совпадения
                            var rightHolder = match.Groups["RightHolder"].Value.Trim();

                            project.egrn.RightHolder.Add(rightHolder);
                        }

                        var projectRightPattern = @"права:(?<Right>.+?)(?=18)";
                        var projectRightHolderMatch = Regex.Matches(text, projectRightPattern);
                        foreach (Match match in projectRightHolderMatch)
                        {
                            // Извлекаем группы из совпадения
                            var right = match.Groups["Right"].Value.Trim();

                            project.egrn.Right.Add(right);
                        }
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
