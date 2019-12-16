using AutoFixture;
using NUnit.Framework;
using System.Linq;

namespace Monzo.Crawler.Business.Tests
{
	public class HtmlParserTests
	{
		private IFixture _fixture;

		private HtmlParser _subject;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_fixture = new Fixture();
		}

		[SetUp]
		public void SetUp()
		{
			_subject = _fixture.Create<HtmlParser>();
		}

		[Test]
		[TestCase("/")]
		[TestCase("/foo")]
		[TestCase("/foo/bar")]
		[TestCase("/foo/bar?asd=123")]
		[TestCase("https://localhost/foo/bar?asd=123")]
		public void ShouldReturnHrefInAnchor(string href)
		{
			// Arrange
			var html = $"<a href='{href}'></a>";

			// Act
			var result = _subject.GetAnchors(html);

			// Assert
			Assert.That(result.Count(), Is.EqualTo(1));

			Assert.That(result.Single().Href, Is.EqualTo(href));
		}

		[Test]
		public void WhenMultipleAnchors_ShouldReturnHrefsInAnchors()
		{
			// Arrange
			var expectedHref1 = _fixture.Create<string>();
			var expectedHref2 = _fixture.Create<string>();

			var html = $"<a href='{expectedHref1}'></a><a href='{expectedHref2}'></a>";

			// Act
			var result = _subject.GetAnchors(html);

			// Assert
			Assert.That(result.Count(), Is.EqualTo(2));
			Assert.That(result.Any(x => x.Href == expectedHref1), Is.True);
			Assert.That(result.Any(x => x.Href == expectedHref2), Is.True);
		}

		[Test]
		public void WhenAnchorsHaveEmptyHrefs_ShouldNotReturnThem()
		{
			// Arrange
			var html = $"<a href=' '></a><a href=''></a>";

			// Act
			var result = _subject.GetAnchors(html);

			// Assert
			Assert.That(result, Is.Empty);
		}

		[Test]
		public void WhenHtmlIsNull_ShouldReturnNull()
		{
			// Arrange, Act
			var result = _subject.GetAnchors(null);

			// Assert
			Assert.That(result, Is.Null);
		}

		[Test]
		public void WhenArgumentHasNoHtmlTags_ShouldReturnEmptyList()
		{
			// Arrange, Act
			var result = _subject.GetAnchors(_fixture.Create<string>());

			// Assert
			Assert.That(result, Is.Empty);
		}

		[Test]
		public void WhenHtmlContainsNoAnchors_ShouldReturnEmptyList()
		{
			// Arrange
			var html = @"<div>foo</div>";

			// Act
			var result = _subject.GetAnchors(html);

			// Assert
			Assert.That(result, Is.Empty);
		}

		[Test]
		public void WhenAnchorTagHasNoHref_ShouldNotReturnIt()
		{
			// Arrange
			var html = @"<a>foo</a>";

			// Act
			var result = _subject.GetAnchors(html);

			// Assert
			Assert.That(result, Is.Empty);
		}
	}
}
