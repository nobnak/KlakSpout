using nobnak.Gist;
using nobnak.Gist.DataUI;
using nobnak.Gist.IMGUI.Scope;
using nobnak.Gist.InputDevice;
using nobnak.Gist.Loader;
using nobnak.Gist.ObjectExt;
using UnityEngine;

namespace Klak.Spout {

	[ExecuteAlways]
	public class SpoutController : MonoBehaviour {
		[SerializeField]
		protected SpoutSender sender = null;
		[SerializeField]
		protected Data data = new Data();
		[SerializeField]
		protected FilePath serialized = new FilePath();
		[SerializeField]
		protected KeycodeToggle toggle = new KeycodeToggle(KeyCode.S);

		protected Validator validator = new Validator();
		protected GUIData guidata;
		protected Rect windowRect = new Rect(10, 10, 300, 100);

		#region unity
		private void OnEnable() {
			validator.Reset();
			validator.Validation += () => {
				Debug.LogFormat("Update Spout : {0}", data);
				var active = sender != null && (!data.spout || (data.width > 4 && data.height > 4));
				if (active) {
					guidata = new GUIData(data);
					data.Apply(sender);
				}
			};
			Load();
		}
		private void OnValidate() {
			validator.Invalidate();
		}
		private void Update() {
			toggle.Update();
			validator.Validate();
		}
		private void OnDisable() {
		}
		private void OnGUI() {
			if (toggle.Visible) {
				windowRect = GUILayout.Window(GetInstanceID(), windowRect, Window, name);
			}
		}
		#endregion
		#region member
		private void Window(int id) {
			using (new GUILayout.HorizontalScope()) {
				if (GUILayout.Button("Save"))
					Save();
				if (GUILayout.Button("Load"))
					Load();
			}
			using (new GUIChangedScope(() => {
				guidata.Apply(data);
				validator.Invalidate();
			})) {
				using (new GUILayout.VerticalScope()) {
					guidata.spout = GUILayout.Toggle(guidata.spout, "Spout");
					using(new GUILayout.HorizontalScope()) {
						GUILayout.Label("Width:");
						guidata.width.StrValue = GUILayout.TextField(guidata.width.StrValue);
					}
					using(new GUILayout.HorizontalScope()) {
						GUILayout.Label("Height:");
						guidata.height.StrValue = GUILayout.TextField(guidata.height.StrValue);
					}
				}
			}
			UnityEngine.GUI.DragWindow();
		}
		private void Load() {
			serialized.TryLoadOverwrite(ref data);
		}
		private void Save() {
			serialized.TrySave(data);
		}
		#endregion
		#region classes
		[System.Serializable]
		public class Data {
			public bool spout;
			public int width = 1920;
			public int height = 1080;

			#region interface
			#region object
			public override bool Equals(object obj) {
				if (!(obj is Data))
					return false;

				var b = (Data)obj;
				return spout == b.spout
					&& width == b.width
					&& height == b.height;
			}
			public override int GetHashCode() {
				var v = 1023;
				v = 560689 * (v) + spout.GetHashCode();
				v = 560689 * (v) + width.GetHashCode();
				v = 560689 * (v) + height.GetHashCode();
				return v;
			}
			public override string ToString() {
				return string.Format("<{0} : spout={1} size=({2}x{3})>",
					GetType().Name, spout, width, height);
			}
			#endregion

			public void Apply(SpoutSender sender) {
				sender.enabled = spout;
				var dst = sender.Data;
				dst.width = width;
				dst.height = height;
				sender.Data = dst;
			}
			#endregion
		}
		public class GUIData {
			public bool spout;
			public TextInt width;
			public TextInt height;

			public GUIData(Data data) {
				spout = data.spout;
				width = new TextInt(data.width);
				height = new TextInt(data.height);
			}
			public void Apply(Data data) {
				data.spout = spout;
				data.width = width;
				data.height = height;
			}
		}
		#endregion
	}
}
