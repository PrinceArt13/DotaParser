using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Word = Microsoft.Office.Interop.Word;
using Excel = Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.Word;
using DotaParser.Models.Entities;
using DotaParser.Models;
using Microsoft.Office.Interop.Excel;
using DotaParser.Models.ViewModels;
using System.Windows;

namespace DotaParser
{
    class ReportGenerator
    {
        private static ReportGenerator? instance;
        public static ReportGenerator GetInstance()
        {
            instance ??= new ReportGenerator();
            return instance;
        }

        public void GenerateReport(object ShablonFile, object SaveAsFile)
        {
            //открываем excel
            Excel.Application excelApp = new Excel.Application();
            Excel.Workbook workbook = excelApp.Workbooks.Add();
            Excel.Worksheet worksheet = workbook.Sheets.Add();

            //массивы параметров героев
            Hero[] heroes = GetHeroes();
            if (heroes == null)
            {
                MessageBox.Show("Сначала заполните БД!");
                return;
            }
            string[] heroesNames = heroes.Select(x => x.Name).ToArray();
            int[] heroesHealth = heroes.Select(x => x.Health).ToArray();
            int[] heroesMana = heroes.Select(x => x.Health).ToArray();
            double[] heroesArmor = heroes.Select(x => x.Armor).ToArray();
            double[] heroesMagicResistance = heroes.Select(x => x.MagicResistance).ToArray();
            int[] heroesDamage = heroes.Select(x => x.Damage).ToArray();
            int[] heroesMoveSpeed = heroes.Select(x => x.Health).ToArray();

            CreateChart(worksheet, heroesNames, heroesHealth, "Здоровье", Excel.XlChartType.xlColumnClustered);
            CreateChart(worksheet, heroesNames, heroesMana, "Мана", Excel.XlChartType.xlLineMarkers);
            CreateChart(worksheet, heroesNames, heroesDamage, "Урон", Excel.XlChartType.xlBarClustered);
            CreateChart(worksheet, heroesNames, heroesMoveSpeed, "Скорость передвижения", Excel.XlChartType.xlLineMarkers);
            string excelFilePath = @"graphics.xlsx";
            workbook.SaveAs2(excelFilePath);
            workbook.Close();
            excelApp.Quit();

            //открываем word
            Word.Application wordApp = new();

            // Открываем документ
            Word.Document wDoc = wordApp.Documents.Add(ref ShablonFile, false, Word.WdNewDocumentType.wdNewBlankDocument, true);

            
            Excel.Workbook excelbook = excelApp.Workbooks.Open(excelFilePath);
            Excel.ChartObjects chartObjects = excelbook.Sheets["Лист2"].ChartObjects();
            foreach(ChartObject item in chartObjects)
            {
                Word.Range range1 = wDoc.Content.Paragraphs.Last.Range;
                item.Copy();
                range1.Paste();
                wDoc.Content.Paragraphs.Add();
                range1 = wDoc.Content.Paragraphs.Last.Range;
            }
            try
            {
                wDoc.SaveAs2(SaveAsFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            // Закрываем приложение 
            wordApp.Quit(Word.WdSaveOptions.wdPromptToSaveChanges);

        }

        private Hero[] GetHeroes()
        {
            using (var db = new dbContext())
            {
                Hero[] heroes = db.Heroes.OrderBy(x => x.Name).ToArray();
                foreach (Hero hero in heroes)
                {
                    heroes.Append(hero);
                }
                return heroes;
            }
        }
        private static void CreateChart(Excel.Worksheet worksheet, string[] xValues, int[] yValues, string chartTitle, Excel.XlChartType chartType)
        {
            Excel.ChartObjects chartObjects = worksheet.ChartObjects();
            Excel.ChartObject chartObject = chartObjects.Add(0, 0, 900, 300);
            Excel.Chart chart = chartObject.Chart;

            chart.ChartType = chartType;
            Excel.Range range = worksheet.Range[$"A1:B{xValues.Length}"];
            range.Value = new object[xValues.Length, 2];
            for (int i = 0; i < xValues.Length; i++)
            {
                range.Cells[i + 1, 1].Value = xValues[i];
                range.Cells[i + 1, 2].Value = yValues[i];
            }
            chart.SetSourceData(range);
            chart.HasTitle = true;
            chart.ChartTitle.Text = chartTitle;
        }
        private static void InsertChartIntoWord(Word.Document wordDoc, string excelFilePath, string chartTitle, float left, float top)
        {
            Word.Paragraph paragraph = wordDoc.Content.Paragraphs.Add();
            Word.InlineShape inlineShape = paragraph.Range.InlineShapes.AddOLEObject(
                ClassType: "Excel.Chart.12",
                FileName: excelFilePath,
                LinkToFile: false,
                DisplayAsIcon: false,
                IconFileName: ""
            );
            paragraph.Range.InsertParagraphAfter();
        }
    }
}
