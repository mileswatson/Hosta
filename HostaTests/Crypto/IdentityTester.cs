using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hosta.Crypto;

namespace HostaTests.Crypto
{
	[TestClass]
	public class IdentityTester
	{
		private PrivateIdentity privateIdentity;
		private PublicIdentity publicIdentity;

		[TestInitialize]
		[TestMethod]
		public void ExportImport()
		{
			privateIdentity = new PrivateIdentity();
			publicIdentity = new PublicIdentity(privateIdentity.PublicIdentityInfo);
		}

		[TestMethod]
		[DataRow(0)]
		[DataRow(1)]
		[DataRow(100)]
		[DataRow(1 << 15)]
		public void TestValid(int length)
		{
			var contract = SecureRandomGenerator.GetBytes(length);
			var signature = privateIdentity.Sign(contract);
			Assert.IsTrue(publicIdentity.Verify(contract, signature));
		}

		[TestMethod]
		[DataRow(1)]
		[DataRow(100)]
		[DataRow(1 << 15)]
		public void TestChangedData(int length)
		{
			var contract = SecureRandomGenerator.GetBytes(length);
			var signature = privateIdentity.Sign(contract);
			contract[0]++;
			Assert.IsFalse(publicIdentity.Verify(contract, signature));
		}

		[TestMethod]
		[DataRow(1)]
		[DataRow(100)]
		[DataRow(1 << 15)]
		public void TestChangedSignature(int length)
		{
			var contract = SecureRandomGenerator.GetBytes(length);
			var signature = privateIdentity.Sign(contract);
			signature[0]++;
			Assert.IsFalse(publicIdentity.Verify(contract, signature));
		}
	}
}