﻿using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using itext.pdfimage.Extensions;
using itext.pdfimage.Models;
using System;
using System.Drawing;

namespace iText.Kernel.Pdf.Canvas.Parser.Listener
{
    public class TextListener : LocationTextExtractionStrategy
    {
        private readonly SortedDictionary<float, IChunk> _chunkDictionairy;
        private readonly Func<float> _increaseCounter;

        public TextListener(SortedDictionary<float, IChunk> chunkDictionairy, Func<float> increaseCounter)
        {
            _chunkDictionairy = chunkDictionairy;
            _increaseCounter = increaseCounter;
        }

        public override void EventOccurred(IEventData data, EventType type)
        {
            if (!type.Equals(EventType.RENDER_TEXT))
                return;

            var renderInfo = (TextRenderInfo)data;
            var counter = _increaseCounter();
            var font = renderInfo.GetFont().GetFontProgram();
            var originalFontName = font.ToString();
            var fontRegex = Regex.Match(originalFontName, @"(?<=\+)[a-zA-Z\s]+");
            var fontName = fontRegex.Success ? fontRegex.Value : originalFontName;
            var fontStyle = font.GetFontNames().GetFontStyle();
            var curFontSize = renderInfo.GetFontSize();
            var key = counter;

            IList<TextRenderInfo> text = renderInfo.GetCharacterRenderInfos();
            foreach (TextRenderInfo character in text)
            {
                key += 0.001f;

                var textRenderMode = character.GetTextRenderMode();
                var opacity = character.GetGraphicsState().GetFillOpacity();
                var letter = character.GetText();

                Color color;

                var fillColor = character.GetFillColor();
                var colors = fillColor.GetColorValue();

                switch (colors.Length)
                {
                    case 1:
                        color = Color.FromArgb((int)(255 * (1 - colors[0])), Color.Black);
                        break;
                    case 3:
                        color = Color.FromArgb((int)(255 * colors[0]), (int)(255 * colors[1]), (int)(255 * colors[2]));
                        break;
                    case 4:
                        color = Color.FromArgb((int)(255 * colors[0]), (int)(255 * colors[1]), (int)(255 * colors[2]), (int)(255 * colors[3]));
                        break;
                    default:
                        color = Color.Black;
                        break;
                }

                if (string.IsNullOrWhiteSpace(letter)) 
                    continue;

                //Get the bounding box for the chunk of text
                var bottomLeft = character.GetDescentLine().GetStartPoint();
                var topRight = character.GetAscentLine().GetEndPoint();

                //Create a rectangle from it
                var rect = new Geom.Rectangle
                (
                    bottomLeft.Get(Vector.I1),
                    topRight.Get(Vector.I2),
                    topRight.Get(Vector.I1),
                    topRight.Get(Vector.I2)
                );
                var currentChunk = new itext.pdfimage.Models.TextChunk()
                {
                    Text = letter,
                    Rect = rect,
                    FontFamily = fontName,
                    FontSize = (int)curFontSize,
                    FontStyle = fontStyle,
                    Color = color,
                    SpaceWidth = character.GetSingleSpaceWidth() / 2f
                };

                _chunkDictionairy.Add(key, currentChunk);
            }

            base.EventOccurred(data, type);
        }
    }
}
