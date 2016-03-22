using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Drawing;
using Microsoft.Office.Interop.Excel;
using TriageMaster.Windows;


namespace TriageMaster.Common
{
    /// <summary>
    /// Class for generator of Excel file
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    public class ExportToExcel<T, U>
        where T : class
        where U : List<T>
    {
        #region [Properties]

        public List<T> dataToPrint;

        #endregion

        #region [Local Variables]

        // Excel object references.
        private Microsoft.Office.Interop.Excel.Application excelApp = null;
        private Microsoft.Office.Interop.Excel.Workbooks books = null;
        private Microsoft.Office.Interop.Excel._Workbook book = null;
        private Microsoft.Office.Interop.Excel.Sheets sheets = null;
        private Microsoft.Office.Interop.Excel._Worksheet sheet = null;
        private Microsoft.Office.Interop.Excel.Range range = null;
        private Microsoft.Office.Interop.Excel.Font font = null;
        // Optional argument variable
        private object optionalValue = Missing.Value;

        #endregion

        #region [Public Methods]

        /// <summary>
        /// Generate report and sub functions
        /// </summary>
        public void GenerateReport()
        {
            try
            {
                if (dataToPrint != null)
                {
                    if (dataToPrint.Count != 0)
                    {
                        Mouse.SetCursor(System.Windows.Input.Cursors.Wait);
                        CreateExcelRef();
                        FillSheet();
                        OpenReport();
                        Mouse.SetCursor(System.Windows.Input.Cursors.Arrow);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.CaptureException(ex);
                Log.LogFileWrite();
            }
            finally
            {
                ReleaseObject(sheet);
                ReleaseObject(sheets);
                ReleaseObject(book);
                ReleaseObject(books);
                ReleaseObject(excelApp);
            }
        }

        #endregion

        #region [Private Methods]

        /// <summary>
        /// Make Microsoft Excel application visible
        /// </summary>
        private void OpenReport()
        {
            excelApp.Visible = true;
        }
        /// <summary>
        /// Populate the Excel sheet
        /// </summary>
        private void FillSheet()
        {
            object[] header = CreateHeader();
            WriteData(header);
        }
        /// <summary>
        /// Write data into the Excel sheet
        /// </summary>
        /// <param name="header"></param>
        private void WriteData(object[] header)
        {
            object[,] objData = new object[dataToPrint.Count, header.Length];

            for (int j = 0; j < dataToPrint.Count; j++)
            {
                var item = dataToPrint[j];
                for (int i = 0; i < header.Length; i++)
                {
                    var y = typeof(T).InvokeMember
            (header[i].ToString(), BindingFlags.GetProperty, null, item, null);
                    objData[j, i] = (y == null) ? "" : y.ToString();
                }
            }
            AddExcelRows("A2", dataToPrint.Count, header.Length, objData);
            AutoFitColumns("A1", dataToPrint.Count + 1, header.Length);

        }
        /// <summary>
        /// Method to make columns auto fit according to data
        /// </summary>
        /// <param name="startRange"></param>
        /// <param name="rowCount"></param>
        /// <param name="colCount"></param>
        private void AutoFitColumns(string startRange, int rowCount, int colCount)
        {
            range = sheet.get_Range(startRange, optionalValue);
            range = range.get_Resize(rowCount, colCount);
            range.ColumnWidth = 50;
            range.Columns.WrapText = true;
            range.Columns.AutoFit();            
           
            sheet.get_Range("A1:I1").Interior.Color = ColorTranslator.ToOle(System.Drawing.Color.DeepSkyBlue);
            sheet.Range["A1", "I" + rowCount].Borders.Color = Color.Black;

            // Range changed to A2 to start giving diffrent color to diffrent row depands upon the color present in the Work item grid.
            range = sheet.get_Range("A2", optionalValue);
            range = range.get_Resize(rowCount-1, colCount);
            
            int count = 0;
            foreach (Range row in range.Rows)
            {
                row.Interior.Color = GetRowColor((dataToPrint[count] as ExcelSheetHeaderName).CreatedDate);
                ++count;
            } 

        }
        /// <summary>
        /// Create header from the properties
        /// </summary>
        /// <returns></returns>
        private object[] CreateHeader()
        {
            PropertyInfo[] headerInfo = typeof(T).GetProperties();

            // Create an array for the headers and add it to the
            // worksheet starting at cell A1.
            List<object> objHeaders = new List<object>();
            for (int n = 0; n < headerInfo.Length; n++)
            {
                objHeaders.Add(headerInfo[n].Name);
            }
           
            //var headerToAdd = HeaderNames.ToArray();

            var headerToAdd = objHeaders.ToArray();
            AddExcelRows("A1", 1, headerToAdd.Length, headerToAdd);
            SetHeaderStyle();

            return headerToAdd;
        }
        /// <summary>
        /// Set Header style as bold
        /// </summary>
        private void SetHeaderStyle()
        {
            font = range.Font;
            font.Bold = true;            
        }
        /// <summary>
        /// Method to add an excel rows
        /// </summary>
        /// <param name="startRange"></param>
        /// <param name="rowCount"></param>
        /// <param name="colCount"></param>
        /// <param name="values"></param>
        private void AddExcelRows
        (string startRange, int rowCount, int colCount, object values)
        {
            range = sheet.get_Range(startRange, optionalValue);
            range = range.get_Resize(rowCount, colCount);
            range.set_Value(optionalValue, values);
        }

        /// <summary>
        /// Create Excel application parameters instances
        /// </summary>
        private void CreateExcelRef()
        {
            excelApp = new Microsoft.Office.Interop.Excel.Application();
            books = (Microsoft.Office.Interop.Excel.Workbooks)excelApp.Workbooks;
            book = (Microsoft.Office.Interop.Excel._Workbook)(books.Add(optionalValue));
            sheets = (Microsoft.Office.Interop.Excel.Sheets)book.Worksheets;
            sheet = (Microsoft.Office.Interop.Excel._Worksheet)(sheets.get_Item(1));
            //sheets[2].Delete();
        }
        /// <summary>
        /// Release unused COM objects
        /// </summary>
        /// <param name="obj"></param>
        private void ReleaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                Log.CaptureException(ex);
                Log.LogFileWrite();
            }
            finally
            {
                GC.Collect();
            }
        }

        private Color GetRowColor(string dateTime)
        {
            string rowColor = "White";
            try
            {
                //Forcing today's Datetime and the work item created date to Convert in {0:MM/dd/yyyy} format.
                
                TimeSpan differentInNumberOfDays = System.DateTime.Parse(DateTime.Now.ToString("MM/dd/yyyy"), System.Globalization.CultureInfo.InvariantCulture).Date - System.DateTime.Parse(string.Format("{0:MM/dd/yyyy}", dateTime), System.Globalization.CultureInfo.InvariantCulture).Date;

                if (Helper.TFSConnectionSettings.PreferenceColorName1 == string.Empty)
                    return Color.FromName(rowColor);;

                if (Helper.TFSConnectionSettings.PreferenceColorName2 == string.Empty)
                    return Color.FromName(rowColor);

                if (Helper.TFSConnectionSettings.PreferenceColorName3 == string.Empty)
                    return Color.FromName(rowColor);


                if (differentInNumberOfDays.Days <= int.Parse(Helper.TFSConnectionSettings.PreferenceInterval1))   //Bug/Defect Created 10 days back. 
                {
                    rowColor = Helper.TFSConnectionSettings.PreferenceColorName1;

                    if (Helper.TFSConnectionSettings.PreferenceColorName1.Contains(" "))
                        rowColor = Helper.TFSConnectionSettings.PreferenceColorName1.Replace(" ", "");

                    return Color.FromName(rowColor);
                }
                else if (differentInNumberOfDays.Days <= int.Parse(Helper.TFSConnectionSettings.PreferenceInterval2) &&
                    differentInNumberOfDays.Days > int.Parse(Helper.TFSConnectionSettings.PreferenceInterval1))   //Bug/Defect Created 20 days back. 
                {
                    rowColor = Helper.TFSConnectionSettings.PreferenceColorName2;

                    if (Helper.TFSConnectionSettings.PreferenceColorName2.Contains(" "))
                        rowColor = Helper.TFSConnectionSettings.PreferenceColorName2.Replace(" ", "");

                    return Color.FromName(rowColor);
                }
                else
                {
                    rowColor = Helper.TFSConnectionSettings.PreferenceColorName3;
                    if (Helper.TFSConnectionSettings.PreferenceColorName3.Contains(" "))
                        rowColor = Helper.TFSConnectionSettings.PreferenceColorName3.Replace(" ", "");

                    return Color.FromName(rowColor);
                }
            }
            catch (Exception)
            {
                return Color.FromName(rowColor);
            }
        }

        #endregion
    }
}
