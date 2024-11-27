using GoogleSheetsToUnity;
using GoogleSheetsToUnity.Utils;
using GreenerGames;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// [fov-studio] Update 2024.04 <para/>
/// Google Sheets to Unity\Scripts\v4의 Spreadsheet.cs를 수정.
/// </summary>

namespace Fov_Studio
{
    [Serializable]
    public class GstuSpreadSheet
    {
        public Dictionary<string, GSTU_Cell> Cells = new Dictionary<string, GSTU_Cell>();

        public SecondaryKeyDictionary<string, List<GSTU_Cell>> columns = new SecondaryKeyDictionary<string, List<GSTU_Cell>>();
        public SecondaryKeyDictionary<int, string, List<GSTU_Cell>> rows = new SecondaryKeyDictionary<int, string, List<GSTU_Cell>>();

        public GstuSpreadSheet()
        {
        }

        public GstuSpreadSheet(GSTU_SpreadsheetResponce data, string titleColumn, int titleRow)
        {
            string startColumn = Regex.Replace(data.StartCell(), "[^a-zA-Z]", "");
            int startRow = int.Parse(Regex.Replace(data.StartCell(), "[^0-9]", ""));

            int startColumnAsInt = GoogleSheetsToUnityUtilities.NumberFromExcelColumn(startColumn);
            int currentRow = startRow;

            Dictionary<string, string> mergeCellRedirect = new Dictionary<string, string>();
            if (data.sheetInfo != null)
            {
                foreach (var merge in data.sheetInfo.merges)
                {
                    string cell = GoogleSheetsToUnityUtilities.ExcelColumnFromNumber(merge.startColumnIndex + 1) + (merge.startRowIndex + 1);

                    for (int r = merge.startRowIndex; r < merge.endRowIndex; r++)
                    {
                        for (int c = merge.startColumnIndex; c < merge.endColumnIndex; c++)
                        {
                            string mergeCell = GoogleSheetsToUnityUtilities.ExcelColumnFromNumber(c + 1) + (r + 1);
                            mergeCellRedirect.Add(mergeCell, cell);
                        }
                    }
                }
            }


            foreach (List<string> dataValue in data.valueRange.values)
            {
                int currentColumn = startColumnAsInt;

                foreach (string entry in dataValue)
                {
                    string realColumn = GoogleSheetsToUnityUtilities.ExcelColumnFromNumber(currentColumn);
                    string cellID = realColumn + currentRow;

                    GSTU_Cell cell = null;
                    if (mergeCellRedirect.ContainsKey(cellID) && Cells.ContainsKey(mergeCellRedirect[cellID]))
                    {
                        cell = Cells[mergeCellRedirect[cellID]];
                    }
                    else
                    {
                        cell = new GSTU_Cell(entry, realColumn, currentRow);

                        //check the title row and column exist, if not create them
                        if (!rows.ContainsKey(currentRow))
                        {
                            rows.Add(currentRow, new List<GSTU_Cell>());
                        }
                        if (!columns.ContainsPrimaryKey(realColumn))
                        {
                            columns.Add(realColumn, new List<GSTU_Cell>());
                        }

                        rows[currentRow].Add(cell);
                        columns[realColumn].Add(cell);


                        //build a series of seconard keys for the rows and columns
                        if (realColumn == titleColumn)
                        {
                            rows.LinkSecondaryKey(currentRow, cell.value);
                        }
                        if (currentRow == titleRow)
                        {
                            columns.LinkSecondaryKey(realColumn, cell.value);
                        }
                    }

                    Cells.Add(cellID, cell);

                    currentColumn++;
                }

                currentRow++;
            }

            //build the column and row string Id's from titles
            foreach (GSTU_Cell cell in Cells.Values)
            {
#if true // [fov-studio GoogleSheet Update]
                if (Cells.TryGetValue(cell.Column() + titleRow, out var cellData) == true)
                {
                    cell.columnId = cellData.value;
                    cell.rowId = Cells[titleColumn + cell.Row()].value;
                }
#else
            cell.columnId = Cells[cell.Column() + titleRow].value;
            cell.rowId = Cells[titleColumn + cell.Row()].value;
#endif
            }

#if true // [fov-studio GoogleSheet Udate]
            var e = Cells.GetEnumerator();
            while (e.MoveNext())
            {
                var key = e.Current.Key;
                if (!e.Current.Value.titleConnectedCells.Contains(key))
                {
                    e.Current.Value.titleConnectedCells.Add(key);
                }
            }
#else
        //build all links to row and columns for cells that are handled by merged title fields.
        foreach (GSTU_Cell cell in Cells.Values)
        {
            foreach (KeyValuePair<string, GSTU_Cell> cell2 in Cells)
            {
                if (cell.columnId == cell2.Value.columnId && cell.rowId == cell2.Value.rowId)
                {
                    if (!cell.titleConnectedCells.Contains(cell2.Key))
                    {
                        cell.titleConnectedCells.Add(cell2.Key);
                    }
                }
            }
        }
#endif
        }

        public GSTU_Cell this[string cellRef]
        {
            get
            {
                return Cells[cellRef];
            }
        }

        public GSTU_Cell this[string rowId, string columnId]
        {
            get
            {
                string columnIndex = columns.secondaryKeyLink[columnId];
                int rowIndex = rows.secondaryKeyLink[rowId];

                return Cells[columnIndex + rowIndex];
            }
        }

        public List<GSTU_Cell> this[string rowID, string columnID, bool mergedCells]
        {
            get
            {
                string columnIndex = columns.secondaryKeyLink[columnID];
                int rowIndex = rows.secondaryKeyLink[rowID];
                List<string> actualCells = Cells[columnIndex + rowIndex].titleConnectedCells;

                List<GSTU_Cell> returnCells = new List<GSTU_Cell>();
                foreach (string s in actualCells)
                {
                    returnCells.Add(Cells[s]);
                }

                return returnCells;
            }
        }
    }
}