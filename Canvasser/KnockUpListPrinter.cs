using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Canvasser
{
    public class KnockUpListPrinter
    {
        private readonly IEnumerable<TargetVoter> _targets;
        private readonly CanvasserDataContext _context;

        public KnockUpListPrinter(CanvasserDataContext context, IEnumerable<TargetVoter> targets)
        {
            _context = context;
            _targets = targets;
        }

        public void Print(string targetPD)
        {
            var electorsToPrint = _targets.Where(x => x.PD == targetPD && !x.Voted)
                .OrderBy(x => x.PN).ThenBy(x => x.PNs);

            var numberToPrint = electorsToPrint.Count();
            var numPages = (numberToPrint + 38) / 39;

            PrintDialog printDialog = new PrintDialog();
            if ((bool)printDialog.ShowDialog().GetValueOrDefault())
            {
                FlowDocument flowDocument = new FlowDocument();
                flowDocument.ColumnWidth = 700; // 96ths of an inch
                flowDocument.FontFamily = new FontFamily("Arial");
                for (var pageNo = 0; pageNo < numPages; pageNo++)
                {
                    Table table = new Table() { CellSpacing = 0, BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) };
                    if (pageNo > 0) table.BreakPageBefore = true;
                    table.Columns.Add(new TableColumn() { Width = new GridLength(50) });
                    table.Columns.Add(new TableColumn() { Width = new GridLength(200) });
                    table.Columns.Add(new TableColumn() { Width = new GridLength(170) });
                    table.Columns.Add(new TableColumn() { Width = new GridLength(80) });
                    table.Columns.Add(new TableColumn() { Width = new GridLength(380) });
                    table.RowGroups.Add(new TableRowGroup());
                    var numberInPage = Math.Min(numberToPrint - pageNo * 39, 39);
                    for (int i = 0; i < numberInPage; i++)
                    {
                        var elector = electorsToPrint.ElementAt(pageNo * 39 + i);

                        var tablerow = new TableRow();
                        tablerow.Cells.Add(new TableCell(new Paragraph(new Run(elector.FullNumber())) { Margin = new Thickness(2), FontSize = 12 }) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) });
                        tablerow.Cells.Add(new TableCell(new Paragraph(new Run(elector.FirstName + " " + elector.Surname)) { Margin = new Thickness(2), FontSize = 12 }) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) });
                        tablerow.Cells.Add(new TableCell(new Paragraph(new Run(elector.Address)) { Margin = new Thickness(2), FontSize = 12 }) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) });
                        tablerow.Cells.Add(new TableCell(new Paragraph(new Run(elector.Telephone)) { Margin = new Thickness(2), FontSize = 12 }) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) });
                        tablerow.Cells.Add(new TableCell(new Paragraph(new Run("  ")) { Margin = new Thickness(2) }) { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) });
                        table.RowGroups[0].Rows.Add(tablerow);
                    }

                    flowDocument.Blocks.Add(table);

                    Paragraph myParagraph = new Paragraph();
                    myParagraph.Margin = new Thickness(0);
                    var imprintProvider = new ImprintProvider(_context);
                    myParagraph.Inlines.Add(new Run(imprintProvider.Provide(targetPD)));
                    flowDocument.Blocks.Add(myParagraph);
                }

                DocumentPaginator paginator = ((IDocumentPaginatorSource)flowDocument).DocumentPaginator;
                printDialog.PrintDocument(paginator, "Title");
            }
        }
    }

    public static class TargetVoterExtensions
    {
        public static string FullNumber(this TargetVoter target)
        {
            return target.PD + target.PN + ((target.PNs > 0) ? "/" + target.PNs : "");
        }
    }
}
