﻿using System;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using EPS.Web.Abstractions;
using EPS.Web.Authentication.Configuration;
using EPS.Web.Authentication.Digest.Configuration;
using FakeItEasy;
using Xunit;

#pragma warning disable 0649 // for [Fake]

namespace EPS.Web.Authentication.Digest.Tests.Unit
{
	public class DigestAuthenticatorTest : IDisposable
	{
		class MockConfiguration : IDigestAuthenticatorConfiguration
		{
			public string Realm { get; set; }
			public string PrivateKey { get; set; }
			public TimeSpan NonceValidDuration { get; set; }
			public bool RequireSsl { get; set; }

			//TODO: 4-6-2011 - need to perform validation here as well -- check on actual config class -- some stuff is reqd, some not
			public string RoleProviderName { get; set; }
			public string Name { get; set; }
			public string ProviderName { get; set; }
			public Abstractions.IAuthenticator Authenticator { get; private set; }
			public IPrincipalBuilder PrincipalBuilder { get; private set; }
		}

		class MockPrincipalBuilderFactory : IPrincipalBuilderFactory
		{
			public static IPrincipalBuilder Builder { get; set; }
			public IPrincipalBuilder Construct(IAuthenticatorConfiguration config)
			{
				return Builder;
			}
		}

		[Fake]
		readonly MembershipProvider fakeMembershipProvider;
		[Fake]
		MembershipUser fakeUser;
		MockConfiguration configuration = new MockConfiguration()
		{
			Realm = "testrealm@testhost.com",
			ProviderName = "Fake",
			PrivateKey = "MyPrivateKey",
			NonceValidDuration = TimeSpan.FromSeconds(30)
		};
		[Fake]
		HttpContextBase context;
		[Fake]
		IPrincipalBuilder principalBuilder;
		PrivateHashEncoder privateHashEncoder;
		private bool disposed;

		public DigestAuthenticatorTest()
		{
			Fake.InitializeFixture(this);

			//configuration.Authenticator = new AuthenticationInspectingAuthenticator(configuration);
			//configuration.PrincipalBuilder = principalBuilder;

			A.CallTo(() => fakeUser.GetPassword()).Returns("Circle Of Life");
			A.CallTo(() => fakeMembershipProvider.Name).Returns("Fake");
			A.CallTo(() => fakeMembershipProvider.GetUser("Mufasa", A<Boolean>.Ignored)).Returns(fakeUser);
			Membership.Providers.AddMembershipProvider(fakeMembershipProvider);

			//for our tests, we're just going to wire up this one hardcoded principalBuilder
			MockPrincipalBuilderFactory.Builder = principalBuilder;

			A.CallTo(() => principalBuilder.ConstructPrincipal(A<HttpContextBase>.Ignored, A<string>.Ignored, A<string>.Ignored))
				.ReturnsLazily(call => new GenericPrincipal(new GenericIdentity(call.GetArgument<string>("userName")), new[] { "roles" }));

			privateHashEncoder = new PrivateHashEncoder("MyPrivateKey");

			Opaque.Current = () => "5ccc069c403ebaf9f0171e9517f40e41";
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					Membership.Providers.RemoveMembershipProvider("Fake");
				}

				disposed = true;
			}
		}

		[Fact]
		public void Constructor_ThrowsOnNullConfiguration()
		{
			Assert.Throws<ArgumentNullException>(() => new DigestAuthenticator(null));
		}

		[Fact]
		public void Constructor_ThrowsOnNullPrivateKey()
		{
			var configuration = new MockConfiguration() { PrivateKey = null, NonceValidDuration = TimeSpan.FromMinutes(10), Realm = "realm", RequireSsl = false };
			Assert.Throws<ArgumentNullException>(() => new DigestAuthenticator(configuration));
		}

		[Fact]
		public void Constructor_ThrowsOnEmptyPrivateKey()
		{
			var configuration = new MockConfiguration() { PrivateKey = string.Empty, NonceValidDuration = TimeSpan.FromMinutes(10), Realm = "realm", RequireSsl = false };
			Assert.Throws<ArgumentException>(() => new DigestAuthenticator(configuration));
		}

		[Fact]
		public void Constructor_ThrowsOnPrivateKeyTooShort()
		{
			var configuration = new MockConfiguration() { PrivateKey = "1234567", NonceValidDuration = TimeSpan.FromMinutes(10), Realm = "realm", RequireSsl = false };
			Assert.Throws<ArgumentException>(() => new DigestAuthenticator(configuration));
		}

		[Fact]
		public void Constructor_ThrowsOnNullRealm()
		{
			var configuration = new MockConfiguration() { PrivateKey = "12345678", NonceValidDuration = TimeSpan.FromMinutes(10), Realm = null, RequireSsl = false };
			Assert.Throws<ArgumentNullException>(() => new DigestAuthenticator(configuration));
		}

		[Fact]
		public void Constructor_ThrowsOnEmptyRealm()
		{
			var configuration = new MockConfiguration() { PrivateKey = "12345678", NonceValidDuration = TimeSpan.FromMinutes(10), Realm = string.Empty, RequireSsl = false };
			Assert.Throws<ArgumentException>(() => new DigestAuthenticator(configuration));
		}

		[Fact]
		public void Constructor_ThrowsOnTimeSpanTooShort()
		{
			var configuration = new MockConfiguration() { PrivateKey = "12345678", NonceValidDuration = TimeSpan.FromSeconds(19), Realm = "realm", RequireSsl = false };
			Assert.Throws<ArgumentException>(() => new DigestAuthenticator(configuration));
		}

		[Fact]
		public void Constructor_ThrowsOnTimeSpanTooLong()
		{
			var configuration = new MockConfiguration() { PrivateKey = "12345678", NonceValidDuration = TimeSpan.FromMinutes(61), Realm = "realm", RequireSsl = false };
			Assert.Throws<ArgumentException>(() => new DigestAuthenticator(configuration));
		}

		[Fact]
		public void Authenticate_ThrowsOnNullContext()
		{
			var inspector = new DigestAuthenticator(configuration);
			Assert.Throws<ArgumentNullException>(() => inspector.Authenticate(null));
		}

		[Fact]
		[SuppressMessage("Gendarme.Rules.BadPractice", "PreferTryParseRule", Justification = "We know our hard coded value is appropriate")]
		[SuppressMessage("Gendarme.Rules.Concurrency", "WriteStaticFieldFromInstanceMethodRule", Justification = "NonceManager.Now is intended to only be used internally by tests, and as such is OK")]
		public void Authenticate_TrueOnValidMembership()
		{
			string ipAddress = "127.0.0.1";
			configuration.Realm = "testrealm@host.com";

			var inspector = new DigestAuthenticator(configuration);

			//the result of MD5 hashing some well known values (either specified in the header below or similar)
			string response = "dc950f2d7c24037a6c775bcc9198b6f8";
			//939e7578ed9e3c518a452acee763bce9:NjM0Mzc3MjI2OTIwMDA6Yjg3ZWZlODM0Mjc1NThjZGVlZWVkYjRjNTI1MzFjMzM=:00000001:0a4f113b:auth:39aff3a2bab6126f332b942af96d3366

			NonceManager.Now = () => DateTime.Parse("4/6/2011 9:38:12 PM", CultureInfo.CurrentCulture);

			string nonce = NonceManager.Generate(ipAddress, privateHashEncoder);
			//this should generate very specific nonce "NjM0Mzc3MjI2OTIwMDA6Yjg3ZWZlODM0Mjc1NThjZGVlZWVkYjRjNTI1MzFjMzM="

			var headers = new NameValueCollection() { { "Authorization", string.Format(CultureInfo.InvariantCulture,
@"Digest username=""Mufasa"",realm=""{0}"",
                     nonce=""{1}"",
                     uri=""/dir/index.html"",qop=auth,nc=00000001,cnonce=""0a4f113b"",
                     response=""{2}"",
                     opaque=""{3}""", configuration.Realm, nonce, response, Opaque.Current())} };


			A.CallTo(() => context.Request.Headers).Returns(headers);
			A.CallTo(() => context.Request.HttpMethod).Returns("GET");
			A.CallTo(() => context.Request.UserHostAddress).Returns(ipAddress);

			var result = inspector.Authenticate(context);

			NonceManager.Now = () => { return DateTime.UtcNow; };
			Assert.True(result.Success);
			Assert.Equal(result.Principal.Identity.Name, "Mufasa");
		}

		[Fact]
		public void Authenticate_FalseOnMismatchedRealm()
		{
			string ipAddress = "127.0.0.1";
			string nonce = NonceManager.Generate(ipAddress, privateHashEncoder);

			configuration.Realm = "testrealm@testhost.com";

			var inspector = new DigestAuthenticator(configuration);

			var headers = new NameValueCollection() { { "Authorization", string.Format(CultureInfo.InvariantCulture,
@"Digest username=""Mufasa"",realm=""testrealm@host.com"",
                     nonce=""{0}"",
                     uri=""/dir/index.html"",qop=auth,nc=00000001,cnonce=""0a4f113b"",
                     response=""6629fae49393a05397450978507c4ef1"",
                     opaque=""5ccc069c403ebaf9f0171e9517f40e41""", nonce) } };

			A.CallTo(() => context.Request.Headers).Returns(headers);
			A.CallTo(() => context.Request.HttpMethod).Returns("GET");
			A.CallTo(() => context.Request.UserHostAddress).Returns(ipAddress);

			var result = inspector.Authenticate(context);
			//TODO: validate that we have a GenericPrincipal / GenericIdentity
			Assert.False(result.Success);
		}

		[Fact(Skip = "Write some more examples of auth failures")]
		public void Authenticate_FalseOnAuthenticationFailures()
		{
		}
	}
}