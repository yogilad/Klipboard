using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Test.Utils;

namespace Klipboard.Utils.Test
{
    [TestClass]
    public class TableGeneratorTests
    {
        [TestMethod]
        public void GenerateStrings()
        {
            var tg = new TableGenerator(autoGenerateScheme: true);
            var scheme = tg.GenerateTableScheme();
            var t1 = tg.GenerateTableString(10, addHeader: true, addNullRows: true, addEmptyRows: true);
            var t2 = tg.GenerateTableString(3, addHeader: false, addNullRows: false, addEmptyRows: false);

            Assert.IsFalse(string.IsNullOrWhiteSpace(scheme));
            Assert.IsFalse(string.IsNullOrWhiteSpace(t1));
            Assert.IsFalse(string.IsNullOrWhiteSpace(t2));
        }

        [TestMethod]
        public void GenerateStreams()
        {
            var tg = new TableGenerator(autoGenerateScheme: true);
            var scheme = tg.GenerateTableScheme();
            var t1 = new StreamReader(tg.GenerateTableStream(10, addHeader: true, addNullRows: true, addEmptyRows: true)).ReadToEnd();
            var t2 = new StreamReader(tg.GenerateTableStream(3, addHeader: false, addNullRows: false, addEmptyRows: false)).ReadToEnd();

            Assert.IsFalse(string.IsNullOrWhiteSpace(scheme));
            Assert.IsFalse(string.IsNullOrWhiteSpace(t1));
            Assert.IsFalse(string.IsNullOrWhiteSpace(t2));
        }
    }
}
