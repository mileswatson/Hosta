using Hosta.Crypto;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HostaTests.Crypto
{
	[TestClass]
	public class KeyExchangerTester
	{
		[TestMethod]
		public void TestExchange()
		{
			var a = new KeyExchanger();
			var b = new KeyExchanger();
			var aKey = a.KeyFromToken(b.Token);
			var bKey = b.KeyFromToken(a.Token);
			CollectionAssert.AreEqual(aKey, bKey);
		}
	}
}