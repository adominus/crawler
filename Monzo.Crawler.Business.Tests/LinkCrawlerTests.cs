using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Monzo.Crawler.Business.Tests
{
	public class LinkCrawlerTests
	{
		private IFixture _fixture;

		private LinkCrawler _subject;
		private Mock<IHttpClientService> _httpClientServiceMock;
		private Mock<IHtmlParser> _htmlParserMock;

		private Uri _address;
		private string _body;
		private List<Anchor> _anchors;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_fixture = new Fixture()
				.Customize(new AutoMoqCustomization());

			_address = new Uri("http://localhost/");

			_httpClientServiceMock = _fixture.Freeze<Mock<IHttpClientService>>();
			_htmlParserMock = _fixture.Freeze<Mock<IHtmlParser>>();
		}

		[SetUp]
		public void SetUp()
		{
			_body = _fixture.Create<string>();
			_httpClientServiceMock.Setup(x => x.GetHtmlBody(_address))
				.ReturnsAsync(() => _body);

			_anchors = new List<Anchor>();
			_htmlParserMock.Setup(x => x.GetAnchors(_body))
				.Returns(() => _anchors);

			_subject = _fixture.Create<LinkCrawler>();
		}

		[TearDown]
		public void TearDown()
		{
			_httpClientServiceMock.Reset();
			_htmlParserMock.Reset();
		}

		[Test]
		public async Task ShouldRequestHtmlBody()
		{
			// Arrange, Act
			await _subject.FindLinksOnSameDomain(_address);

			// Assert
			_httpClientServiceMock.Verify(x => x.GetHtmlBody(_address));
		}

		[Test]
		public async Task ShouldParseAnchorsFromBody()
		{
			// Arrange, Act
			await _subject.FindLinksOnSameDomain(_address);

			// Assert
			_htmlParserMock.Verify(x => x.GetAnchors(_body));
		}

		[Test]
		public async Task WhenBodyIsNull_ShouldReturnEmptyResult()
		{
			// Arrange
			_body = null;

			// Act
			var result = await _subject.FindLinksOnSameDomain(_address);

			// Assert
			Assert.That(result, Is.Empty);
		}

		[Test]
		public async Task WhenAnchorsAreNull_ShouldReturnEmptyResult()
		{
			// Arrange
			_anchors = null;

			// Act
			var result = await _subject.FindLinksOnSameDomain(_address);

			// Assert
			Assert.That(result, Is.Empty);
		}

		[Test]
		[TestCase("foo", "http://localhost/foo")]
		[TestCase("/foo/bar", "http://localhost/foo/bar")]
		public async Task WhenAnchorHrefsAreRelative_ShouldCreateFromExpectedBase(string href, string expectedAddress)
		{
			// Arrange
			_anchors.Add(new Anchor { Href = href });

			// Act
			var results = await _subject.FindLinksOnSameDomain(_address);

			// Assert
			Assert.That(results.Single().AbsoluteUri, Is.EqualTo(expectedAddress));
		}

		[Test]
		[TestCase("http://localhost/foo/", "http://localhost/foo")]
		[TestCase("http://localhost/foo/bar/", "http://localhost/foo/bar")]
		[TestCase("foo/bar/", "http://localhost/foo/bar")]
		public async Task ShouldRemoveTrailingSlash(string href, string expectedAddress)
		{
			// Arrange
			_anchors.Add(new Anchor { Href = href });

			// Act
			var results = await _subject.FindLinksOnSameDomain(_address);

			// Assert
			Assert.That(results.Single().AbsoluteUri, Is.EqualTo(expectedAddress));
		}

		[Test]
		[TestCase("http://localhost/foo", "http://localhost/foo")]
		[TestCase("http://localhost/foo/bar", "http://localhost/foo/bar")]
		[TestCase("HTTP://LOCALHOST/foo/bar", "http://localhost/foo/bar")]
		public async Task WhenAnchorHrefsAreAbsoluteAndMatchDomain_ShouldReturnAsUri(string href, string expectedAddress)
		{
			// Arrange
			_anchors.Add(new Anchor { Href = href });

			// Act
			var results = await _subject.FindLinksOnSameDomain(_address);

			// Assert
			Assert.That(results.Single().AbsoluteUri, Is.EqualTo(expectedAddress));
		}

		[Test]
		[TestCase("http://subdomain.localhost/foo")]
		[TestCase("http://monzo.com")]
		public async Task WhenAnchorHrefsAreAbsoluteAndDoNotMatchDomain_ShouldReturnEmpty(string href)
		{
			// Arrange
			_anchors.Add(new Anchor { Href = href });

			// Act
			var results = await _subject.FindLinksOnSameDomain(_address);

			// Assert
			Assert.That(results, Is.Empty);
		}

		[Test]
		[TestCase("foo?asd=123", "http://localhost/foo")]
		[TestCase("/foo/bar?q=a#baz", "http://localhost/foo/bar")]
		[TestCase("http://localhost/foo/bar?q=a#baz", "http://localhost/foo/bar")]
		public async Task WhenAnchorHrefsHaveQueryStringsAndFragments_ShouldSanitizeToPathAndHost(string href, string expectedAddress)
		{
			// Arrange
			_anchors.Add(new Anchor { Href = href });

			// Act
			var results = await _subject.FindLinksOnSameDomain(_address);

			// Assert
			Assert.That(results.Single().AbsoluteUri, Is.EqualTo(expectedAddress));
		}

		[Test]
		public async Task WhenMultipleAnchorsReturns_ShouldReturnMultipleAddresses()
		{
			// Arrange
			var address1 = new Uri(_address, _fixture.Create<string>());
			var address2 = new Uri(_address, _fixture.Create<string>());
			var address3 = new Uri(_address, _fixture.Create<string>());

			_anchors.Add(new Anchor { Href = address1.AbsoluteUri });
			_anchors.Add(new Anchor { Href = address2.AbsoluteUri });
			_anchors.Add(new Anchor { Href = address3.AbsoluteUri });

			// Act
			var results = await _subject.FindLinksOnSameDomain(_address);

			// Assert
			Assert.That(results.Count(), Is.EqualTo(3));
			Assert.That(results.Any(x => x.AbsoluteUri == address1.AbsoluteUri), Is.True);
			Assert.That(results.Any(x => x.AbsoluteUri == address2.AbsoluteUri), Is.True);
			Assert.That(results.Any(x => x.AbsoluteUri == address3.AbsoluteUri), Is.True);
		}

		[Test]
		[TestCase("foo", "foo?asd=123", "http://localhost/foo")]
		[TestCase("http://localhost/foo", "foo?asd=123", "http://localhost/foo")]
		[TestCase("http://localhost/foo", "http://localhost/foo/", "http://localhost/foo")]
		[TestCase("http://localhost/foo", "http://localhost/foo", "http://localhost/foo")]
		[TestCase("http://localhost/foo/bar", "foo/bar?asd=123#asd", "http://localhost/foo/bar")]
		public async Task WhenMultipleAnchorsReturnSameValue_ShouldReturnSingleAddresses(string href1, string href2, string expectedAddress)
		{
			// Arrange
			_anchors.Add(new Anchor { Href = href1 });
			_anchors.Add(new Anchor { Href = href2 });

			// Act
			var results = await _subject.FindLinksOnSameDomain(_address);

			// Assert
			Assert.That(results.Single().AbsoluteUri, Is.EqualTo(expectedAddress));
		}
	}
}
