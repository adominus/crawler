using Monzo.Crawler.Domain;
using System;

namespace Monzo.Crawler
{
	public class ConsoleWriter : ITextWriter
    {
        public void WriteLine(string s)
        {
            Console.WriteLine(s);
        }
    }
}
