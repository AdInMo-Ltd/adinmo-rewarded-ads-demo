using UnityEngine;
using UnityEngine.UI;
using Adinmo.Flatbufs;


namespace Adinmo
{


    public delegate void EventCallback(AdinmoTexture t);

    ////////////////////////////////////////////////////////////////////////////////////
    public class AdinmoTexture : AdinmoReplace
    {


        // Implementation

        private EventCallback m_onReadyCallback = null;
        private GameObject m_debugBackground = null;
        private Image m_debugBackgroundImage = null;
        private GameObject m_debugForeground = null;
        private Image m_debugForegroundImage = null;
        private static bool s_bShowMessageFromServer = true;

        private const int BORDER_WIDTH = 3;
        ////////////////////////////////////////////////////////////////////////////////////
        void Start()
        {
            ShowMessageFromServer();
            Init();
        }


        ////////////////////////////////////////////////////////////////////////////////////
        // Allow the server to print a debug message to the editor window just once.
        // This message is usually a notification that the SDK is oboslete
        ////////////////////////////////////////////////////////////////////////////////////
        void ShowMessageFromServer()
        {
            if (s_bShowMessageFromServer)
            {
                s_bShowMessageFromServer = false;

                string message = AdinmoManager.GetMessageFromServer();
                if (message != null && message != "")
                    Debug.Log(message);
            }
        }


        ////////////////////////////////////////////////////////////////////////////////////



        /// <summary>
        /// Is the image component active and enabled
        /// </summary>
        /// <returns></returns>
        public override bool IsImageActive()
        {
            if (m_rawImage)
                return m_rawImage.isActiveAndEnabled;
            else
                return m_image.isActiveAndEnabled;
        }

        /// <summary>
        /// Method for what happens when a placement is initialised
        /// </summary>
        protected override void Init()
        {
            // Get the image component
            m_image = GetComponent<Image>();
            if (m_image != null)
                m_originalImage = m_image.gameObject;
            else
                m_originalImage = null;

            base.Init();
        }

        /// <summary>
        /// A Developer settable function to call when cycling has started on this placement
        /// </summary>
        /// <param name="pfn"></param>
        public void SetOnReadyCallback(EventCallback pfn)
        {
            m_onReadyCallback = pfn;
        }

        /// <summary>
        /// Called when the placement is showing something for the first time
        /// </summary>
        protected override void OnReady()
        {
            // Let the owner know that this is ready with the downloaded texture
            m_onReadyCallback?.Invoke(this);
        }

        /// <summary>
        /// Enabled the Sprite / Image component
        /// </summary>
        /// <param name="bEnabled"></param>
        protected override void SetRendererEnabled(bool bEnable)
        {
            if (TryGetComponent<SpriteRenderer>(out var sr))
                sr.enabled = bEnable;
            else
            {
                if (TryGetComponent<MeshRenderer>(out var mr))
                    mr.enabled = bEnable;

                else
                {
                    if (TryGetComponent<Image>(out var i))
                        i.enabled = bEnable;
                }
            }
        }


        /// <summary>
        /// Update the debugging object such that a white header indicates nothing, red indicates failed impression, green indicates successful impression
        /// </summary>
        /// <param name="c">The colour to set the header too</param>
        /// <param name="percentRemaining">The remaining percentage of the impression</param>
		public override void UpdateDebugging(Color c, float percentRemaining)
        {
            base.UpdateDebugging(c, percentRemaining);

            if (m_debugBackground != null && m_debugBackgroundImage != null)
            {
                DuplicateParentWidth(percentRemaining);
                m_debugBackgroundImage.color = c;
                m_debugForegroundImage.enabled = (c == Color.white);
            }
        }


        /// <summary>
        /// Initiate the debugging object
        /// </summary>
		public override void StartDebugging()
        {
            base.StartDebugging();

            // Quads and Sprites are handled by the base class
            if (!TryGetComponent<Image>(out var i))
                return;

            GameObject borderPrefab = (GameObject)Resources.Load("AdinmoDebugImage", typeof(GameObject));

            if (borderPrefab == null)
                return;

            m_debugBackground = Instantiate(borderPrefab, transform);
            m_debugBackground.name = "Debug Background";
            m_debugBackgroundImage = m_debugBackground.GetComponent<Image>();
            m_debugForeground = Instantiate(borderPrefab, transform);
            m_debugForeground.name = "Debug Foreground";
            m_debugForegroundImage = m_debugForeground.GetComponent<Image>();
            RectTransform fgRt = m_debugForeground.GetComponent<RectTransform>();
            fgRt.position += (Vector3.down + Vector3.right) * BORDER_WIDTH;

            DuplicateParentWidth(0);
        }

        protected void DuplicateParentWidth(float percentRemaining)
        {
            if (!TryGetComponent<RectTransform>(out var rt))
                return;

            if (!m_debugBackground.TryGetComponent<RectTransform>(out var bgRt))
                return;
            bgRt.sizeDelta = new Vector2(rt.rect.width, 20);
            if (!m_debugForeground.TryGetComponent<RectTransform>(out var fgRt))
                return;
            float fgWidth = (rt.rect.width - BORDER_WIDTH * 2) * percentRemaining;
            fgRt.sizeDelta = new Vector2(fgWidth, 20 - BORDER_WIDTH * 2);

        }

        /// <summary>
        /// Stop the debugging object
        /// </summary>
        public override void StopDebugging()
        {
            base.StopDebugging();

            if (m_debugBackground)
                Destroy(m_debugBackground);
        }


    }

}
