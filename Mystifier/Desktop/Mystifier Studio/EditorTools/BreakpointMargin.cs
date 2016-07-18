using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Utils;

namespace Mystifier.EditorTools
{
    internal class BreakPointMargin : AbstractMargin
    {
        private const int margin = 20;
        private BookmarkManager bookmarkManager;

        public BreakPointMargin()
        {
            bookmarkManager = new BookmarkManager();
            bookmarkManager.Bookmarks = new List<IBookmark>();
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            return new PointHitTestResult(this, hitTestParameters.HitPoint);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return new Size(margin, 0);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            Size renderSize = this.RenderSize;
            drawingContext.DrawRectangle(SystemColors.ControlDarkDarkBrush, null,
                                         new Rect(0, 0, renderSize.Width, renderSize.Height));
            drawingContext.DrawLine(new Pen(SystemColors.ControlDarkBrush, 1),
                                    new Point(renderSize.Width - 0.5, 0),
                                    new Point(renderSize.Width - 0.5, renderSize.Height));

            TextView textView = this.TextView;
            if (textView != null && textView.VisualLinesValid)
            {
                // create a dictionary line number => first bookmark
                Dictionary<int, IBookmark> bookmarkDict = new Dictionary<int, IBookmark>();
                foreach (IBookmark bm in bookmarkManager.Bookmarks)
                {
                    int line = bm.LineNumber;
                    IBookmark existingBookmark;
                    if (!bookmarkDict.TryGetValue(line, out existingBookmark) || bm.ZOrder > existingBookmark.ZOrder)
                        bookmarkDict[line] = bm;
                }
                Size pixelSize = PixelSnapHelpers.GetPixelSize(this);
                foreach (VisualLine line in textView.VisualLines)
                {
                    int lineNumber = line.FirstDocumentLine.LineNumber;
                    IBookmark bm;
                    if (bookmarkDict.TryGetValue(lineNumber, out bm))
                    {
                        double lineMiddle = line.GetTextLineVisualYPosition(line.TextLines[0], VisualYPosition.TextMiddle) - textView.VerticalOffset;
                        Rect rect = new Rect(0, PixelSnapHelpers.Round(lineMiddle - 8, pixelSize.Height), 16, 16);
                        if (dragDropBookmark == bm && dragStarted)
                            drawingContext.PushOpacity(0.5);
                        drawingContext.DrawImage(BookmarkBase.DefaultBookmarkImage.Source, rect);
                        if (dragDropBookmark == bm && dragStarted)
                            drawingContext.Pop();
                    }
                }
                if (dragDropBookmark != null && dragStarted)
                {
                    Rect rect = new Rect(0, PixelSnapHelpers.Round(dragDropCurrentPoint - 8, pixelSize.Height), 16, 16);
                    drawingContext.DrawImage(BookmarkBase.DefaultBookmarkImage.Source, rect);
                }
            }
        }

        private IBookmark dragDropBookmark; // bookmark being dragged (!=null if drag'n'drop is active)
        private double dragDropStartPoint;
        private double dragDropCurrentPoint;
        private bool dragStarted; // whether drag'n'drop operation has started (mouse was moved minimum distance)
    }
}