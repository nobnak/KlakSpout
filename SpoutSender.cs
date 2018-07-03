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
    [ExecuteInEditMode]
    public class SpoutSender : MonoBehaviour {
        [SerializeField] protected bool _clearAlpha = true;
        [SerializeField] protected SpoutSenderTexture.Data data;

        [SerializeField] BoolEvent EnabledOnEnable;
        [SerializeField] BoolEvent EnabledOnDisable;
        [SerializeField] RenderTextureEvent EventOnUpdateTexture;

		protected ResizableRenderTexture temporaryTexture;
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

			temporaryTexture = new ResizableRenderTexture(new nobnak.Gist.Resizable.FormatRT() {
				readWrite = RenderTextureReadWrite.sRGB,
				antiAliasing = QualitySettings.antiAliasing
			});

			//coroutineUpdateSharedTexture = StartCoroutine(ProcessUpdateSharedTexture());

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
			if (temporaryTexture != null) {
				temporaryTexture.Dispose();
				temporaryTexture = null;
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
			temporaryTexture.Size = data.Size;
            SetTargetTexture(temporaryTexture.Texture);

			if (!senderTexture.ExistSender())
				Debug.LogWarning("Sender not found");

			PluginEntry.Poll();
		}
		private void OnRenderImage(RenderTexture source, RenderTexture destination) {
			UpdateSharedTexture();
		}
		#endregion

		protected virtual void SetTargetTexture(RenderTexture tex) {
            EventOnUpdateTexture.Invoke(tex);
		}
		protected virtual void UpdateSharedTexture() {
			Texture2D sharedTexture;
			if (senderTexture != null
				&& temporaryTexture != null
				&& (sharedTexture = senderTexture.SharedTexture()) != null
				&& sharedTexture.width > 0
				&& sharedTexture.height > 0) {
				
				if (_fixupMaterial == null)
					_fixupMaterial = new Material(Shader.Find("Hidden/Spout/Fixup"));
				_fixupMaterial.SetFloat("_ClearAlpha", _clearAlpha ? 1 : 0);
				
				var tempRT = RenderTexture.GetTemporary(sharedTexture.width, sharedTexture.height,
					0, UnityEngine.RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);

				Graphics.Blit(temporaryTexture.Texture, tempRT, _fixupMaterial, 0);
				Graphics.CopyTexture(tempRT, sharedTexture);
				RenderTexture.ReleaseTemporary(tempRT);
			}
		}
		protected IEnumerator ProcessUpdateSharedTexture() {
			while (true) {
				yield return new WaitForEndOfFrame();
				UpdateSharedTexture();
			}
		}

		[System.Serializable]
        public class BoolEvent : UnityEngine.Events.UnityEvent<bool> { }
        [System.Serializable]
        public class RenderTextureEvent : UnityEngine.Events.UnityEvent<RenderTexture> { }
    }
}
