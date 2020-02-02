using nobnak.Gist;
using nobnak.Gist.DataUI;
using nobnak.Gist.IMGUI.Scope;
using nobnak.Gist.InputDevice;
using nobnak.Gist.Loader;
using nobnak.Gist.ObjectExt;
using nobnak.Gist.Resizable;
using UnityEngine;

namespace Klak.Spout {

	[ExecuteAlways]
	[RequireComponent(typeof(Camera))]
	[RequireComponent(typeof(SpoutSender))]
	public class SpoutController : MonoBehaviour {
		[SerializeField]
		protected Data data = new Data();
		[SerializeField]
		protected FilePath serialized = new FilePath(
			string.Format(FilePath.DEFAULT_FILEPATH_PATTERN, "Spout.txt"));
		[SerializeField]
		protected KeycodeToggle toggle = new KeycodeToggle(KeyCode.S);
		[SerializeField]
		protected RenderTextureFormat format = RenderTextureFormat.ARGBHalf;

		[SerializeField]
		protected RenderTextureEvent Changed = new RenderTextureEvent();
		[SerializeField]
		protected BoolEvent ActiveOnEnabled = new BoolEvent();
		[SerializeField]
		protected BoolEvent ActiveOnDisabled = new BoolEvent();

		protected SpoutSender sender = null;
		protected Camera targetCamera = null;

		protected Validator validator = new Validator();
		protected GUIData guidata;
		protected Rect windowRect = new Rect(10, 10, 300, 100);
		protected ResizableRenderTexture targetTex;

		#region unity
		private void OnEnable() {
			sender = GetComponent<SpoutSender>();
			targetCamera = GetComponent<Camera>();

			targetTex = new ResizableRenderTexture();

			validator.Reset();
			validator.Validation += () => {
				Debug.LogFormat("Update Spout : {0}", data);
				SetTargetTexture(null);
				guidata = new GUIData(data);

				var frt = new FormatRT() {
					textureFormat = format,
					depth = 24,
					useMipMap = false,
					antiAliasing = QualitySettings.antiAliasing,
					readWrite = RenderTextureReadWrite.Default,
					filterMode = FilterMode.Bilinear,
					wrapMode = TextureWrapMode.Clamp,
					anisoLevel = 0
				};

				if (sender != null)
					sender.enabled = data.spout;

				if (data.spout) {
					targetTex.Format = frt;
					targetTex.Size = data.Size;
					SetTargetTexture(targetTex);
				} else {
					targetTex.Release();
				}

				ActiveOnEnabled.Invoke(data.spout);
				ActiveOnDisabled.Invoke(!data.spout);
			};
			Load();

			validator.Validate();
		}


		private void OnDisable() {
			SetTargetTexture(null);
			if (targetTex != null) {
				targetTex.Dispose();
				targetTex = null;
			}
		}
		private void OnValidate() {
			validator.Invalidate();
		}
		private void Update() {
			toggle.Update();
			validator.Validate();
		}
		private void OnGUI() {
			if (toggle.Visible) {
				windowRect = GUILayout.Window(GetInstanceID(), windowRect, Window, name);
			}
		}
		#endregion

		#region member
		#region gui
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
		#endregion

		private void SetTargetTexture(RenderTexture targetTex) {
			if (targetCamera != null)
				targetCamera.targetTexture = targetTex;
			Changed.Invoke(targetTex);
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

			public Vector2Int Size {
				get {
					return new Vector2Int(width, height);
				}
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
		[System.Serializable]
		public class RenderTextureEvent : UnityEngine.Events.UnityEvent<RenderTexture> { }
		[System.Serializable]
		public class BoolEvent : UnityEngine.Events.UnityEvent<bool> { }
		#endregion
	}
}
