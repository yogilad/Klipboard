using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.ComponentModel.DataAnnotations;

namespace Klipboard.Utils
{
    public static class ClipboardHelper
    {
        public static bool TryDetectTabularTextFormat(string data, out char seperator)
        {
            seperator = '\0';
            if (string.IsNullOrWhiteSpace(data))
            {
                return false;
            }

            int pos = 0;
            int lastTabs = 0;
            int lastComas = 0;

            while (ProcessNextLine(data, pos, out pos, out var lineLength, out var comas, out var tabs))
            {
                if (lineLength == 0)
                    continue;

                lastComas = (lastComas == 0) ? comas : lastComas;
                lastComas = (lastComas != comas) ? -1 : lastComas;

                lastTabs = (lastTabs == 0) ? tabs : lastTabs;
                lastTabs = (lastTabs != tabs) ? -1 : lastTabs;

                if (lastComas == -1 && lastTabs == -1)
                {
                    return false;
                }
            }

            if (lastTabs > 0)
            {
                seperator = '\t';
            }
            else if (lastComas > 0)
            {
                seperator = ',';
            }
            else
            {
                return false;
            }

            return true;
        }

        private static bool ProcessNextLine(string data, int pos, out int newPos, out int length, out int comas, out int tabs)
        {
            comas = 0;
            tabs = 0;
            length = 0;

            while (pos < data.Length)
            {
                var curChar = data[pos];
                switch (curChar)
                {
                    case '\t':
                        tabs++;
                        break;

                    case ',':
                        comas++;
                        break;
                }

                pos++;
                if (curChar != '\r')
                {
                    length++;
                }
                
                if (curChar == '\n')
                {
                    break;
                }
            }

            newPos = pos;
            return pos != data.Length;
        }
    }
}
