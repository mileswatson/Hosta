using Hosta.Crypto;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HostaTests.Crypto
{
	[TestClass]
	public class SymmetricCrypterTester
	{
		SymmetricCrypter crypter = new SymmetricCrypter(SecureRandomGenerator.GetBytes(32));

		[DataTestMethod]
		[DataRow(0)]
		[DataRow(1)]
		[DataRow(31)]
		[DataRow(32)]
		[DataRow(33)]
		[DataRow(1 << 16 + 1)]
		public void TestCycle(int size)
		{
			var bytes = SecureRandomGenerator.GetBytes(size);
			CollectionAssert.AreEqual(bytes, crypter.Decrypt(crypter.Encrypt(bytes)).Value);
		}

		[TestMethod]
		public void TestShort()
		{
			var bytes = SecureRandomGenerator.GetBytes(SymmetricCrypter.NONCE_SIZE + SymmetricCrypter.TAG_SIZE - 1);
			var crypter = new SymmetricCrypter(SecureRandomGenerator.GetBytes(32));
			Assert.IsTrue(crypter.Decrypt(bytes).Error is SymmetricCrypter.MessageTooShortError);
		}

		[TestMethod]
		public void TestTampered()
		{
			var bytes = SecureRandomGenerator.GetBytes(100);
			var encrypted = crypter.Encrypt(bytes);
			encrypted[0]++;
			Assert.IsTrue(crypter.Decrypt(encrypted).Error is SymmetricCrypter.TamperedMessageError);
		}
	}
}