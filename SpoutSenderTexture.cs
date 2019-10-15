using nobnak.Gist.Extensions.Texture2DExt;
using System.Collections.Generic;
using UnityEngine;

namespace Klak.Spout {

    public class SpoutSenderTexture : System.IDisposable {
        protected bool currValidity = false;
        protected Data curr;

        protected System.IntPtr _sender = System.IntPtr.Zero;
        protected Texture2D _sharedTexture = null;

        public SpoutSenderTexture() {
            Invalidate();
        }

        #region IDisposable
        public virtual void Dispose() {
            ClearSender();
            ClearSharedTexture();
			Invalidate();
		}
        #endregion
        
		#region public
		public virtual void Invalidate() {
            currValidity = false;
            curr = default(Data);
        }
        public virtual void Prepare(Data next) {
            if (!Validity() || !curr.Equals(next)) {
                ClearSender();
                ClearSharedTexture();
                Invalidate();
                if (InputValidity(next) && TryBuildSender(next, out _sender)) {
                    currValidity = true;
                    curr = next.Clone();
                }
            }
        }
        public virtual Texture2D SharedTexture() {
            if (currValidity && _sharedTexture == null)
                TryBuildTexture(_sender, out _sharedTexture);
            return _sharedTexture;
        }
		public virtual Vector2Int Size {
			get {
				return (_sender == System.IntPtr.Zero) ?
					default(Vector2Int) :
					new Vector2Int(
						PluginEntry.GetTextureWidth(_sender),
						PluginEntry.GetTextureHeight(_sender));
			}
		}
		public virtual bool ExistSender() {
			return _sender != System.IntPtr.Zero && PluginEntry.ExistSender(_sender);
		}
		#endregion

		#region static
		public static bool InputValidity(Data data) {
			return
				!string.IsNullOrEmpty(data.name)
				&& data.width >= 4
				&& data.height >= 4;
		}
		public static bool TryBuildSender(Data next, out System.IntPtr sender) {
			sender = PluginEntry.CreateSender(next.name, next.width, next.height);
			return sender != System.IntPtr.Zero;
		}
		public static bool TryBuildTexture(System.IntPtr sender, out Texture2D sharedTexture) {
			sharedTexture = null;

			var ptr = PluginEntry.GetTexturePointer(sender);
			if (ptr == System.IntPtr.Zero)
				return false;

			var width = PluginEntry.GetTextureWidth(sender);
			var height = PluginEntry.GetTextureHeight(sender);
			sharedTexture = Texture2D.CreateExternalTexture(width, height,
				TextureFormat.ARGB32, false, false, ptr);
			Debug.LogFormat("Create External Texture2D ({0}x{1})", width, height);
			return true;
		}
		#endregion

		#region private
		protected virtual bool Validity() {
			return currValidity && ExistSender();
		}
		protected virtual void ClearSharedTexture() {
            if (_sharedTexture != null) {
                _sharedTexture.Destroy();
                _sharedTexture = null;
            }
        }
        protected virtual void ClearSender() {
            if (_sender != System.IntPtr.Zero) {
                PluginEntry.DestroySharedObject(_sender);
                _sender = System.IntPtr.Zero;
            }
        }
		#endregion

		#region Classes
		[System.Serializable]
        public struct Data {
			public string name;
            public int width;
            public int height;

            public override bool Equals(object obj) {
                var b = (Data)obj;
                var result = name == b.name
                    && width == b.width
                    && height == b.height;
                return result;
            }
            public override string ToString() {
                return string.Format("Data <name={0} size=({1}x{2})", name, width, height);
            }
            public override int GetHashCode() {
                var hashCode = -1072973697;
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(name);
                hashCode = hashCode * -1521134295 + width.GetHashCode();
                hashCode = hashCode * -1521134295 + height.GetHashCode();
                return hashCode;
            }

            public Data Clone() {
                return new Data() { name = this.name, width = this.width, height = this.height };
            }
			public Vector2Int Size {
				get { return new Vector2Int(width, height); }
			}
        }
        #endregion
    }
}
