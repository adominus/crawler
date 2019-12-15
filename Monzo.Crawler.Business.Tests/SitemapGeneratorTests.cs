using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Monzo.Crawler.Business.Tests
{
	public class SitemapGeneratorTests
	{
		private IFixture _fixture;

		private SitemapGenerator _subject;
		private Mock<ILinkCrawler> _linkCrawlerMock;

		private string _validHost;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_fixture = new Fixture()
				.Customize(new AutoMoqCustomization());

			_validHost = "http://localhost/";
		}

		[SetUp]
		public void SetUp()
		{
			_linkCrawlerMock = _fixture.Freeze<Mock<ILinkCrawler>>();

			_subject = _fixture.Create<SitemapGenerator>();
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
			await _subject.Generate(_validHost);

			// Assert 
			_linkCrawlerMock.Verify(x => x.FindLinksOnSameDomain(
				It.Is<Uri>(uri => uri.AbsoluteUri == _validHost)));
		}

		[Test]
		public async Task WhenNoLinksReturned_ShouldReturnSingleVisitedPage()
		{
			// Arrange, Act
			var result = await _subject.Generate(_validHost);

			// Assert 
			Assert.That(result.Count(), Is.EqualTo(1));
		}

		[Test]
		public async Task WhenNoLinksReturned_ShouldReturnPageMatchingOriginalHost()
		{
			// Arrange, Act
			var result = await _subject.Generate(_validHost);

			// Assert 
			Assert.That(result.Single().Address.AbsoluteUri, Is.EqualTo(_validHost));
		}
	}
}
