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
	public class SitemapGeneratorTests
	{
		private IFixture _fixture;

		private SitemapGenerator _subject;
		private Mock<ILinkCrawler> _linkCrawlerMock;

		private string _website;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_fixture = new Fixture()
				.Customize(new AutoMoqCustomization());

			_website = "http://localhost/";

			_linkCrawlerMock = _fixture.Freeze<Mock<ILinkCrawler>>();
		}

		[SetUp]
		public void SetUp()
		{
			_subject = _fixture.Create<SitemapGenerator>();
		}

		[TearDown]
		public void TearDown()
		{
			_linkCrawlerMock.Reset();
		}

		[Test]
		public void Generate_WhenWebsiteUriIsNotValid_ShouldThrowException()
		{
			// Arrange, Act
			AsyncTestDelegate act = () => _subject.Generate(_fixture.Create<string>());

			// Assert 
			Assert.That(act, Throws.TypeOf<ArgumentException>());
		}

		[Test]
		public async Task ShouldFindLinksOnSameDomain()
		{
			// Arrange, Act
			await _subject.Generate(_website);

			// Assert 
			_linkCrawlerMock.Verify(x => x.FindLinksOnSameDomain(
				It.Is<Uri>(uri => uri.AbsoluteUri == _website)));
		}

		[Test]
		public async Task WhenNoLinksReturned_ShouldReturnSingleVisitedPage()
		{
			// Arrange, Act
			var result = await _subject.Generate(_website);

			// Assert 
			Assert.That(result.Count(), Is.EqualTo(1));
		}

		[Test]
		public async Task WhenNoLinksReturned_ShouldReturnPageMatchingOriginalHost()
		{
			// Arrange, Act
			var result = await _subject.Generate(_website);

			// Assert 
			Assert.That(result.Single().Address.AbsoluteUri, Is.EqualTo(_website));
		}

		[Test]
		public async Task WhenLinkCrawlerReturnsNull_ShouldReturnSingleVisitedPage()
		{
			// Arrange
			_linkCrawlerMock.Setup(x => x.FindLinksOnSameDomain(It.IsAny<Uri>()))
				.ReturnsAsync((IEnumerable<Uri>)null);

			// Act
			var result = await _subject.Generate(_website);

			// Assert 
			Assert.That(result.Count(), Is.EqualTo(1));
		}

		[Test]
		public async Task When1LinkReturned_ShouldVisitLinkedPageOnce()
		{
			// Arrange
			var expectedVisitedLink = _fixture.Create<Uri>();

			_linkCrawlerMock.Setup(x => x.FindLinksOnSameDomain(It.Is<Uri>(uri => uri.AbsoluteUri == _website)))
				.ReturnsAsync(new[] { expectedVisitedLink });

			// Act
			await _subject.Generate(_website);

			// Assert
			_linkCrawlerMock.Verify(x => x.FindLinksOnSameDomain(
				It.Is<Uri>(uri => uri.AbsoluteUri == expectedVisitedLink.AbsoluteUri)));
		}

		[Test]
		public async Task When1LinkReturned_ShouldReturn2Pages()
		{
			// Arrange
			_linkCrawlerMock.Setup(x => x.FindLinksOnSameDomain(It.Is<Uri>(uri => uri.AbsoluteUri == _website)))
				.ReturnsAsync(new[] { _fixture.Create<Uri>() });

			// Act
			var results = await _subject.Generate(_website);

			// Assert
			Assert.That(results.Count(), Is.EqualTo(2));
		}

		[Test]
		public async Task When1LinkReturned_ShouldList2ndPageAsLinkFromFirst()
		{
			// Arrange
			var expectedLink = _fixture.Create<Uri>();

			_linkCrawlerMock.Setup(x => x.FindLinksOnSameDomain(It.Is<Uri>(uri => uri.AbsoluteUri == _website)))
				.ReturnsAsync(new[] { expectedLink });

			// Act
			var results = await _subject.Generate(_website);

			// Assert
			var basePage = results.Single(x => x.Address.AbsoluteUri == _website);

			Assert.That(basePage.LinkedAddresses.Single(), Is.EqualTo(expectedLink));
		}

		[Test]
		public async Task When1LinkReturned_ShouldReturn2ndPage()
		{
			// Arrange
			var expectedLink = _fixture.Create<Uri>();

			_linkCrawlerMock.Setup(x => x.FindLinksOnSameDomain(It.Is<Uri>(uri => uri.AbsoluteUri == _website)))
				.ReturnsAsync(new[] { expectedLink });

			// Act
			var results = await _subject.Generate(_website);

			// Assert
			var expectedPage = results.SingleOrDefault(x => x.Address == expectedLink);

			Assert.That(expectedPage, Is.Not.Null);
		}

		[Test]
		public async Task WhenLinkedPageCreatesACycle_ShouldNotRevisit()
		{
			// Arrange
			var linkedAddress = _fixture.Create<Uri>();

			_linkCrawlerMock.Setup(x => x.FindLinksOnSameDomain(It.Is<Uri>(uri => uri.AbsoluteUri == _website)))
				.ReturnsAsync(new[] { linkedAddress });

			_linkCrawlerMock.Setup(x => x.FindLinksOnSameDomain(linkedAddress))
				.ReturnsAsync(new[] { new Uri(_website) });

			// Act
			var results = await _subject.Generate(_website);

			// Assert
			Assert.That(results.Count(), Is.EqualTo(2));

			_linkCrawlerMock.Verify(x => x.FindLinksOnSameDomain(It.IsAny<Uri>()),
				Times.Exactly(2));
		}

		[Test]
		public async Task WhenLinkedPageCreatesACycle_ShouldShowCycleInResults()
		{
			// Arrange
			var linkedAddress = _fixture.Create<Uri>();

			_linkCrawlerMock.Setup(x => x.FindLinksOnSameDomain(It.Is<Uri>(uri => uri.AbsoluteUri == _website)))
				.ReturnsAsync(new[] { linkedAddress });

			_linkCrawlerMock.Setup(x => x.FindLinksOnSameDomain(linkedAddress))
				.ReturnsAsync(new[] { new Uri(_website) });

			// Act
			var results = await _subject.Generate(_website);

			// Assert
			var basePage = results.Single(x => x.Address.AbsoluteUri == _website);
			var linkedPage = results.Single(x => x.Address == linkedAddress);

			Assert.That(basePage.LinkedAddresses.Single(), Is.EqualTo(linkedAddress));
			Assert.That(linkedPage.LinkedAddresses.Single(), Is.EqualTo(basePage.Address));
		}

		[Test]
		public async Task WhenMultipleLinksPointToSameAddress_ShouldOnlyVisitAddressOnce()
		{
			// Arrange
			var addressA = _fixture.Create<Uri>();
			var addressB = _fixture.Create<Uri>();
			var sharedAddress = _fixture.Create<Uri>();

			_linkCrawlerMock.Setup(x => x.FindLinksOnSameDomain(It.Is<Uri>(uri => uri.AbsoluteUri == _website)))
				.ReturnsAsync(new[] { addressA, addressB });

			_linkCrawlerMock.Setup(x => x.FindLinksOnSameDomain(It.Is<Uri>(uri => uri == addressA || uri == addressB)))
				.ReturnsAsync(new[] { sharedAddress });

			// Act
			await _subject.Generate(_website);

			// Assert
			_linkCrawlerMock.Verify(x => x.FindLinksOnSameDomain(sharedAddress));
		}

		[Test]
		public async Task WhenMultipleLinksPointToSameAddress_ShouldShowLinkedPageInResults()
		{
			// Arrange
			var addressA = _fixture.Create<Uri>();
			var addressB = _fixture.Create<Uri>();
			var sharedAddress = _fixture.Create<Uri>();

			_linkCrawlerMock.Setup(x => x.FindLinksOnSameDomain(It.Is<Uri>(uri => uri.AbsoluteUri == _website)))
				.ReturnsAsync(new[] { addressA, addressB });

			_linkCrawlerMock.Setup(x => x.FindLinksOnSameDomain(It.Is<Uri>(uri => uri == addressA || uri == addressB)))
				.ReturnsAsync(new[] { sharedAddress });

			// Act
			var results = await _subject.Generate(_website);

			// Assert
			var linkedPageA = results.Single(x => x.Address == addressA);
			var linkedPageB = results.Single(x => x.Address == addressB);

			Assert.That(linkedPageA.LinkedAddresses.Single(), Is.EqualTo(sharedAddress));
			Assert.That(linkedPageB.LinkedAddresses.Single(), Is.EqualTo(sharedAddress));
		}
	}
}
