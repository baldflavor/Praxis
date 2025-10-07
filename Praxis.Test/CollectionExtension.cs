namespace Praxis.Test;

using System.Collections;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Praxis.Knowledge;

[TestClass]
public class CollectionExtension {

	[TestMethod]
	public void DisposeAny() {
		object[] objs = [.. Disposey.Create(100).Concat(Enumerable.Range(0, 50).Select(e => Word.Random()))];
		Random.Shared.Shuffle(objs);

		objs.DisposeAny();
		var disps = objs.OfType<Disposey>();
		Assert.IsTrue(disps.All(d => d.IsDisposed));
	}


	private class Disposey : IDisposable {
		private static int _autoID;

		private bool _isDisposed;

		public bool IsDisposed => _isDisposed;

		/// <summary>
		/// Random number set when the object has been disposed
		/// </summary>
		public int DisposedCode { get; private set; }

		public int DisposeyID { get; } = Interlocked.Increment(ref _autoID);

		protected virtual void Dispose(bool disposing) {
			if (!_isDisposed) {
				if (disposing) {
					this.DisposedCode = Random.Shared.Next();
				}

				_isDisposed = true;
			}
		}


		public static object[] Create(int count) => [.. Enumerable.Range(0, count).Select(e => new Disposey())];

		public override int GetHashCode() => this.DisposeyID;

		public void Dispose() {
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
