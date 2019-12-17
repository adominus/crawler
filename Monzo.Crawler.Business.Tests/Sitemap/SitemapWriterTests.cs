using AutoFixture;
using Monzo.Crawler.Business.Sitemap;
using Monzo.Crawler.Domain;
using Monzo.Crawler.Domain.Sitemap;
using NUnit.Framework;
using System;
using System.Text;

namespace Monzo.Crawler.Business.Tests.Sitemap
{
	public class SitemapWriterTests
	{
		private IFixture _fixture;

		private SitemapWriter _subject;
		private FakeTextWriter _fakeTextWriter;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_fixture = new Fixture();

			_fakeTextWriter = new FakeTextWriter();
			_fixture.Inject<ITextWriter>(_fakeTextWriter);
		}

		[SetUp]
		public void SetUp()
		{
			_subject = _fixture.Create<SitemapWriter>();
		}

		[TearDown]
		public void TearDown()
		{
			_fakeTextWriter.Reset();
		}

		[Test]
		public void ShouldOutputPageAndLinks()
		{
			// Arrange 
			var page = new PageModel
			{
				Address = new Uri("http://localhost/"),
				LinkedAddresses = new[] { new Uri("http://localhost/foo") }
			};

			// Act
			_subject.Write(new[] { page });
			var result = _fakeTextWriter.GetOutput();

			// Assert
			Assert.That(result, Is.EqualTo(
@"http://localhost/
- 1 Link: 
	- http://localhost/foo
"));
		}

		[Test]
		public void ShouldOrderMultipleLinks()
		{
			// Arrange 
			var page = new PageModel
			{
				Address = new Uri("http://localhost/"),
				LinkedAddresses = new[]
				{
					new Uri("http://localhost/foo"),
					new Uri("http://localhost/bar/baz"),
					new Uri("http://localhost/bar"),
					new Uri("http://localhost/bar/qux")
				}
			};

			// Act
			_subject.Write(new[] { page });
			var result = _fakeTextWriter.GetOutput();

			// Assert
			Assert.That(result, Is.EqualTo(
@"http://localhost/
- 4 Links: 
	- http://localhost/bar
	- http://localhost/bar/baz
	- http://localhost/bar/qux
	- http://localhost/foo
"));
		}

		[Test]
		public void WhenNoLinks_ShouldWriteNoLinks()
		{
			// Arrange 
			var page = new PageModel { Address = new Uri("http://localhost/foo") };

			// Act
			_subject.Write(new[] { page });
			var result = _fakeTextWriter.GetOutput();

			// Assert
			Assert.That(result, Is.EqualTo(
@"http://localhost/foo
- No Links
"));
		}

		[Test]
		public void When2Pages_ShouldOrderByAddress()
		{
			// Arrange 
			var page1 = new PageModel { Address = new Uri("http://localhost/foo") };
			var page2 = new PageModel { Address = new Uri("http://localhost/bar") };

			// Act
			_subject.Write(new[] { page1, page2 });
			var result = _fakeTextWriter.GetOutput();

			// Assert
			Assert.That(result, Is.EqualTo(
@"http://localhost/bar
- No Links
http://localhost/foo
- No Links
"));
		}

		[Test]
		public void WhenAddressIsNull_ShouldSkipPage()
		{
			// Arrange 
			var page1 = new PageModel();
			var page2 = new PageModel { Address = new Uri("http://localhost/") };

			// Act
			_subject.Write(new[] { page1, page2 });
			var result = _fakeTextWriter.GetOutput();

			// Assert
			Assert.That(result, Is.EqualTo(
@"http://localhost/
- No Links
"));
		}
	}

	public class FakeTextWriter : ITextWriter
	{
		private StringBuilder _stringBuilder;

		public FakeTextWriter()
		{
			_stringBuilder = new StringBuilder();
		}

		public void Reset()
		{
			_stringBuilder = new StringBuilder();
		}

		public void WriteLine(string s)
		{
			_stringBuilder.Append(s + Environment.NewLine);
		}

		public string GetOutput()
		{
			return _stringBuilder.ToString();
		}
	}
}
