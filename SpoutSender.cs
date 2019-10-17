// KlakSpout - Spout realtime video sharing plugin for Unity
// https://github.com/keijiro/KlakSpout
using nobnak.Gist.Resizable;
using System.Collections;
using System.Threading;
using UnityEngine;

namespace Klak.Spout
{
    /// Spout sender class
    [AddComponentMenu("Klak/Spout/Spout Sender")]
    [ExecuteAlways]
    public class SpoutSender : MonoBehaviour {
        public enum UpdateMode { Update = 0, RenderImage }

        [SerializeField] protected bool _clearAlpha = true;
		[SerializeField]
		protected SpoutSenderTexture.Data data = new SpoutSenderTexture.Data() {
			name = "UnitySender",
			width = 1920,
			height = 1080
		};

        [SerializeField] BoolEvent EnabledOnEnable = new BoolEvent();
        [SerializeField] BoolEvent EnabledOnDisable = new BoolEvent();
        [SerializeField] RenderTextureEvent EventOnUpdateTexture = new RenderTextureEvent();

		[SerializeField] protected bool linear;
        [SerializeField] protected UpdateMode updateMode;

        protected Camera attachedCamera;
		protected ResizableRenderTexture temptex0;
		protected SpoutSenderTexture senderTexture;
        protected Material _fixupMaterial;
		protected Coroutine coroutineUpdateSharedTexture;

        public bool clearAlpha {
            get { return _clearAlpha; }
            set { _clearAlpha = value; }
        }

        public SpoutSenderTexture.Data Data { get { return data; } set { data = value; } }

        #region MonoBehaviour functions
        void OnEnable()
        {
            senderTexture = new SpoutSenderTexture();

			temptex0 = new ResizableRenderTexture(new nobnak.Gist.Resizable.FormatRT() {
				readWrite = GetColorspace(),
				antiAliasing = QualitySettings.antiAliasing,
			});

			attachedCamera = GetComponent<Camera>();

			coroutineUpdateSharedTexture = StartCoroutine(ProcessUpdateSharedTexture());

            EnabledOnEnable.Invoke(enabled);
            EnabledOnDisable.Invoke(!enabled);
        }
        void OnDisable() {
			Thread.Sleep((int)(1 * (1000 * Time.smoothDeltaTime)));

			SetTargetTexture(null);
			if (senderTexture != null) {
                senderTexture.Dispose();
                senderTexture = null;
            }
			if (temptex0 != null) {
				temptex0.Dispose();
				temptex0 = null;
			}
			if (coroutineUpdateSharedTexture != null) {
				StopCoroutine(coroutineUpdateSharedTexture);
				coroutineUpdateSharedTexture = null;
			}
            EnabledOnEnable.Invoke(enabled);
            EnabledOnDisable.Invoke(!enabled);
        }
        void Update() {
			senderTexture.Prepare(data);
			temptex0.Size = data.Size;
            SetTargetTexture(temptex0.Texture);

			if (!senderTexture.ExistSender())
				Debug.LogWarning("Sender not found");

			PluginEntry.Poll();
		}
        private void OnRenderImage(RenderTexture source, RenderTexture destination) {
            Graphics.Blit(source, destination);

            switch (updateMode) {
                case UpdateMode.RenderImage:
                    UpdateSharedTexture();
                    break;
            }
        }
        #endregion

        #region member
        protected virtual void SetTargetTexture(RenderTexture tex) {
			if (attachedCamera != null)
				attachedCamera.targetTexture = tex;
			EventOnUpdateTexture.Invoke(tex);
		}
		protected virtual void UpdateSharedTexture() {
			Texture2D sharedTexture;
			if (senderTexture != null
				&& temptex0 != null
				&& (sharedTexture = senderTexture.SharedTexture()) != null
				&& sharedTexture.width > 0
				&& sharedTexture.height > 0) {

				if (_fixupMaterial == null)
					_fixupMaterial = new Material(Shader.Find("Hidden/Spout/Fixup"));
				_fixupMaterial.SetFloat("_ClearAlpha", _clearAlpha ? 1 : 0);

				var tempRT = RenderTexture.GetTemporary(
					sharedTexture.width, sharedTexture.height, 0,
					UnityEngine.RenderTextureFormat.ARGB32,
					GetColorspace());

				var prevSrgbWrite = GL.sRGBWrite;
				GL.sRGBWrite = !linear;
				Graphics.Blit(temptex0.Texture, tempRT, _fixupMaterial, 0);
				Graphics.CopyTexture(tempRT, sharedTexture);
				RenderTexture.ReleaseTemporary(tempRT);
				GL.sRGBWrite = prevSrgbWrite;
			}
		}

		protected RenderTextureReadWrite GetColorspace() {
			return (linear ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB);
		}

		protected IEnumerator ProcessUpdateSharedTexture() {
			while (true) {
				yield return new WaitForEndOfFrame();
                if (updateMode == UpdateMode.Update)
				    UpdateSharedTexture();
			}
		}
		#endregion

		#region definitions
		[System.Serializable]
        public class BoolEvent : UnityEngine.Events.UnityEvent<bool> { }
        [System.Serializable]
        public class RenderTextureEvent : UnityEngine.Events.UnityEvent<RenderTexture> { }
		#endregion
	}
}
