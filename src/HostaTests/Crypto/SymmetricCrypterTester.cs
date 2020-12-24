using Hosta.Crypto;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HostaTests.Crypto
{
	[TestClass]
	public class SymmetricCrypterTester
	{
		[DataTestMethod]
		[DataRow(0)]
		[DataRow(1)]
		[DataRow(31)]
		[DataRow(32)]
		[DataRow(33)]
		[DataRow(12343)]
		public void TestCycle(int size)
		{
			var bytes = SecureRandomGenerator.GetBytes(size);
			var crypter = new SymmetricCrypter(SecureRandomGenerator.GetBytes(32));
			CollectionAssert.AreEqual(bytes, crypter.Decrypt(crypter.Encrypt(bytes)));
		}
	}
}