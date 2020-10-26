using Hosta.Crypto;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace HostaTests.Crypto
{
	[TestClass]
	public class KDFRatchetTester
	{
		[TestMethod]
		public void TestSymmetry()
		{
			byte[] key = SecureRandomGenerator.GetBytes(32);
			var a = new KDFRatchet(key);
			var b = new KDFRatchet(key);
			for (int i = 0; i < 100; i++)
			{
				byte[] input = SecureRandomGenerator.GetBytes(32);
				var outputA = a.Turn(input);
				var outputB = b.Turn(input);
				CollectionAssert.AreEqual(outputA, outputB);
			}
		}
	}
}